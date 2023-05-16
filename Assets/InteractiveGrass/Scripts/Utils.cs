using UnityEngine;

namespace InteractiveGrass.Scripts
{
    public static class Utils
    {
        public static Vector3 CalculateRandomTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            var randomX = Random.Range(0, 1f);
            var randomY = Random.Range(0, 1 - randomX);
            var randomZ = 1 - randomX - randomY;
            return v1 * randomX + v2 * randomY + v3 * randomZ;
        }
        public static Vector3 CalculateTriangleNormal(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            return Vector3.Cross(v2 - v1, v3 - v1);
        }
    }
}