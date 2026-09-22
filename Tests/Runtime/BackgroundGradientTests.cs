using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

using BBUnity.TestSupport;
using BBUnity.UI.Components;

namespace BBUnity.UI.Tests {
    public class BackgroundGradientTests {

        private Image _image;
        private BackgroundGradient _gradient;

        [SetUp]
        public void SetUp() {
            _image = TestUtilities.CreateGameObject<Image>(name: "Gradient Under Test");

            RectTransform rectTransform = _image.rectTransform;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(100, 50); // rect: min=(-50,-25), size=(100,50)

            _gradient = _image.gameObject.AddComponent<BackgroundGradient>();
            _gradient.Color1 = Color.red;   // top (local y = 1)
            _gradient.Color2 = Color.blue;  // bottom (local y = 0)
        }

        [TearDown]
        public void TearDown() {
            TestUtilities.DestroyRootObjectsInScene();
        }

        private VertexHelper BuildQuad(Color startingColor) {
            var vh = new VertexHelper();
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = startingColor;

            vertex.position = new Vector3(-50, -25, 0); vh.AddVert(vertex); // 0: bottom-left
            vertex.position = new Vector3(50, -25, 0); vh.AddVert(vertex);  // 1: bottom-right
            vertex.position = new Vector3(50, 25, 0); vh.AddVert(vertex);   // 2: top-right
            vertex.position = new Vector3(-50, 25, 0); vh.AddVert(vertex);  // 3: top-left

            return vh;
        }

        private UIVertex GetVertex(VertexHelper vh, int index) {
            UIVertex vertex = default;
            vh.PopulateUIVertex(ref vertex, index);
            return vertex;
        }

        private static void AssertColorsApproximatelyEqual(Color expected, Color actual, float tolerance = 0.01f) {
            Assert.AreEqual(expected.r, actual.r, tolerance, "r");
            Assert.AreEqual(expected.g, actual.g, tolerance, "g");
            Assert.AreEqual(expected.b, actual.b, tolerance, "b");
            Assert.AreEqual(expected.a, actual.a, tolerance, "a");
        }

        [Test]
        public void ModifyMesh_BottomVertex_BecomesColor2() {
            VertexHelper vh = BuildQuad(Color.white);

            _gradient.ModifyMesh(vh);

            AssertColorsApproximatelyEqual(Color.blue, GetVertex(vh, 0).color);
        }

        [Test]
        public void ModifyMesh_TopVertex_BecomesColor1() {
            VertexHelper vh = BuildQuad(Color.white);

            _gradient.ModifyMesh(vh);

            AssertColorsApproximatelyEqual(Color.red, GetVertex(vh, 2).color);
        }

        [Test]
        public void ModifyMesh_MultipliesIntoExistingVertexColor_RatherThanReplacingIt() {
            // Documents the intentional design (see class doc comment): the gradient composes
            // with the Graphic's own tint instead of overwriting it.
            var tint = new Color(1f, 1f, 1f, 0.5f); // half-alpha white tint, common for a faded Image
            VertexHelper vh = BuildQuad(tint);

            _gradient.ModifyMesh(vh);

            Color result = GetVertex(vh, 0).color; // bottom vertex, gradient contribution = Color2 (blue)
            Color expected = tint * Color.blue;
            AssertColorsApproximatelyEqual(expected, result);
        }

        [Test]
        public void ModifyMesh_AngleNinety_RotatesGradientToHorizontal() {
            _gradient.Angle = 90.0f;
            VertexHelper vh = BuildQuad(Color.white);

            _gradient.ModifyMesh(vh);

            // At angle 90 the sample axis runs left→right: left edge should read Color2 (blue),
            // right edge should read Color1 (red) — vertical position should no longer matter.
            AssertColorsApproximatelyEqual(Color.blue, GetVertex(vh, 0).color); // bottom-left
            AssertColorsApproximatelyEqual(Color.blue, GetVertex(vh, 3).color); // top-left
            AssertColorsApproximatelyEqual(Color.red, GetVertex(vh, 1).color);  // bottom-right
            AssertColorsApproximatelyEqual(Color.red, GetVertex(vh, 2).color);  // top-right
        }

        [Test]
        public void Color1Setter_MarksGraphicVerticesDirty() {
            // No direct way to observe CanvasRenderer dirtying without a live Canvas, so this
            // just guards against the setter throwing when graphic is already resolved.
            Assert.DoesNotThrow(() => _gradient.Color1 = Color.green);
        }
    }
}
