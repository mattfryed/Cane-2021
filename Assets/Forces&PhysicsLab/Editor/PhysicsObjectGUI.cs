using UnityEngine;
using UnityEditor;

[SerializeField]
[CanEditMultipleObjects]
[CustomEditor(typeof(PhysicsObject))]
class PhysicsObjectLabelHandle : Editor
{
    SerializedProperty CanBeAttracted;
    SerializedProperty AlignToForce;
    SerializedProperty AlignStrength;
    SerializedProperty AlignLimit;
    SerializedProperty EnableForceProcessing;
    SerializedProperty centerOfMass;

    private void Awake()
    {
        PhysicsObject com = (PhysicsObject)target;
        com.rBody = com.GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        CanBeAttracted = serializedObject.FindProperty("CanBeAttracted");
        AlignToForce = serializedObject.FindProperty("AlignToForce");
        AlignStrength = serializedObject.FindProperty("AlignStrength");
        AlignLimit = serializedObject.FindProperty("AlignLimit");
        EnableForceProcessing = serializedObject.FindProperty("EnableForceProcessing");
        centerOfMass = serializedObject.FindProperty("centerOfMass");

    }


    void OnSceneGUI()
    {
        PhysicsObject com = (PhysicsObject)target;
        if (com == null)
        {
            return;
        }

        Vector3 cm = com.transform.TransformDirection(com.centerOfMass);

        Handles.color = new Color(0, 0, 0, 0.5f);
        Handles.SphereHandleCap(0, com.transform.position + cm, Quaternion.identity, 1, EventType.Repaint);
        Handles.Label(com.transform.position + cm, "Center of mass");

        //Position Handle
        Handles.matrix = Matrix4x4.TRS(com.transform.position, com.transform.rotation, new Vector3(0.5f,0.5f, 0.5f));

        EditorGUI.BeginChangeCheck();
        Vector3 newTargetPosition = Handles.PositionHandle(com.centerOfMass*2, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(com, "Change Center of mass");
            com.centerOfMass = newTargetPosition / 2;
        }
        Handles.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 1, 1));

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        PhysicsObject com = (PhysicsObject)target;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(CanBeAttracted,new GUIContent("Can be attracted", "Specifies wether this object can be attracted by GravityObjects."));
        EditorGUILayout.PropertyField(AlignToForce, new GUIContent("Align to Force", "If true, this will rotate itself to point with it's Center of Mass towards the gravity that is attracting it. (For example: This can be used to make a player stand straight up in a spherical world.)"));
        if (com.AlignToForce)
        {
            EditorGUILayout.BeginVertical(new GUIStyle("box"));
            EditorGUILayout.PropertyField(AlignStrength,new GUIContent("Align Strength", "The strength of the aligning force."));
            EditorGUILayout.PropertyField(AlignLimit,new GUIContent("Align Limit", "How much the objects rotation may differ from being aligned to the force."));
            EditorGUILayout.EndVertical();

        }
        EditorGUILayout.PropertyField(EnableForceProcessing,new GUIContent("Enable Force Processing", "When enabled and multiple forces apply to this object. The object will only apply the highest priority force. And discard the others."));

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(centerOfMass,new GUIContent("Center of Mass:", "The relative position of the Center of Mass."));
        if (EditorGUI.EndChangeCheck())
        {
            com.SetCenterOfMass(com.centerOfMass);
        }
        // 
        if (GUILayout.Button("Auto center of mass"))
        {
            foreach (PhysicsObject c in targets)
            {
                c.SetAutoCenterOfMass();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

    }

}