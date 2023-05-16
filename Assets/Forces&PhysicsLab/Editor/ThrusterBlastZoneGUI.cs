using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ThrusterBlastZone))]
public class ThrusterBlastZoneGUI : Editor
{
    SerializedProperty BlastMultiplier;
    SerializedProperty ForceLimit;
    SerializedProperty Wobble;
    SerializedProperty Turbulence;
    SerializedProperty Frequency;
    SerializedProperty Radius;

    private void OnEnable()
    {
        BlastMultiplier = serializedObject.FindProperty("BlastMultiplier");
        ForceLimit = serializedObject.FindProperty("ForceLimit");
        Wobble = serializedObject.FindProperty("Wobble");
        Turbulence = serializedObject.FindProperty("Turbulence");
        Frequency = serializedObject.FindProperty("Frequency");
        Radius = serializedObject.FindProperty("Radius");
    }

    private void Awake()
    {
        ThrusterBlastZone blastzone = (ThrusterBlastZone)target;
        if (blastzone == null)
        {
            return;
        }

        blastzone.T = blastzone.GetComponent<Thruster>();
    }
    void OnSceneGUI()
    {
        ThrusterBlastZone blastzone = (ThrusterBlastZone)target;
        if (blastzone == null)
        {
            return;
        }

        if (blastzone.T.Thrust == Thruster.ThrustMode.Hover)
        {
            float f = blastzone.T.hoverHeight / 2;
            Vector3 cm = blastzone.transform.TransformDirection(new Vector3(0, 0, f));

            float tr;
            EditorGUI.BeginChangeCheck();
            tr = Handles.RadiusHandle(blastzone.transform.rotation, blastzone.transform.position + cm, f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blastzone, "Change radius");
                f = tr;
                blastzone.T.hoverHeight = f * 2;
            }

            
            Handles.ConeHandleCap(0, (blastzone.T.transform.position + blastzone.T.transform.forward * (blastzone.T.hoverHeight)), blastzone.T.transform.rotation, 1f, EventType.Repaint);
            Handles.DrawLine(blastzone.T.transform.position, blastzone.T.transform.position + blastzone.T.transform.forward * blastzone.T.hoverHeight);

        }
        else
        {
            Vector3 cm = blastzone.transform.TransformDirection(new Vector3(0, 0, blastzone.Radius));

            float tr;
            EditorGUI.BeginChangeCheck();
            tr = Handles.RadiusHandle(blastzone.transform.rotation, blastzone.transform.position + cm, blastzone.Radius);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blastzone, "Change radius");
                blastzone.Radius = tr;
            }


            Handles.ConeHandleCap(0, (blastzone.T.transform.position + blastzone.T.transform.forward * (blastzone.Radius * 2)), blastzone.T.transform.rotation, 1f, EventType.Repaint);
            Handles.DrawLine(blastzone.T.transform.position, blastzone.T.transform.position + blastzone.T.transform.forward * blastzone.Radius * 2);
        }
    }


    public override void OnInspectorGUI()
    {

        serializedObject.Update();

        ThrusterBlastZone blastzone = (ThrusterBlastZone)target;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(BlastMultiplier,new GUIContent("Blast Multiplier", "The thruster component's original Force is multiplied by this."));
        EditorGUILayout.PropertyField(ForceLimit,new GUIContent("Force Limit", "The maximum strength of a force. This could provide you some stability, if necessary."));
        if (EditorGUILayout.PropertyField(Wobble, new GUIContent("Wobble", "Adds a fast wobble to the blast force. For a more windy appearance on objects.")))
        {
            EditorGUILayout.PropertyField(Turbulence,new GUIContent("Wobble Turbulence", "The strength of the wobble"));
            EditorGUILayout.PropertyField(Frequency,new GUIContent("Wobble Frequency", "How fast the wobble is."));
        }

        if (blastzone.T.Thrust == Thruster.ThrustMode.RocketPower)
        {
            EditorGUILayout.PropertyField(Radius,new GUIContent("Blast Radius", "The maximum blast radius. Does not affect the Blast decay. Can be changed in scene view by dragging the handles of the blast radius sphere."));
        }
            
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }


}
