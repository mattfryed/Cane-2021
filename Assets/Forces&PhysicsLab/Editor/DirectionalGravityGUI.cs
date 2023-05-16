using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[SerializeField]
[CanEditMultipleObjects]

[CustomEditor(typeof(DirectionalGravity))]
class DirectionalGravityLabelHandle : Editor
{

    SerializedProperty GravityMultiplier;
    SerializedProperty Direction;
    SerializedProperty CanBeAttracted;
    SerializedProperty AffectAllRigidBodies;

    SerializedProperty AlignToForce;
    SerializedProperty AlignStrength;
    SerializedProperty AlignLimit;
    SerializedProperty DisableLocking;
    SerializedProperty NoDecay;
    SerializedProperty EnableForceProcessing;
    SerializedProperty NonAlloc;
    SerializedProperty NonAllocBuffer;
    SerializedProperty ForceShape;
    SerializedProperty Size;
    SerializedProperty Radius;
    SerializedProperty centerOfMass;

    SerializedProperty Priority;
    SerializedProperty Forcemode;
    SerializedProperty maskLayers;

    private void OnEnable()
    {
        GravityMultiplier = serializedObject.FindProperty("GravityMultiplier");
        Direction = serializedObject.FindProperty("Direction");
        CanBeAttracted = serializedObject.FindProperty("CanBeAttracted");
        AffectAllRigidBodies = serializedObject.FindProperty("AffectAllRigidBodies");

        AlignToForce = serializedObject.FindProperty("AlignToForce");
        AlignStrength = serializedObject.FindProperty("AlignStrength");
        AlignLimit = serializedObject.FindProperty("AlignLimit");

        DisableLocking = serializedObject.FindProperty("DisableLocking");
        NoDecay = serializedObject.FindProperty("NoDecay");
        EnableForceProcessing = serializedObject.FindProperty("EnableForceProcessing");
        NonAlloc = serializedObject.FindProperty("NonAlloc");
        NonAllocBuffer = serializedObject.FindProperty("NonAllocBuffer");
        ForceShape = serializedObject.FindProperty("ForceShape");
        Size = serializedObject.FindProperty("Size");
        Radius = serializedObject.FindProperty("Radius");
        centerOfMass = serializedObject.FindProperty("centerOfMass");
        Priority = serializedObject.FindProperty("Priority");
        Forcemode = serializedObject.FindProperty("Forcemode");
        maskLayers = serializedObject.FindProperty("maskLayers");
    }

