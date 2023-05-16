using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


[SerializeField]
[CanEditMultipleObjects]
[CustomEditor(typeof(Explosion))]
class ExplosionGUI : Editor
{
    SerializedProperty delay;
    SerializedProperty radius;
    SerializedProperty upwardsModifier;

    SerializedProperty power;
    SerializedProperty maskLayers;
    SerializedProperty blockLayers;
    SerializedProperty loop;
    SerializedProperty CheckIfBlocked;
    SerializedProperty NonAlloc;
    SerializedProperty NonAllocBuffer;

    private void OnEnable()
    {
        delay = serializedObject.FindProperty("delay");
        radius = serializedObject.FindProperty("radius");
        upwardsModifier = serializedObject.FindProperty("upwardsModifier");

        power = serializedObject.FindProperty("power");
        maskLayers = serializedObject.FindProperty("maskLayers");

        blockLayers = serializedObject.FindProperty("blockLayers");
        loop = serializedObject.FindProperty("loop");
        CheckIfBlocked = serializedObject.FindProperty("CheckIfBlocked");
        NonAlloc = serializedObject.FindProperty("NonAlloc");
        NonAllocBuffer = serializedObject.FindProperty("NonAllocBuffer");
    }

    void OnSceneGUI()
    {
        Explosion explosion = (Explosion)target;
        if (explosion == null)
        {
            return;
        }

        float tr;
        Handles.color = new Color(0.2f, 0.2f, 0.2f, 0.75f);


        EditorGUI.BeginChangeCheck();
        tr = Handles.RadiusHandle(Quaternion.identity, explosion.transform.position, explosion.radius);
        if (EditorGUI.EndChangeCheck())
        {
                Undo.RecordObject(explosion, "Change radius");
            explosion.radius = tr;
        }
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();

        Explosion explosion = (Explosion)target;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(delay, new GUIContent("Delay", "The delay in seconds before the explosion occurs."));
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(power, new GUIContent("Force", "The force of the explosion (which may be modified by distance)."));
        EditorGUILayout.PropertyField(radius, new GUIContent("Radius", "The radius of the sphere within which the explosion has its effect."));
        EditorGUILayout.PropertyField(upwardsModifier, new GUIContent("Upward modifier", "Adjustment to the apparent position of the explosion to make it seem to lift objects."));
        EditorGUILayout.EndVertical();
        EditorGUILayout.PropertyField(maskLayers, new GUIContent("Mask layers", "The layers on wich the explosion will have effect."));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(CheckIfBlocked, new GUIContent("Check if blocked", "Only apply force to objects that are not blocked by others."));
        if (explosion.CheckIfBlocked)
        { EditorGUILayout.PropertyField(blockLayers, new GUIContent("Block layers", "The layers that are able to block the force.")); }
            EditorGUILayout.PropertyField(NonAlloc, new GUIContent("Non Alloc", "If enabled. The component will create no memory garbage. But a Buffer is mandatory."));
        if (explosion.NonAlloc) { EditorGUILayout.PropertyField(NonAllocBuffer, new GUIContent("Non Alloc Buffer", "The maximum number of objects that can be affected by this component.")); }
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(loop, new GUIContent("After Explode", "Specify what the script does after the explosion is resolved."));
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

    }

}