using UnityEngine;


namespace MyUtils {
    internal static class MyRandom {
        public static Color GetColorFormGradient(Gradient g) {
            return g.Evaluate(Random.Range(0f, 1f));
        }
        public static T GetFromArray<T>(T[] a) {
            int random = Random.Range(0, a.Length);
            return a[random];
        }
    }




}

