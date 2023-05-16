using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class PFL_Menu : EditorWindow
{
    // Add menu item named "My Window" to the Window menu

    [MenuItem("GameObject/Forces and Physics Lab/Physics Object/Cube")]
    public static void CreatePC()
    {
        GameObject PC = GameObject.CreatePrimitive(PrimitiveType.Cube);
        PC.name = "Physics Cube";
        PC.AddComponent<PhysicsObject>();
        PC.AddComponent<Rigidbody>();

        //Position
        PC.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = PC;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(PC, "Create Physics Cube");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Physics Object/Sphere")]
    public static void CreatePS()
    {
        GameObject PC = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        PC.name = "Physics Sphere";
        PC.AddComponent<PhysicsObject>();
        PC.AddComponent<Rigidbody>();

        //Position
        PC.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = PC;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(PC, "Create Physics Sphere");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Physics Object/Cylinder")]
    public static void CreatePCy()
    {
        GameObject PC = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        PC.name = "Physics Cylinder";
        PC.AddComponent<PhysicsObject>();
        PC.AddComponent<Rigidbody>();

        //Position
        PC.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = PC;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(PC, "Create Physics Cylinder");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Physics Object/Capsule")]
    public static void CreatePCa()
    {
        GameObject PC = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        PC.name = "Physics Capsule";
        PC.AddComponent<PhysicsObject>();
        PC.AddComponent<Rigidbody>();

        //Position
        PC.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = PC;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(PC, "Create Physics Capsule");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Physics Object/Plane")]
    public static void CreatePP()
    {
        GameObject PC = GameObject.CreatePrimitive(PrimitiveType.Plane);
        PC.name = "Physics Plane";
        PC.AddComponent<PhysicsObject>();
        PC.AddComponent<Rigidbody>();

        //Position
        PC.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = PC;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(PC, "Create Physics Plane");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Physics Object/Quad")]
    public static void CreatePQ()
    {
        GameObject PC = GameObject.CreatePrimitive(PrimitiveType.Quad);
        PC.name = "Physics Quad";
        PC.AddComponent<PhysicsObject>();
        PC.AddComponent<Rigidbody>();

        //Position
        PC.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = PC;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(PC, "Create Physics Quad");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Forces/Gravity Object")]
    public static void CreateGO()
    {
        GameObject GO = new GameObject();
        GO.name = "Gravity Object";
        GO.AddComponent<GravityObject>();

        //Position
        GO.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = GO;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(GO, "Create Gravity Object");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Forces/Directional Gravity Object")]
    public static void CreateGDO()
    {
        GameObject GDO = new GameObject();
        GDO.name = "Directional Gravity Object";
        GDO.AddComponent<DirectionalGravity>();

        //Position
        GDO.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = GDO;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(GDO, "Create Directional Gravity Object");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Forces/Bipolar Magnet")]
    public static void CreateBM()
    {
        GameObject BM = new GameObject();
        BM.name = "Bipolar Magnet";
        BM.AddComponent<BipolarMagnet>();

        //Position
        BM.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = BM;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(BM, "Create Bipolar Magnet");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Forces/Density Object")]
    public static void CreateDO()
    {
        GameObject DO = new GameObject();
        DO.name = "Density Object";
        DO.AddComponent<DensityObject>();

        //Position
        DO.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = DO;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(DO, "Create Density Object");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Forces/Soft collision")]
    public static void CreateSC()
    {
        GameObject SC = new GameObject();
        SC.name = "Density Object";
        SC.AddComponent<SoftCollision>();

        //Position
        SC.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = SC;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(SC, "Create Soft collision");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Forces/Magnetic Properties")]
    public static void CreateMP()
    {
        GameObject MP = new GameObject();
        MP.name = "Density Object";
        MP.AddComponent<MagneticProperties>();

        //Position
        MP.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = MP;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(MP, "Create Magnetic Properties");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Forces/Rigidbody Windzone")]
    public static void CreateRWZ()
    {
        GameObject RWZ = new GameObject();
        RWZ.name = "Rigidbody Windzone";
        RWZ.AddComponent<RigidBodyWindZone>();

        //Position
        RWZ.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = RWZ;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(RWZ, "Create Rigidbody Windzone");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Forces/Global Water")]
    public static void CreateGW()
    {
        GameObject GW = new GameObject();
        GW.name = "Global Water";
        GW.AddComponent<GlobalWater>();
        GW.GetComponent<BoxCollider>().size = new Vector3(100, 10, 100);
        GW.GetComponent<BoxCollider>().center = new Vector3(0, 5, 0);
        GW.GetComponent<BoxCollider>().isTrigger = true;

        //Position
        GW.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = GW;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(GW, "Create Global water");
    }

    [MenuItem("GameObject/Forces and Physics Lab/Forces/Thruster")]
    public static void CreateT()
    {
        GameObject T = new GameObject();
        T.name = "Thruster";
        T.AddComponent<Thruster>();

        //Position
        T.transform.position = Camera.current.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        //Selection
        GameObject[] temp = new GameObject[1];
        temp[0] = T;
        Selection.objects = temp;

        Undo.RegisterCreatedObjectUndo(T, "Create Thruster");
    }

}

