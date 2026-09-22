using UnityEngine;

namespace BBUnity.UI.Utilities {

    /// <summary>
    /// Math helpers for building an axis-agnostic linear gradient over a UI Graphic's local
    /// rect space. See <see cref="LocalPositionMatrix"/> for how a rotation angle and a rect
    /// combine into the transform used to map a vertex position to normalized [0, 1] gradient
    /// space.
    /// </summary>
    public static class ColorGradient {

        /// <summary>
        /// A 2x3 affine transform (2x2 linear part + translation) used to map a local vertex
        /// position into normalized gradient space. Its multiplication operator applies the
        /// standard 2D rotation matrix form [[cos, -sin], [sin, cos]] — note that the minus
        /// sign lives here, not in <see cref="LocalPositionMatrix"/>'s m01, which is
        /// deliberate: m01/m11 are only a valid rotation once combined with this operator's
        /// sign convention. Do not "simplify" one without the other.
        /// </summary>
        public struct Matrix2x3 {
            public float m00, m01, m02, m10, m11, m12;
            public Matrix2x3(float m00, float m01, float m02, float m10, float m11, float m12) {
                this.m00 = m00;
                this.m01 = m01;
                this.m02 = m02;
                this.m10 = m10;
                this.m11 = m11;
                this.m12 = m12;
            }

            public static Vector2 operator *(Matrix2x3 m, Vector2 v) {
                float x = (m.m00 * v.x) - (m.m01 * v.y) + m.m02;
                float y = (m.m10 * v.x) + (m.m11 * v.y) + m.m12;
                return new Vector2(x, y);
            }
        }

        /// <summary>
        /// Builds the transform that maps a vertex's local position (in the same space as
        /// <paramref name="rect"/>, i.e. RectTransform.rect / UIVertex.position) into normalized
        /// [0, 1] gradient space along <paramref name="dir"/>, regardless of the rect's pivot or
        /// size. For dir = (1, 0) (angle 0, see <see cref="RotationDir"/>) this is a plain
        /// per-axis normalization; non-cardinal directions additionally rotate the sampled axis.
        /// </summary>
        public static Matrix2x3 LocalPositionMatrix(Rect rect, Vector2 dir) {
            float cos = dir.x;
            float sin = dir.y;
            Vector2 rectMin = rect.min;
            Vector2 rectSize = rect.size;
            float c = 0.5f;
            float ax = rectMin.x / rectSize.x + c;
            float ay = rectMin.y / rectSize.y + c;
            float m00 = cos / rectSize.x;
            float m01 = sin / rectSize.y;
            float m02 = -(ax * cos - ay * sin - c);
            float m10 = sin / rectSize.x;
            float m11 = cos / rectSize.y;
            float m12 = -(ax * sin + ay * cos - c);
            return new Matrix2x3(m00, m01, m02, m10, m11, m12);
        }

        /// <summary>
        /// Returns the (cos, sin) unit direction for a gradient rotated by <paramref name="angle"/>
        /// degrees. 0 degrees is the vertical (bottom-to-top) direction used by
        /// <see cref="LocalPositionMatrix"/>.
        /// </summary>
        public static Vector2 RotationDir(float angle) {
            float angleRad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);
            return new Vector2(cos, sin);
        }

        /// <summary>
        /// Adjusts a (cos, sin) direction so a rotated gradient looks visually correct on a
        /// non-square rect, rather than skewing towards the rect's longer axis. A no-op for the
        /// cardinal directions (angle a multiple of 90 degrees), since one component is always
        /// zero there.
        /// </summary>
        public static Vector2 CompensateAspectRatio(Rect rect, Vector2 dir) {
            float ratio = rect.height / rect.width;
            dir.x *= ratio;
            return dir.normalized;
        }
    }
}
