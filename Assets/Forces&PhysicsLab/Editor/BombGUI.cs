using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


[SerializeField]
[CanEditMultipleObjects]
[CustomEditor(typeof(Bomb))]
class BombGUI : Editor
{


    SerializedProperty StartActive;
    SerializedProperty Countdown;

    SerializedProperty delay;
    SerializedProperty radius;
    SerializedProperty upwardsModifier;

    SerializedProperty power;
    SerializedProperty explosionPos;
    SerializedProperty maskLayers;
    SerializedProperty blockLayers;
    SerializedProperty loop;
    SerializedProperty CheckIfBlocked;
    SerializedProperty hits;
    SerializedProperty NonAlloc;
    SerializedProperty NonAllocBuffer;

    private void OnEnable()
    {
        Countdown = serializedObject.FindProperty("Countdown");
        StartActive = serializedObject.FindProperty("StartActive");
        delay = serializedObject.FindProperty("delay");
        radius = serializedObject.FindProperty("radius");
        upwardsModifier = serializedObject.FindProperty("upwardsModifier");

        power = serializedObject.FindProperty("power");
        explosionPos = serializedObject.FindProperty("explosionPos");
        maskLayers = serializedObject.FindProperty("maskLayers");

        blockLayers = serializedObject.FindProperty("blockLayers");
        loop = serializedObject.FindProperty("loop");
        CheckIfBlocked = serializedObject.FindProperty("CheckIfBlocked");
        hits = serializedObject.FindProperty("hits");
        NonAlloc = serializedObject.FindProperty("NonAlloc");
        NonAllocBuffer = serializedObject.FindProperty("NonAllocBuffer");
    }

    void OnSceneGUI()
    {
        Bomb explosion = (Bomb)target;
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

        Bomb explosion = (Bomb)target;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("CountDown", explosion.Countdown.ToString());
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(StartActive, new GUIContent("Start active", "Specify if the countdown will start on Start."));
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