using UnityEngine;

namespace BBUnity.Combat.FloatingText {

    /// <summary>
    /// The handful of properties FloatingTextInstance needs regardless of which underlying text
    /// component is actually doing the rendering — see UnityFloatingTextRenderer and
    /// TextMeshProFloatingTextRenderer for the two adapters.
    /// </summary>
    internal interface IFloatingTextRenderer {
        string Text { get; set; }
        Color Color { get; set; }
        float Size { get; set; }
    }
}
