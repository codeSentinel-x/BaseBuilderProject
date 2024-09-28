using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridSystem))]
public class GridSystemEditor : Editor {
    public override void OnInspectorGUI() {

        GridSystem worldGen = (GridSystem)target;

        if (DrawDefaultInspector()) {
            //Maybe later
        }

        GUIStyle buttonStyle = new(GUI.skin.button) {
            richText = true
        };

        if (GUILayout.Button("<b>\nGenerate world\n</b>", buttonStyle)) {
            worldGen.GenerateWorld();
        }
    }
}
