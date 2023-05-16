using UnityEngine;
using UnityEditor;

[SerializeField]
[CanEditMultipleObjects]
[CustomEditor(typeof(GlobalWater))]
class GlobalWaterLabelHandle : Editor
{
    SerializedProperty Density;
    SerializedProperty NonDensity;
    SerializedProperty SurfaceHeight;
    SerializedProperty ThrustForce;
    SerializedProperty PressureDamp;
    SerializedProperty minimumPressure;
    SerializedProperty ApplyRotatingPressure;
    SerializedProperty Forcemode;
    SerializedProperty maskLayers;

    private void OnEnable()
    {
        Density = serializedObject.FindProperty("Density");
        NonDensity = serializedObject.FindProperty("NonDensity");
        SurfaceHeight = serializedObject.FindProperty("SurfaceHeight");
        ThrustForce = serializedObject.FindProperty("ThrustForce");
        PressureDamp = serializedObject.FindProperty("PressureDamp");
        minimumPressure = serializedObject.FindProperty("minimumPressure");
        ApplyRotatingPressure = serializedObject.FindProperty("ApplyRotatingPressure");
        Forcemode = serializedObject.FindProperty("Forcemode");
        maskLayers = serializedObject.FindProperty("maskLayers");

    }

    void OnSceneGUI()
    {
        GlobalWater globalwater = (GlobalWater)target;
        if (globalwater == null)
        {
            return;
        }

        float size = 3f;

        globalwater.coll = globalwater.GetComponent<Collider>();

        Vector3 origin = new Vector3(globalwater.transform.position.x, globalwater.coll.bounds.min.y + globalwater.SurfaceHeight, globalwater.transform.position.z);

        float h;

        EditorGUI.BeginChangeCheck();
        h = (Handles.Slider(origin, Vector3.up, size / 2, Handles.ConeHandleCap, 1).y - globalwater.coll.bounds.min.y);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(globalwater, "Change Surface height");
            globalwater.SurfaceHeight = h;
        }

        Handles.color = new Color(0, 255, 255, 0.5f);
        Handles.DrawSolidDisc(origin, Vector3.up, size);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GlobalWater globalwater = (GlobalWater)target;

        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(Density,new GUIContent("Density", "The Density of the water."));
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.PropertyField(NonDensity, new GUIContent("Non Density Object: ", "Specify wether the entering gameObject will float or sink, if it has no DensityObject component."));

        EditorGUILayout.PropertyField(SurfaceHeight, new GUIContent("Surface height:", "The height of the surface of the water. Or in other words: how deep the water is. Measured from the lowest point of the collider (along the y-axis). This can also be edited by dragging the upward arrow in the scene view."));

        EditorGUILayout.PropertyField(ThrustForce, new GUIContent("Pressure Multiplier:", "How strongly the object will float towards the desired height. This does not specify how high an object will float. A higher value will make the water lift heavier objects faster, but still in respect to the density. A sinking object will still sink."));
        EditorGUILayout.PropertyField(PressureDamp, new GUIContent("Pressure Damping:", "The pressure falloff. The closer an object is to the water surface. The more the upward pressure is decreased. This value specifies how much that decrease is applied. A value higher than 0 is needed to stabilize objects floating in the water. A value of 1 to make the upward pressure exactly 0 when the object's height is exactly on the water surface."));
        EditorGUILayout.PropertyField(minimumPressure, new GUIContent("Minimum Pressure:", "The minimum density difference between two density object's. "));
        EditorGUILayout.PropertyField(ApplyRotatingPressure, new GUIContent("Raycast Pressure", "This will apply rotation to the floating objects. Objects will always rotate to lay-flat on the water. You could turn this of if you use (for example) spherical objects, since they don't need to rotate will floating. One raycast per floating object is used."));

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(Forcemode, new GUIContent("Force Mode", "Use ForceMode to specify how to apply a force."));
        EditorGUILayout.PropertyField(maskLayers, new GUIContent("Included layers:", "Specify wich layers to exclude from the force."));
        EditorGUILayout.Space();
        if (!globalwater.GetComponent<Collider>().isTrigger)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Your collider must be set to Trigger in order for the water to work.");
            if (GUILayout.Button("Fix Now"))
            {
                globalwater.GetComponent<Collider>().isTrigger = true;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

}




      
