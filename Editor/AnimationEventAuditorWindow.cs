using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEditor;

namespace BBUnity.Entities.Controllers.EditorTools {

    /// <summary>
    /// Scans every AnimationClip in the project for embedded Animation Events and reports
    /// whether each one's function name actually resolves to a callable method anywhere in the
    /// project — Unity only surfaces a mismatch ("has no receiver") as a console warning at
    /// runtime, once that exact clip happens to play, which makes a rename/refactor (e.g.
    /// StateController's SetAnimationEventBooleanTrue -> SetEventBooleanTrue) easy to miss across
    /// dozens of clips. This lets you find every broken one up front instead.
    ///
    /// The check is necessarily best-effort: an Animation Event is dispatched (like SendMessage)
    /// to whatever component ends up on the Animator's GameObject at runtime, which this window
    /// has no way to know for a clip in isolation. So "OK" means "a compatible method exists
    /// somewhere in the project", not "this exact clip is correctly wired on its target" — but
    /// "Missing" is unambiguous: no such method exists anywhere, so it WILL fail at runtime.
    /// </summary>
    public class AnimationEventAuditorWindow : EditorWindow {

        private enum Status { Ok, SignatureMismatch, Missing }

        private class EventRow {
            public AnimationClip Clip;
            public AnimationEvent Event;
            public Status Status;
            public string Detail;
        }

        private static readonly Type[] SupportedParameterTypes = { typeof(string), typeof(float), typeof(int) };

        private static Dictionary<string, List<MethodInfo>> _methodsByName;

        private List<EventRow> _rows = new List<EventRow>();
        private Vector2 _scrollPosition;
        private bool _showOnlyProblems = true;
        private bool _includeArchived = false;

        [MenuItem("Window/BBUnity/Animation Event Auditor")]
        private static void Open() {
            GetWindow<AnimationEventAuditorWindow>("Animation Event Auditor");
        }

        private void OnEnable() {
            Scan();
        }

        private void OnGUI() {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
                if (GUILayout.Button("Rescan", EditorStyles.toolbarButton, GUILayout.Width(70))) {
                    Scan();
                }

                GUILayout.FlexibleSpace();
                _showOnlyProblems = GUILayout.Toggle(_showOnlyProblems, "Only show problems", EditorStyles.toolbarButton);
                _includeArchived = GUILayout.Toggle(_includeArchived, "Include _Old_/_Archive", EditorStyles.toolbarButton);
            }

            var visibleClips = _rows
                .GroupBy(row => row.Clip)
                .Where(group => _includeArchived || !IsArchived(group.Key))
                .Where(group => !_showOnlyProblems || group.Any(row => row.Status != Status.Ok))
                .OrderBy(group => AssetDatabase.GetAssetPath(group.Key))
                .ToList();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{visibleClips.Count} clip(s) with events shown, {_rows.Count(r => r.Status != Status.Ok)} problem event(s) total.", EditorStyles.miniLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var group in visibleClips) {
                DrawClip(group.Key, group.ToList());
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawClip(AnimationClip clip, List<EventRow> rows) {
            bool hasProblem = rows.Any(row => row.Status != Status.Ok);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField((hasProblem ? "⚠ " : "✓ ") + clip.name, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Ping", GUILayout.Width(50))) {
                    EditorGUIUtility.PingObject(clip);
                }
            }

            foreach (EventRow row in rows) {
                DrawEvent(row);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEvent(EventRow row) {
            string icon = row.Status == Status.Ok ? "✓" : row.Status == Status.SignatureMismatch ? "?" : "✗";
            string dataLabel = string.IsNullOrEmpty(row.Event.stringParameter) ? "(no data)" : row.Event.stringParameter;

            EditorGUILayout.LabelField($"    {icon}  t={row.Event.time:0.###}  {row.Event.functionName}(\"{dataLabel}\")");

            if (row.Status != Status.Ok) {
                EditorGUILayout.HelpBox(row.Detail, row.Status == Status.Missing ? MessageType.Error : MessageType.Warning);
            }
        }

        private static bool IsArchived(AnimationClip clip) {
            string path = AssetDatabase.GetAssetPath(clip);
            return path.Contains("_Old_") || path.Contains("_Archive");
        }

        private void Scan() {
            _methodsByName = null;
            _rows = new List<EventRow>();

            foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip")) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null) { continue; }

                foreach (AnimationEvent animationEvent in AnimationUtility.GetAnimationEvents(clip)) {
                    if (string.IsNullOrEmpty(animationEvent.functionName)) { continue; }

                    _rows.Add(BuildRow(clip, animationEvent));
                }
            }

            Repaint();
        }

        private static EventRow BuildRow(AnimationClip clip, AnimationEvent animationEvent) {
            List<MethodInfo> candidates = FindMethodsByName(animationEvent.functionName);

            if (candidates.Count == 0) {
                return new EventRow {
                    Clip = clip,
                    Event = animationEvent,
                    Status = Status.Missing,
                    Detail = $"No method named '{animationEvent.functionName}' exists on any component in the project — this will log \"has no receiver\" and do nothing at runtime."
                };
            }

            if (candidates.Any(IsCompatibleSignature)) {
                return new EventRow { Clip = clip, Event = animationEvent, Status = Status.Ok };
            }

            string declaringTypes = string.Join(", ", candidates.Select(m => m.DeclaringType.Name).Distinct());
            return new EventRow {
                Clip = clip,
                Event = animationEvent,
                Status = Status.SignatureMismatch,
                Detail = $"Found '{animationEvent.functionName}' on {declaringTypes}, but its parameters don't match any signature an Animation Event can call (none, string, float or int)."
            };
        }

        private static bool IsCompatibleSignature(MethodInfo method) {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 0) { return true; }
            if (parameters.Length != 1) { return false; }

            Type parameterType = parameters[0].ParameterType;
            return SupportedParameterTypes.Contains(parameterType) || typeof(UnityEngine.Object).IsAssignableFrom(parameterType);
        }

        // Cached for the lifetime of one Scan() — an Animation Event is dispatched by name to
        // whatever component is on the receiving GameObject at runtime, so this deliberately
        // searches every UnityEngine.Object-derived type in every loaded assembly rather than
        // trying to guess which component a given clip is meant to target.
        private static List<MethodInfo> FindMethodsByName(string functionName) {
            _methodsByName ??= BuildMethodIndex();

            return _methodsByName.TryGetValue(functionName, out List<MethodInfo> methods) ? methods : new List<MethodInfo>();
        }

        private static Dictionary<string, List<MethodInfo>> BuildMethodIndex() {
            var index = new Dictionary<string, List<MethodInfo>>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                Type[] types;
                try {
                    types = assembly.GetTypes();
                } catch (ReflectionTypeLoadException e) {
                    types = e.Types.Where(t => t != null).ToArray();
                }

                foreach (Type type in types) {
                    if (!typeof(UnityEngine.Object).IsAssignableFrom(type)) { continue; }

                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
                        if (!index.TryGetValue(method.Name, out List<MethodInfo> methods)) {
                            methods = new List<MethodInfo>();
                            index[method.Name] = methods;
                        }

                        methods.Add(method);
                    }
                }
            }

            return index;
        }
    }
}
