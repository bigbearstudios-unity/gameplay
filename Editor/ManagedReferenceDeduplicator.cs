using System.Collections.Generic;

using UnityEditor;

namespace BBUnity.Entities.Controllers.EditorTools {

    /// <summary>
    /// Unity's default array "+" and "Duplicate Array Element" clone a new element from the
    /// previous one — for plain fields (int, string, ObjectReference, ...) that's a normal value
    /// copy, but for a [SerializeReference] field nested anywhere inside that element (e.g.
    /// DamageEntry.DamageValue, ReductionEntry.Reduction) it clones the *managed reference id*
    /// rather than the object, so the two elements end up editing the exact same underlying
    /// instance.
    ///
    /// Call Deduplicate(root) after drawing a list that might contain nested [SerializeReference]
    /// fields (root can be the whole list property) — it walks everything nested under it and
    /// nulls out every occurrence of a managed reference id after its first, so nothing can stay
    /// aliased. This both prevents new aliasing (a fresh duplicate's shared field resets to "none"
    /// rather than silently sharing) and repairs data that's already aliased from before this fix
    /// existed, since it runs every time the Inspector draws.
    /// </summary>
    internal static class ManagedReferenceDeduplicator {

        public static void Deduplicate(SerializedProperty root) {
            var seenIds = new HashSet<long>();
            bool changed = false;

            SerializedProperty iterator = root.Copy();
            SerializedProperty end = iterator.GetEndProperty();

            while (iterator.NextVisible(true) && !SerializedProperty.EqualContents(iterator, end)) {
                if (iterator.propertyType != SerializedPropertyType.ManagedReference) { continue; }
                if (iterator.managedReferenceValue == null) { continue; }

                if (!seenIds.Add(iterator.managedReferenceId)) {
                    iterator.managedReferenceValue = null;
                    changed = true;
                }
            }

            if (changed) {
                root.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
