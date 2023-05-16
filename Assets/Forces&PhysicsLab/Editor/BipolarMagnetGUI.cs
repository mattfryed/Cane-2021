using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[SerializeField]
[CanEditMultipleObjects]

[CustomEditor(typeof(BipolarMagnet))]
class MagnetLabelHandle : Editor
{
    SerializedProperty MagneticMultiplier;
    SerializedProperty CanBeAttracted;
    SerializedProperty NoDecay;

    SerializedProperty NonAlloc;
    SerializedProperty NonAllocBuffer;
    SerializedProperty Radius;
    SerializedProperty NorthPole;
    SerializedProperty SouthPole;
    SerializedProperty Forcemode;
    SerializedProperty maskLayers;


    private void OnEnable()
    {
        MagneticMultiplier = serializedObject.FindProperty("MagneticMultiplier");
        CanBeAttracted = serializedObject.FindProperty("CanBeAttracted");
        NoDecay = serializedObject.FindProperty("NoDecay");
        NonAlloc = serializedObject.FindProperty("NonAlloc");
        NonAllocBuffer = serializedObject.FindProperty("NonAllocBuffer");
        Radius = serializedObject.FindProperty("Radius");
        NorthPole = serializedObject.FindProperty("NorthPole");
        SouthPole = serializedObject.FindProperty("SouthPole");
        Forcemode = serializedObject.FindProperty("Forcemode");
        maskLayers = serializedObject.FindProperty("maskLayers");

    }

    void OnSceneGUI()
    {
        BipolarMagnet gravityobject = (BipolarMagnet)target;
        if (gravityobject == null)
        {
            return;
        }
        Vector3 ncm = gravityobject.transform.TransformDirection(gravityobject.NorthPole);
        Vector3 scm = gravityobject.transform.TransformDirection(gravityobject.SouthPole);


        

        if (gravityobject.MagneticMultiplier > 0) { Handles.color = new Color(255,0,0,0.5f); } else { Handles.color = new Color(0, 255, 0, 0.5f); }
        Handles.SphereHandleCap(0, gravityobject.transform.position+ncm, Quaternion.identity, 1, EventType.Repaint);
        Handles.Label(gravityobject.transform.position + ncm, "N");

        //Position Handle
        Handles.matrix = Matrix4x4.TRS(gravityobject.transform.position, gravityobject.transform.rotation, new Vector3(0.5f, 0.5f, 0.5f));

        EditorGUI.BeginChangeCheck();
        Vector3 newTargetPositionNorth = Handles.PositionHandle(gravityobject.NorthPole * 2, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(gravityobject, "Change North Pole");
            gravityobject.NorthPole = newTargetPositionNorth / 2;
        }
        Handles.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 1, 1));

        float tr;

        EditorGUI.BeginChangeCheck();
        tr = Handles.RadiusHandle(Quaternion.identity, gravityobject.transform.position + ncm, gravityobject.Radius);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(gravityobject, "Change radius");
            gravityobject.Radius = tr;
        }

        if (gravityobject.MagneticMultiplier > 0 ) { Handles.color = new Color(0, 255, 0, 0.5f); } else { Handles.color = new Color(255, 0, 0, 0.5f); }
        Handles.SphereHandleCap(0, gravityobject.transform.position + scm, Quaternion.identity, 1, EventType.Repaint);
        Handles.Label(gravityobject.transform.position + scm, "S");

        //Position Handle
        Handles.matrix = Matrix4x4.TRS(gravityobject.transform.position, gravityobject.transform.rotation, new Vector3(0.5f, 0.5f, 0.5f));

        EditorGUI.BeginChangeCheck();
        Vector3 newTargetPositionSouth = Handles.PositionHandle(gravityobject.SouthPole * 2, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(gravityobject, "Change South Pole");
            gravityobject.SouthPole = newTargetPositionSouth / 2;
        }
        Handles.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 1, 1));

        EditorGUI.BeginChangeCheck();
        tr = Handles.RadiusHandle(Quaternion.identity, gravityobject.transform.position + scm, gravityobject.Radius);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(gravityobject, "Change radius");
            gravityobject.Radius = tr;
        }
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();
        BipolarMagnet gravityobject = (BipolarMagnet)target;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(MagneticMultiplier,new GUIContent("Magnetism Multiplier", "This is multiplied by the RigidBody mass to define how much magnetic attraction it has."));
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(new GUIStyle("box"));
        EditorGUILayout.PropertyField(CanBeAttracted,new GUIContent("Can be attracted", "Specifies wether this object can be attracted by other Magnets."));
        EditorGUILayout.PropertyField(NoDecay,new GUIContent("No Decay", "Specify wether the magnetic field has a decay or not."));
        EditorGUILayout.PropertyField(NonAlloc,new GUIContent("Non Alloc", "If enabled. The component will create no memory garbage. But a buffer is mandatory."));
        if (gravityobject.NonAlloc) { EditorGUILayout.PropertyField(NonAllocBuffer,new GUIContent("Non Alloc Buffer", "The maximum number of objects that can be affected by this component.")); }

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(new GUIStyle("box"));

        EditorGUILayout.PropertyField(Radius,new GUIContent("Field Radius", "The maximum radius of the Spherical force field.")); 
        EditorGUILayout.PropertyField(NorthPole,new GUIContent("Northern end:", "Relative position of the Northern magnetic pole."));
        EditorGUILayout.PropertyField(SouthPole,new GUIContent("Southern end:", "Relative position of the Southern magnetic pole."));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(Forcemode,new GUIContent("Force Mode", "Use ForceMode to specify how to apply a force. Acceleration is recommended."));
        EditorGUILayout.PropertyField(maskLayers,new GUIContent("Included layers:", "Specify wich layers to exclude from the force."));
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}