    private void Awake()
    {
        DirectionalGravity gravityobject = (DirectionalGravity)target;
        gravityobject.rBody = gravityobject.gameObject.GetComponent<Rigidbody>();
    }
    void OnSceneGUI()
    {
        DirectionalGravity gravityobject = (DirectionalGravity)target;
        if (gravityobject == null)
        {
            return;
        }

        Vector3 cm = gravityobject.transform.TransformDirection(gravityobject.centerOfMass);

        Handles.color = new Color(0, 0, 0, 0.5f);
        Handles.SphereHandleCap(0, gravityobject.transform.position + cm, Quaternion.identity, 1, EventType.Repaint);
        Handles.Label(gravityobject.transform.position + cm, "Center of mass");


            if (gravityobject.ForceShape == DirectionalGravity.FieldShape.Sphere)
            {
                float arrowpos = gravityobject.Radius / 2f;
                float size = Mathf.Clamp(96, 0, arrowpos);

                float tr;

            if (gravityobject.Direction != Vector3.zero)
            {
                if (gravityobject.GravityMultiplier > 0)
                {
                    Handles.color = new Color(0.9f, 0.9f, 0.9f, 0.75f);
                    Handles.ArrowHandleCap(0, gravityobject.transform.position + (new Vector3(arrowpos, 0, 0)), Quaternion.LookRotation(gravityobject.transform.TransformDirection(gravityobject.Direction)), size, EventType.Repaint);
                    Handles.ArrowHandleCap(0, gravityobject.transform.position + (new Vector3(0, arrowpos, 0)), Quaternion.LookRotation(gravityobject.transform.TransformDirection(gravityobject.Direction)), size, EventType.Repaint);
                    Handles.ArrowHandleCap(0, gravityobject.transform.position + (new Vector3(0, 0, arrowpos)), Quaternion.LookRotation(gravityobject.transform.TransformDirection(gravityobject.Direction)), size, EventType.Repaint);
                    Handles.ArrowHandleCap(0, gravityobject.transform.position + (new Vector3(-(arrowpos), 0, 0)), Quaternion.LookRotation(gravityobject.transform.TransformDirection(gravityobject.Direction)), size, EventType.Repaint);
                    Handles.ArrowHandleCap(0, gravityobject.transform.position + (new Vector3(0, -(arrowpos), 0)), Quaternion.LookRotation(gravityobject.transform.TransformDirection(gravityobject.Direction)), size, EventType.Repaint);
                    Handles.ArrowHandleCap(0, gravityobject.transform.position + (new Vector3(0, 0, -(arrowpos))), Quaternion.LookRotation(gravityobject.transform.TransformDirection(gravityobject.Direction)), size, EventType.Repaint);
                }
                else
                {
                    Handles.color = new Color(0.9f, 0.9f, 0.9f, 0.75f);
                    Handles.ArrowHandleCap(0, gravityobject.transform.position + (new Vector3(arrowpos, 0, 0)), Quaternion.LookRotation(-gravityobject.transform.TransformDirection(gravityobject.Direction)), size, EventType.Repaint);
                    Handles.ArrowHandleCap(0, gravityobject.transform.position + (new Vector3(0, arrowpos, 0)), Quaternion.LookRotation(-gravityobject.transform.TransformDirection(gravityobject.Direction)), size, EventType.Repaint);
                    Handles.ArrowHandleCap(0, gravityobject.transform.position + (new Vector3(0, 0, arrowpos)), Quaternion.LookRotation(-gravityobject.transform.TransformDirection(gravityobject.Direction)), size, EventType.Repaint);
                    Handles.ArrowHandleCap(0, gravityobject.transform.position + (new Vector3(-arrowpos, 0, 0)), Quaternion.LookRotation(-gravityobject.transform.TransformDirection(gravityobject.Direction)), size, EventType.Repaint);
                    Handles.ArrowHandleCap(0, gravityobject.transform.position + (new Vector3(0, -arrowpos, 0)), Quaternion.LookRotation(-gravityobject.transform.TransformDirection(gravityobject.Direction)), size, EventType.Repaint);
                    Handles.ArrowHandleCap(0, gravityobject.transform.position + (new Vector3(0, 0, -arrowpos)), Quaternion.LookRotation(-gravityobject.transform.TransformDirection(gravityobject.Direction)), size, EventType.Repaint);
                }
            }
                EditorGUI.BeginChangeCheck();
                tr = Handles.RadiusHandle(Quaternion.identity, gravityobject.transform.position, gravityobject.Radius);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(gravityobject, "Change radius");
                    gravityobject.Radius = tr;
                }

            }
            if (gravityobject.ForceShape == DirectionalGravity.FieldShape.Cube)
            {
                float arrowposx = gravityobject.Size.x / 2;
                float sizex = Mathf.Clamp(96, 0, arrowposx);
                float arrowposy = gravityobject.Size.y / 2;
                float sizey = Mathf.Clamp(96, 0, arrowposy);
                float arrowposz = gravityobject.Size.z / 2;
                float sizez = Mathf.Clamp(96, 0, arrowposz);


                Handles.matrix = Matrix4x4.TRS(gravityobject.transform.position, gravityobject.transform.rotation, new Vector3(1, 1, 1));


                float size;

                Vector3 s = new Vector3();
                EditorGUI.BeginChangeCheck();
                Handles.color = new Color(0.7f, 0.5f, 0.5f, 1f);
                Vector3 pos = new Vector3(gravityobject.Size.x, 0, 0);
                size = HandleUtility.GetHandleSize(pos) * 0.5f;
                s.x = (Handles.Slider(pos, Vector3.right, size, Handles.ArrowHandleCap, 1)).x;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(gravityobject, "Change Size X");
                    gravityobject.Size.x = s.x;
                }

