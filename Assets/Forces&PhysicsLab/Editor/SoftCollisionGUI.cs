using UnityEngine;
using UnityEditor;


[SerializeField]
[CanEditMultipleObjects]
[CustomEditor(typeof(SoftCollision))]
class SoftCollisionGUI : Editor
{
    SerializedProperty Density;
    SerializedProperty NonDensity;
    SerializedProperty DisableDensityCalculation;
    SerializedProperty CollideWithSoftCollision;
    SerializedProperty ThrustForce;
    SerializedProperty PressureDamp;
    SerializedProperty SurfaceCorrection;
    SerializedProperty Forcemode;
    SerializedProperty maskLayers;

    private void OnEnable()
    {
        Density = serializedObject.FindProperty("Density");
        NonDensity = serializedObject.FindProperty("NonDensity");
        DisableDensityCalculation = serializedObject.FindProperty("DisableDensityCalculation");
        CollideWithSoftCollision = serializedObject.FindProperty("CollideWithSoftCollision");
        ThrustForce = serializedObject.FindProperty("ThrustForce");

        PressureDamp = serializedObject.FindProperty("PressureDamp");
        Forcemode = serializedObject.FindProperty("Forcemode");
        maskLayers = serializedObject.FindProperty("maskLayers");
    
        SoftCollision softobject = (SoftCollision)target;
        softobject.rBody = softobject.gameObject.GetComponent<Rigidbody>();
    }


    public override void OnInspectorGUI()
    {

        serializedObject.Update();

        SoftCollision softobject = (SoftCollision)target;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(Density,new GUIContent("Density", "The Density of the object's material."));
        EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.PropertyField(NonDensity,new GUIContent("Non Density Objects will: ", "Specify what objects without Density will do when colliding. They can be pressed outward or be ignored by this Soft collision Object. When set to sink, the Softcollision object will move away from the colliding object."));

            EditorGUILayout.PropertyField(ThrustForce,new GUIContent("Pressure Multiplier:", "How strongly the object will be pushed out of the density object.  In respect to both object's density."));
            EditorGUILayout.PropertyField(PressureDamp,new GUIContent("Pressure Damping:", "The pressure falloff. The closer an is to being out of the DensityObject's boundary. The more the outward pressure is decreased. This value specifies how much that decrease is applied."));
            EditorGUILayout.PropertyField(DisableDensityCalculation, new GUIContent("Disable Density Calculation", "If true: every colliding object will be treated as a 'Non Density object'. Even when it has a density component."));
            EditorGUILayout.PropertyField(CollideWithSoftCollision, new GUIContent("Collide with Softcollision", "Specify wether it will collide with other soft collision objects."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(Forcemode, new GUIContent("Force Mode", "Use ForceMode to specify how to apply a force. Acceleration is recommended."));
            EditorGUILayout.PropertyField(maskLayers,new GUIContent("Included layers:", "Specify wich layers to exclude from the force."));
            EditorGUILayout.Space();


           
        
        if (softobject.GetComponent<Collider>())
        {
            if (!softobject.GetComponent<Collider>().isTrigger)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Your collider must be set to Trigger in order for soft collision to work.");
                if (GUILayout.Button("Fix Now"))
                {
                    softobject.GetComponent<Collider>().isTrigger = true;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField("This Density object has no Collider.");
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }


}

      
