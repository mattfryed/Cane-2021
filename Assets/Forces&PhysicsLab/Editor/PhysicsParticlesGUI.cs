using UnityEngine;
using UnityEditor;

[SerializeField]
[CanEditMultipleObjects]
[CustomEditor(typeof(PhysicsParticles))]
class PhysicsParticlesLabelHandle : Editor
{
    SerializedProperty CanBeAttracted;
    SerializedProperty EnableParticleProcessing;
    SerializedProperty ParticleBuffer;

    private void Awake()
    {
        PhysicsParticles com = (PhysicsParticles)target;
        com.rBody = com.GetComponent<Rigidbody>();
        com.particlesystem = com.GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        CanBeAttracted = serializedObject.FindProperty("CanBeAttracted");
        EnableParticleProcessing = serializedObject.FindProperty("EnableParticleProcessing");
        ParticleBuffer = serializedObject.FindProperty("ParticleBuffer");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        PhysicsObject com = (PhysicsObject)target;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(CanBeAttracted,new GUIContent("Can be attracted", "Specifies wether these particles can be attracted by GravityObjects."));
        EditorGUILayout.PropertyField(EnableParticleProcessing,new GUIContent("Enable Particle Processing", "When enabled, gravity will be calculated per particle. When disabled, gravity will calculate from this this Transform."));
        EditorGUILayout.PropertyField(ParticleBuffer, new GUIContent("Particle Buffer", "The maximum number of particles that can be influenced by a force."));

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

    }

}