                EditorGUI.BeginChangeCheck();
                Handles.color = new Color(0.5f, 0.7f, 0.5f, 1f);
                pos = new Vector3(0, gravityobject.Size.y, 0);
                size = HandleUtility.GetHandleSize(pos) * 0.5f;
                s.y = (Handles.Slider(pos, Vector3.up, size, Handles.ArrowHandleCap, 1)).y;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(gravityobject, "Change Size Y");
                    gravityobject.Size.y = s.y;
                }

                EditorGUI.BeginChangeCheck();
                Handles.color = new Color(0.5f, 0.5f, 0.7f, 1f);
                pos = new Vector3(0, 0, gravityobject.Size.z);
                size = HandleUtility.GetHandleSize(pos) * 0.5f;
                s.z = (Handles.Slider(pos, Vector3.forward, size, Handles.ArrowHandleCap, 1)).z;
                if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(gravityobject, "Change Size Z"); gravityobject.Size.z = s.z; }

                if (gravityobject.Direction != Vector3.zero)
                {
                    if (gravityobject.GravityMultiplier > 0)
                    {
                        Handles.color = new Color(0.9f, 0.9f, 0.9f, 0.75f);
                        Handles.DrawWireCube(Vector3.zero, gravityobject.Size * 2);
                        Handles.ArrowHandleCap(0, Vector3.zero + (new Vector3(arrowposx, 0, 0)), Quaternion.LookRotation(gravityobject.Direction), sizex, EventType.Repaint);
                        Handles.ArrowHandleCap(0, Vector3.zero + (new Vector3(0, arrowposy, 0)), Quaternion.LookRotation(gravityobject.Direction), sizey, EventType.Repaint);
                        Handles.ArrowHandleCap(0, Vector3.zero + (new Vector3(0, 0, arrowposz)), Quaternion.LookRotation(gravityobject.Direction), sizez, EventType.Repaint);
                        Handles.ArrowHandleCap(0, Vector3.zero + (new Vector3(-(arrowposx), 0, 0)), Quaternion.LookRotation(gravityobject.Direction), sizex, EventType.Repaint);
                        Handles.ArrowHandleCap(0, Vector3.zero + (new Vector3(0, -(arrowposy), 0)), Quaternion.LookRotation(gravityobject.Direction), sizey, EventType.Repaint);
                        Handles.ArrowHandleCap(0, Vector3.zero + (new Vector3(0, 0, -(arrowposz))), Quaternion.LookRotation(gravityobject.Direction), sizez, EventType.Repaint);
                    }
                    else
                    {
                        Handles.color = new Color(0.9f, 0.9f, 0.9f, 0.75f);
                        Handles.DrawWireCube(Vector3.zero, gravityobject.Size * 2);
                        Handles.ArrowHandleCap(0, Vector3.zero + (new Vector3(arrowposx, 0, 0)), Quaternion.LookRotation(-gravityobject.Direction), sizex, EventType.Repaint);
                        Handles.ArrowHandleCap(0, Vector3.zero + (new Vector3(0, arrowposy, 0)), Quaternion.LookRotation(-gravityobject.Direction), sizey, EventType.Repaint);
                        Handles.ArrowHandleCap(0, Vector3.zero + (new Vector3(0, 0, arrowposz)), Quaternion.LookRotation(-gravityobject.Direction), sizez, EventType.Repaint);
                        Handles.ArrowHandleCap(0, Vector3.zero + (new Vector3(-arrowposx, 0, 0)), Quaternion.LookRotation(-gravityobject.Direction), sizex, EventType.Repaint);
                        Handles.ArrowHandleCap(0, Vector3.zero + (new Vector3(0, -arrowposy, 0)), Quaternion.LookRotation(-gravityobject.Direction), sizey, EventType.Repaint);
                        Handles.ArrowHandleCap(0, Vector3.zero + (new Vector3(0, 0, -arrowposz)), Quaternion.LookRotation(-gravityobject.Direction), sizez, EventType.Repaint);
                    }
                }
                else
                {
                    Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
                    Handles.DrawWireCube(Vector3.zero, gravityobject.Size * 2);
                }


                Handles.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 1, 1));

            }
        



        //Position Handle
        Handles.matrix = Matrix4x4.TRS(gravityobject.transform.position, gravityobject.transform.rotation, new Vector3(0.5f, 0.5f, 0.5f));

        EditorGUI.BeginChangeCheck();
        Vector3 newTargetPosition = Handles.PositionHandle(gravityobject.centerOfMass * 2, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(gravityobject, "Change Center of mass");
            gravityobject.centerOfMass = newTargetPosition / 2;
        }
        Handles.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 1, 1));
    }

    public override void OnInspectorGUI()
    {


        serializedObject.Update();

        DirectionalGravity gravityobject = (DirectionalGravity)target;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(GravityMultiplier, new GUIContent("Gravity Multiplier", "This is multiplied by the RigidBody mass to define how much gravitational pull it has."));
        EditorGUILayout.PropertyField(Direction, new GUIContent("Direction", "The direction to pull Objects towards (Direction will be normalized)."));
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(new GUIStyle("box"));
        EditorGUILayout.PropertyField(CanBeAttracted, new GUIContent("Can be attracted", "Specifies wether this object can be attracted by other GravityObjects."));
        EditorGUILayout.PropertyField(AffectAllRigidBodies, new GUIContent("Affects all rigidbodies", "Specify wether it affects all rigidbodies. Or only objects with a Physics object component."));
        EditorGUILayout.PropertyField(AlignToForce, new GUIContent("Align to Force (self)", "If true, this will rotate itself to point with it's Center of Mass towards the gravity that is attracting it. (For example: This can be used to make a player stand straight up in a spherical world.)"));
        if (gravityobject.AlignToForce)
        {
            EditorGUILayout.BeginVertical(new GUIStyle("box"));
            EditorGUILayout.PropertyField(AlignStrength, new GUIContent("Align Strength", "The strength of the Lock"));
            EditorGUILayout.PropertyField(AlignLimit, new GUIContent("Align Limit", "How much the objects rotation may differ from being aligned to the force."));
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.PropertyField(DisableLocking, new GUIContent("Disable Align to Force (other)", "If set to true. This gravity field will not Lock Center of mass Direction on other objects."));
        EditorGUILayout.PropertyField(NoDecay, new GUIContent("No Decay", "Specify wether the gravity field has a decay or not."));
        EditorGUILayout.PropertyField(EnableForceProcessing, new GUIContent("Enable Force Processing", "When enabled and multiple forces apply to this object. The object will only apply the highest priority force. And discard the others."));
        EditorGUILayout.PropertyField(NonAlloc, new GUIContent("Non Alloc", "If enabled. The component will create no memory garbage. But a Buffer is mandatory."));
        if (gravityobject.NonAlloc) { EditorGUILayout.PropertyField(NonAllocBuffer, new GUIContent("Non Alloc Buffer", "The maximum number of objects that can be affected by this component.")); }
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(new GUIStyle("box"));
        EditorGUILayout.PropertyField(ForceShape, new GUIContent("Force Shape", "The shape of the force field. All objects in this zone will be attracted."));

        if (gravityobject.ForceShape == DirectionalGravity.FieldShape.Cube) { EditorGUILayout.PropertyField(Size, new GUIContent("Field Size:", "The size and shape of a Cubic force field.")); }
        if (gravityobject.ForceShape == DirectionalGravity.FieldShape.Sphere) { EditorGUILayout.PropertyField(Radius, new GUIContent("Field Radius", "The maximum radius of the Spherical force field.")); }
        EditorGUILayout.PropertyField(centerOfMass, new GUIContent("Center of Mass:", "The relative position of the Center of Mass. Objects will get pulled to this point."));
        // 
        if (GUILayout.Button("Auto center of mass"))
        {
            foreach (PhysicsObject c in targets)
            {
                c.SetAutoCenterOfMass();
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(Priority, new GUIContent("Priority", "Use with Force processing. Lower priority forces will be discarded when applying force to an object, that has Force processing enabled."));
        EditorGUILayout.PropertyField(Forcemode, new GUIContent("Force Mode", "Use ForceMode to specify how to apply a force."));
        EditorGUILayout.PropertyField(maskLayers, new GUIContent("Included layers:", "Specify wich layers to exclude from the force."));
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }


}