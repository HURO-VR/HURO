using UnityEngine;

namespace Utility
{
    public static class Utils
    {
        /// <summary>
        /// Checks if a transform's XZ position is within a 0.5 x 0.5 box around a target point.
        /// </summary>
        /// <param name="t">Transform to check.</param>
        /// <param name="center">Center of the box in world space.</param>
        /// <returns>True if inside the box, false otherwise.</returns>
        public static bool IsInXZBox(Transform t, Vector3 center, float boxSize)
        {
            Vector3 pos = t.position;
            return Mathf.Abs(pos.x - center.x) <= boxSize && Mathf.Abs(pos.z - center.z) <= boxSize;
        }
    }
}