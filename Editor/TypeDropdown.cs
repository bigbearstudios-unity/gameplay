using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace BBUnity.Entities.Controllers.EditorTools {

    /// <summary>
    /// Searchable "pick a type" dropdown. Flat (no nested namespace folders to click through) —
    /// each entry is labelled "LastNamespaceSegment.TypeName" so same-named types in different
    /// namespaces — e.g. a Player and an Enemy GroundedIdleState — still read as unambiguous.
    /// Used for both state and condition type pickers.
    /// </summary>
    internal class TypeDropdown : AdvancedDropdown {
        private readonly List<Type> _types;
        private readonly Action<Type> _onTypeSelected;
        private readonly string _title;

        // AdvancedDropdownItem.id can't be trusted as a stable index: Unity's internal search
        // index recomputes/overwrites it independently of whatever's assigned in BuildRoot (seen
        // in practice — an id explicitly set to a small sequential int came back as an unrelated
        // large negative number in ItemSelected). Tracking the leaf item's Type by object
        // reference instead sidesteps that entirely.
        private readonly Dictionary<AdvancedDropdownItem, Type> _itemTypes = new Dictionary<AdvancedDropdownItem, Type>();

        public TypeDropdown(AdvancedDropdownState state, List<Type> types, Action<Type> onTypeSelected, string title) : base(state) {
            _types = types;
            _onTypeSelected = onTypeSelected;
            _title = title;
            minimumSize = new Vector2(350, 350);
        }

        protected override AdvancedDropdownItem BuildRoot() {
            _itemTypes.Clear();
            var root = new AdvancedDropdownItem(_title);

            foreach (Type type in _types) {
                var leaf = new AdvancedDropdownItem(BuildLabel(type));
                root.AddChild(leaf);
                _itemTypes[leaf] = type;
            }

            return root;
        }

        // Internal (not private): StateControllerInspector reuses this so a state's label in the
        // registered-states list matches the label it was picked under in this dropdown.
        internal static string BuildLabel(Type type) {
            var displayName = type.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
            if (displayName != null && !string.IsNullOrEmpty(displayName.DisplayName)) {
                return displayName.DisplayName;
            }

            string lastNamespaceSegment = GetLastNamespaceSegment(type);
            return lastNamespaceSegment != null ? $"{lastNamespaceSegment}.{type.Name}" : type.Name;
        }

        private static string GetLastNamespaceSegment(Type type) {
            if (string.IsNullOrEmpty(type.Namespace)) { return null; }

            string[] segments = type.Namespace.Split('.');
            return segments[segments.Length - 1];
        }

        protected override void ItemSelected(AdvancedDropdownItem item) {
            if (_itemTypes.TryGetValue(item, out Type type)) {
                _onTypeSelected(type);
            }
        }
    }
}
