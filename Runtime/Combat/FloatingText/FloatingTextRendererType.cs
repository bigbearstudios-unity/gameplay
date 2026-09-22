namespace BBUnity.Combat.FloatingText {

    /// <summary>
    /// Which underlying text component FloatingTextController spawns instances with. Unity uses
    /// the engine's built-in world-space TextMesh; TextMeshPro uses the non-UGUI, world-space
    /// TMPro.TextMeshPro (not TextMeshProUGUI, which needs a Canvas) — both are simple
    /// MeshRenderer-based components positionable directly via Transform, which is what makes
    /// them a matching pair for floating world-space text.
    /// </summary>
    public enum FloatingTextRendererType {
        Unity,
        TextMeshPro
    }
}
