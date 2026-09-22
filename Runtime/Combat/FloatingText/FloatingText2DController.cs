using UnityEngine;

namespace BBUnity.Combat.FloatingText {

    /// <summary>
    /// 2D-specific FloatingTextController — the only thing this dimension needs beyond the base
    /// is correct sorting against sprites, since world-space TextMesh/TextMeshPro both render via
    /// a MeshRenderer, which supports sortingLayerName/sortingOrder the same way SpriteRenderer
    /// does.
    /// </summary>
    public class FloatingText2DController : FloatingTextController {

        [SerializeField]
        private string _sortingLayerName = "Default";

        [SerializeField]
        private int _sortingOrder = 0;

        protected override void ConfigureInstanceRenderer(GameObject instance) {
            MeshRenderer meshRenderer = instance.GetComponent<MeshRenderer>();
            if (meshRenderer == null) { return; }

            meshRenderer.sortingLayerName = _sortingLayerName;
            meshRenderer.sortingOrder = _sortingOrder;
        }
    }
}
