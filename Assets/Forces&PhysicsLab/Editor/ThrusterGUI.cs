using UnityEngine;
using UnityEditor;

[SerializeField]
[CanEditMultipleObjects]
[CustomEditor(typeof(Thruster))]
class ThrusterGUI : Editor
{
    SerializedProperty Thrust;
    SerializedProperty Enabled;
    SerializedProperty CurPower;
    SerializedProperty BoundRigidBody;
    SerializedProperty ThrustForce;
    SerializedProperty hoverHeight;
    SerializedProperty hoverDamp;
    SerializedProperty Forcemode;

    private void OnEnable()
    {
        Thrust = serializedObject.FindProperty("Thrust");
        Enabled = serializedObject.FindProperty("Enabled");
        CurPower = serializedObject.FindProperty("CurPower");
        BoundRigidBody = serializedObject.FindProperty("BoundRigidBody");
        ThrustForce = serializedObject.FindProperty("ThrustForce");
        hoverHeight = serializedObject.FindProperty("hoverHeight");
        hoverDamp = serializedObject.FindProperty("hoverDamp");
        Forcemode = serializedObject.FindProperty("Forcemode");

    }

    void OnSceneGUI()
    {
        Thruster thruster = (Thruster)target;
        if (thruster == null)
        {
            return;
        }
        float size = 1f;

            if (thruster.Thrust == Thruster.ThrustMode.Hover)
            {
                Handles.color = new Color(1, 0, 0, 0.5f);
                Handles.ConeHandleCap(0, (thruster.transform.position + (thruster.transform.forward * thruster.hoverHeight) / 2), thruster.transform.rotation, thruster.hoverHeight, EventType.Repaint);
                Handles.color = new Color(1, 0, 0, 1f);
                Handles.DrawWireDisc((thruster.transform.position + thruster.transform.forward * thruster.hoverHeight), thruster.transform.forward, size);
            }
            else
            {
                Handles.color = new Color(1, 0, 0, 0.5f);
                size = Mathf.Clamp(thruster.ThrustForce / 2, 0.2f, 10);
                Handles.ConeHandleCap(0, (thruster.transform.position + (thruster.transform.forward * size) / 2), thruster.transform.rotation, size, EventType.Repaint);
            }
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Thruster thruster = (Thruster)target;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(Thrust,new GUIContent("Mode", "Rocket Power: Adds a force to the bound rigidBody. Hover: Adds an amount of force to the bound rigidBody to make it hover above a  surface."));
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(Enabled,new GUIContent("Enabled", "Specify wether the Thruster is enabled."));
        EditorGUILayout.PropertyField(CurPower,new GUIContent("Current Power %", "The current amount of power in percentages."));
        EditorGUILayout.PropertyField(BoundRigidBody, new GUIContent("Bound RigidBody", "The rigidBody to wich the forces of this thruster are applied. "));
        EditorGUILayout.PropertyField(ThrustForce,new GUIContent("Force", "How much force will be applied. "));
        if (thruster.Thrust == Thruster.ThrustMode.Hover)
        {
           EditorGUILayout.Space();
           EditorGUILayout.PropertyField(hoverHeight,new GUIContent("Hover Height", "How high above a surface should it hover?"));
           EditorGUILayout.PropertyField(hoverDamp,new GUIContent("Hover Pressure Damp", "The force falloff. The closer the bound RigidBody is to the desired hover height. The more the force is decreased. This value specifies how much that decrease is applied. A value of 1 to make the force exactly 0 when the object is extactly on the hover height."));
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(Forcemode,new GUIContent("Force Mode", "Use ForceMode to specify how to apply a force."));
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }


}
