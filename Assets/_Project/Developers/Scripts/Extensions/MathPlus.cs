using UnityEngine;

namespace Extensions {
    public static class MathPlus {
        /// <summary>
        /// Calculates a point around a given axis at a specified distance and angle.
        /// </summary>
        /// <param name="axis">The axis of rotation (e.g., a normalized vector).</param>
        /// <param name="centerPoint">The center point around which the rotation occurs.</param>
        /// <param name="distanceFromCenter">The distance from the center point to the calculated point.</param>
        /// <param name="zeroDirection">The reference direction for the zero angle (e.g., forward direction).</param>
        /// <param name="angle">The angle (in degrees) to rotate around the axis.</param>
        /// <returns>
        /// A <see cref="Vector3"/> representing the calculated point at the specified angle and distance
        /// from the center point, rotated around the given axis.
        /// </returns>
        public static Vector3 GetPointAroundAxis(Vector3 axis, Vector3 centerPoint, float distanceFromCenter,
            Vector3 zeroDirection, float angle) =>
                centerPoint + Quaternion.AngleAxis(angle, axis) * (zeroDirection.normalized * distanceFromCenter);
    }
}