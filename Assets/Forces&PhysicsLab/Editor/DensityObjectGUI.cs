using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


[SerializeField]
[CanEditMultipleObjects]
[CustomEditor(typeof(DensityObject))]
class DensityObjectGUI : Editor
{
    private void Awake()
    {
        DensityObject densityobject = (DensityObject)target;
        densityobject.rBody = densityobject.gameObject.GetComponent<Rigidbody>();
    }

    SerializedProperty Density;

    private void OnEnable()
    {
        Density = serializedObject.FindProperty("Density");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DensityObject densityobject = (DensityObject)target;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(Density, new GUIContent("Density", "The Density of the object's material."));
        EditorGUILayout.Space();
        if (!densityobject.GetComponent<Collider>())
        {
            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField("This Density object has no Collider.");
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }


}


