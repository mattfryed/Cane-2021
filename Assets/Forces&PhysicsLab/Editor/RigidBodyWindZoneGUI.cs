using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


[SerializeField]
[CanEditMultipleObjects]

[CustomEditor(typeof(RigidBodyWindZone))]
class RigidBodyWindZoneGUI : Editor
{
    SerializedProperty master;
    SerializedProperty Multiplier;
    SerializedProperty Radius;
    SerializedProperty mode;
    SerializedProperty Main;
    SerializedProperty Turbulence;
    SerializedProperty PulseMagnitude;
    SerializedProperty PulseFrequency;
    SerializedProperty NonAlloc;
    SerializedProperty NonAllocBuffer;
    SerializedProperty Forcemode;
    SerializedProperty maskLayers;

    private void OnEnable()
    {
        master = serializedObject.FindProperty("master");
        Multiplier = serializedObject.FindProperty("Multiplier");
        Radius = serializedObject.FindProperty("Radius");
        mode = serializedObject.FindProperty("mode");
        Main = serializedObject.FindProperty("Main");
        Turbulence = serializedObject.FindProperty("Turbulence");
        PulseMagnitude = serializedObject.FindProperty("PulseMagnitude");
        PulseFrequency = serializedObject.FindProperty("PulseFrequency");
        NonAlloc = serializedObject.FindProperty("NonAlloc");
        NonAllocBuffer = serializedObject.FindProperty("NonAllocBuffer");
        Forcemode = serializedObject.FindProperty("Forcemode");
        maskLayers = serializedObject.FindProperty("maskLayers");
    }

    void OnSceneGUI()
    {
        RigidBodyWindZone windzone = (RigidBodyWindZone)target;
        if (windzone == null)
        {
            return;
        }

        Handles.color = new Color(0.9f, 0.9f, 0.9f, 0.75f);
        float tr;

        EditorGUI.BeginChangeCheck();
        tr = Handles.RadiusHandle(Quaternion.identity, windzone.transform.position, windzone.Radius);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(windzone, "Change radius");
            windzone.Radius = tr;
        }

        float arrowpos = windzone.Radius / 1.3f;
        float size = Mathf.Clamp(96, 0, arrowpos);

        if ((windzone.mode == RigidBodyWindZone.Mode.Directional && windzone.master == null) || (windzone.master != null && windzone.master.mode == 0))
        {
            Handles.ArrowHandleCap(0, windzone.transform.position, Quaternion.LookRotation(windzone.transform.forward), size, EventType.Repaint);

            Handles.ArrowHandleCap(0, windzone.transform.position + (new Vector3(arrowpos, 0, 0)), Quaternion.LookRotation(windzone.transform.forward), size, EventType.Repaint);
            Handles.ArrowHandleCap(0, windzone.transform.position + (new Vector3(0, arrowpos, 0)), Quaternion.LookRotation(windzone.transform.forward), size, EventType.Repaint);
            Handles.ArrowHandleCap(0, windzone.transform.position + (new Vector3(0, 0, arrowpos)), Quaternion.LookRotation(windzone.transform.forward), size, EventType.Repaint);
            Handles.ArrowHandleCap(0, windzone.transform.position + (new Vector3(-(arrowpos), 0, 0)), Quaternion.LookRotation(windzone.transform.forward), size, EventType.Repaint);
            Handles.ArrowHandleCap(0, windzone.transform.position + (new Vector3(0, -(arrowpos), 0)), Quaternion.LookRotation(windzone.transform.forward), size, EventType.Repaint);
            Handles.ArrowHandleCap(0, windzone.transform.position + (new Vector3(0, 0, -(arrowpos))), Quaternion.LookRotation(windzone.transform.forward), size, EventType.Repaint);
        }
        else
        {

            Handles.ArrowHandleCap(0, windzone.transform.position + (new Vector3(arrowpos - size, 0, 0)), Quaternion.LookRotation(Vector3.right), size, EventType.Repaint);
            Handles.ArrowHandleCap(0, windzone.transform.position + (new Vector3(0, arrowpos - size, 0)), Quaternion.LookRotation(Vector3.up), size, EventType.Repaint);
            Handles.ArrowHandleCap(0, windzone.transform.position + (new Vector3(0, 0, arrowpos - size)), Quaternion.LookRotation(Vector3.forward), size, EventType.Repaint);
            Handles.ArrowHandleCap(0, windzone.transform.position + (new Vector3(-(arrowpos - size), 0, 0)), Quaternion.LookRotation(-Vector3.right), size, EventType.Repaint);
            Handles.ArrowHandleCap(0, windzone.transform.position + (new Vector3(0, -(arrowpos - size), 0)), Quaternion.LookRotation(-Vector3.up), size, EventType.Repaint);
            Handles.ArrowHandleCap(0, windzone.transform.position + (new Vector3(0, 0, -(arrowpos - size))), Quaternion.LookRotation(-Vector3.forward), size, EventType.Repaint);
        }



    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        RigidBodyWindZone windzone = (RigidBodyWindZone)target;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(master, new GUIContent("Master", "If a windzone is specified this component will become a slave to the standard windzone. The mode, Main, Turbulence, PulseMagnitude and PulseFrequency will be driven by the standard windzone."));

        if (windzone.master != null)
        {
            EditorGUILayout.PropertyField(Multiplier, new GUIContent("Multiplier", "This value is multiplied by the Main value."));
            if (windzone.master.mode == 0)
            {
                EditorGUILayout.PropertyField(Radius, new GUIContent("Radius", "The radius of the windzone."));
            }
            else
            {
                windzone.Radius = windzone.master.radius;
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Settings are overriden by the master WindZone.");
            EditorGUILayout.EndVertical();


        }
        else
        {
            EditorGUILayout.PropertyField(Multiplier, new GUIContent("Multiplier", "This value is multiplied by the Main value."));
            EditorGUILayout.PropertyField(Radius, new GUIContent("Radius", "The radius of the windzone."));
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.PropertyField(mode, new GUIContent("Mode", "Defines the type of wind zone to be used (Spherical or Directional)."));
            EditorGUILayout.PropertyField(Main, new GUIContent("Main", "The primary wind force."));
            EditorGUILayout.PropertyField(Turbulence, new GUIContent("Turbulence", "The turbulence wind force."));
            EditorGUILayout.PropertyField(PulseMagnitude, new GUIContent("Pulse Magnitude", "Defines how much the wind changes over time."));
            EditorGUILayout.PropertyField(PulseFrequency, new GUIContent("Pulse Frequency", "Defines the frequency of the wind changes."));
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(NonAlloc, new GUIContent("Non Alloc", "If enabled. The component will create no memory garbage. But a Buffer is mandatory."));
        if (windzone.NonAlloc) { EditorGUILayout.PropertyField(NonAllocBuffer, new GUIContent("Non Alloc Buffer", "The maximum number of objects that can be affected by this component.")); }
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(Forcemode, new GUIContent("Force Mode", "Use ForceMode to specify how to apply a force."));
        EditorGUILayout.PropertyField(maskLayers, new GUIContent("Included layers:", "Specify wich layers to exclude from the force."));
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

    }

}