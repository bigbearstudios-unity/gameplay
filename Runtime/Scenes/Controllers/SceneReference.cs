using UnityEngine;

namespace BBUnity.Scenes.Controllers {

    /// <summary>
    /// A scene picked from Build Settings via a dropdown (see SceneReferenceDrawer) rather than a
    /// free-typed string — stored as the scene's name, which is what SceneManager.LoadScene(string)
    /// needs directly. Renaming a scene in Build Settings means re-picking it here; there's no
    /// GUID-based resolution.
    /// </summary>
    [System.Serializable]
    public class SceneReference {

        [SerializeField]
        private string _sceneName;

        public string SceneName {
            get { return _sceneName; }
        }

        public bool IsSet {
            get { return !string.IsNullOrEmpty(_sceneName); }
        }
    }
}
