using UnityEngine;

namespace MyUtils {
    public static class MeshUtils {
        private static Quaternion[] _cachedQuaternionEulerArr;
        private static void CacheQuaternionEuler() {
            if (_cachedQuaternionEulerArr != null) return;
            _cachedQuaternionEulerArr = new Quaternion[360];
            for (int i = 0; i < 360; i++) {
                _cachedQuaternionEulerArr[i] = Quaternion.Euler(0, 360 - i, 0);
            }
        }
        public static Quaternion GetQuaternionEuler(int rot) {
            rot %= 360;
            if (rot < 0) rot += 360;
            if (_cachedQuaternionEulerArr == null) CacheQuaternionEuler();
            return _cachedQuaternionEulerArr[rot];
        }
        public static void AddToMesh(Vector3[] vert, Vector2[] uvs, int[] triangles, int i, Vector3 pos, Vector3 size, Vector2 uv00, Vector2 uv11) {
            int vI = i * 4;
            int vI0 = vI;
            int vI1 = vI + 1;
            int vI2 = vI + 2;
            int vI3 = vI + 3;

            size *= 0.5f;
            vert[vI0] = pos + GetQuaternionEuler(-270) * size;
            vert[vI1] = pos + GetQuaternionEuler(-180) * size;
            vert[vI2] = pos + GetQuaternionEuler(-90) * size;
            vert[vI3] = pos + GetQuaternionEuler(0) * size;

            uvs[vI0] = new Vector2(uv00.x, uv11.y);
            uvs[vI1] = new Vector2(uv00.x, uv00.y);
            uvs[vI2] = new Vector2(uv11.x, uv00.y);
            uvs[vI3] = new Vector2(uv11.x, uv11.y);

            int tI = i * 6;

            triangles[tI + 0] = vI0;
            triangles[tI + 1] = vI3;
            triangles[tI + 2] = vI1;

            triangles[tI + 3] = vI1;
            triangles[tI + 4] = vI3;
            triangles[tI + 5] = vI2;

        }
    }
}

