using NUnit.Framework;
using UnityEngine;

using BBUnity.TestSupport;

namespace BBUnity.UI.Utilities.Tests {
    public class ColorGradientTests {

        [Test]
        public void RotationDir_Zero_ReturnsRightVector() {
            Vector2 dir = ColorGradient.RotationDir(0.0f);

            UnityAssert.AreEqual(new Vector2(1.0f, 0.0f), dir);
        }

        [Test]
        public void RotationDir_Ninety_ReturnsUpVector() {
            Vector2 dir = ColorGradient.RotationDir(90.0f);

            UnityAssert.AreEqual(new Vector2(0.0f, 1.0f), dir, tolerance: 0.0001f);
        }

        [Test]
        public void RotationDir_OneEighty_ReturnsLeftVector() {
            Vector2 dir = ColorGradient.RotationDir(180.0f);

            UnityAssert.AreEqual(new Vector2(-1.0f, 0.0f), dir, tolerance: 0.0001f);
        }

        [Test]
        public void RotationDir_IsAlwaysUnitLength() {
            Vector2 dir = ColorGradient.RotationDir(37.0f);

            Assert.AreEqual(1.0f, dir.magnitude, 0.0001f);
        }

        [Test]
        public void CompensateAspectRatio_CardinalDirection_IsUnaffected() {
            var rect = new Rect(0, 0, 200, 100);
            Vector2 dir = ColorGradient.RotationDir(0.0f); // (1, 0)

            Vector2 compensated = ColorGradient.CompensateAspectRatio(rect, dir);

            UnityAssert.AreEqual(new Vector2(1.0f, 0.0f), compensated, tolerance: 0.0001f);
        }

        [Test]
        public void CompensateAspectRatio_DiagonalOnNonSquareRect_SkewsTowardsShorterAxis() {
            var rect = new Rect(0, 0, 200, 100); // wider than tall
            Vector2 dir = ColorGradient.RotationDir(45.0f);

            Vector2 compensated = ColorGradient.CompensateAspectRatio(rect, dir);

            Assert.AreEqual(1.0f, compensated.magnitude, 0.0001f, "Must remain a unit vector");
            Assert.Less(compensated.x, dir.x, "x-component should shrink relative to y on a wider-than-tall rect");
        }

        [Test]
        public void CompensateAspectRatio_SquareRect_IsUnaffected() {
            var rect = new Rect(0, 0, 100, 100);
            Vector2 dir = ColorGradient.RotationDir(45.0f);

            Vector2 compensated = ColorGradient.CompensateAspectRatio(rect, dir);

            UnityAssert.AreEqual(dir, compensated, tolerance: 0.0001f);
        }

        [Test]
        public void LocalPositionMatrix_AngleZero_CenteredPivot_MapsCornersToUnitSquare() {
            var rect = new Rect(-50, -25, 100, 50); // min=(-50,-25), size=(100,50): centered pivot
            Vector2 dir = ColorGradient.RotationDir(0.0f);
            ColorGradient.Matrix2x3 matrix = ColorGradient.LocalPositionMatrix(rect, dir);

            UnityAssert.AreEqual(new Vector2(0, 0), matrix * new Vector2(-50, -25), tolerance: 0.0001f); // bottom-left
            UnityAssert.AreEqual(new Vector2(1, 1), matrix * new Vector2(50, 25), tolerance: 0.0001f);   // top-right
            UnityAssert.AreEqual(new Vector2(1, 0), matrix * new Vector2(50, -25), tolerance: 0.0001f);  // bottom-right
            UnityAssert.AreEqual(new Vector2(0, 1), matrix * new Vector2(-50, 25), tolerance: 0.0001f);  // top-left
        }

        [Test]
        public void LocalPositionMatrix_AngleZero_BottomLeftPivot_MapsCornersToUnitSquare() {
            var rect = new Rect(0, 0, 100, 50); // min=(0,0): bottom-left pivot
            Vector2 dir = ColorGradient.RotationDir(0.0f);
            ColorGradient.Matrix2x3 matrix = ColorGradient.LocalPositionMatrix(rect, dir);

            UnityAssert.AreEqual(new Vector2(0, 0), matrix * new Vector2(0, 0), tolerance: 0.0001f);
            UnityAssert.AreEqual(new Vector2(1, 1), matrix * new Vector2(100, 50), tolerance: 0.0001f);
        }

        [Test]
        public void LocalPositionMatrix_NinetyDegrees_RotatesSampleAxisFromVerticalToHorizontal() {
            var rect = new Rect(-50, -25, 100, 50);
            Vector2 dir = ColorGradient.RotationDir(90.0f);
            ColorGradient.Matrix2x3 matrix = ColorGradient.LocalPositionMatrix(rect, dir);

            // At angle 0 the gradient's y-axis runs bottom→top; at 90 degrees it should instead
            // run left→right, i.e. the *x* world axis now drives the interpolation factor.
            UnityAssert.AreEqual(new Vector2(0, 0), matrix * new Vector2(-50, 25), tolerance: 0.0001f);  // top-left
            UnityAssert.AreEqual(new Vector2(1, 1), matrix * new Vector2(50, -25), tolerance: 0.0001f);  // bottom-right
            UnityAssert.AreEqual(new Vector2(1, 0), matrix * new Vector2(-50, -25), tolerance: 0.0001f); // bottom-left
            UnityAssert.AreEqual(new Vector2(0, 1), matrix * new Vector2(50, 25), tolerance: 0.0001f);   // top-right
        }
    }
}
