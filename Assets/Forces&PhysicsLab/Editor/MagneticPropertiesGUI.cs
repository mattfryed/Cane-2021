using UnityEngine;
using UnityEditor;

[SerializeField]
[CanEditMultipleObjects]

[CustomEditor(typeof(MagneticProperties))]

public class MagneticPropertiesGUI : Editor {

    SerializedProperty CanBeAttracted;

    private void OnEnable()
    {
        CanBeAttracted = serializedObject.FindProperty("CanBeAttracted");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        MagneticProperties magneticproperties = (MagneticProperties)target;

        
        EditorGUILayout.BeginVertical(new GUIStyle("box"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(CanBeAttracted, new GUIContent("Can be attracted", "Specifies wether this object can be attracted by Magnets."));
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
