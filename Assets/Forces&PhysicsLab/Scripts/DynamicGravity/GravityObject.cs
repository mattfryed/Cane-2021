using UnityEngine;
using FPL_Forces;

public class GravityObject : PhysicsObject
{
    public enum FieldShape { Sphere, Cube };
    public FieldShape ForceShape;


    [SerializeField]
    public float GravityMultiplier = 1f;

    public float Radius = 50f;
    public Vector3 Size = new Vector3(100f, 100f, 100f);

    public int Priority;

    public bool NonAlloc = false;
    public int NonAllocBuffer = 100;
    public bool DisableLocking = false;
    public bool AffectAllRigidBodies = true;
    public bool NoDecay = false;
    public LayerMask maskLayers = ~0;

    public ForceMode Forcemode = ForceMode.Acceleration;

    PhysicsObject go;
    BipolarMagnet mo;
    Rigidbody rb;

    GravityForce forces;
    Collider[] hitColliders = new Collider[0];

    protected override void Awake()
    {
        base.Awake();
        if (NonAlloc) { SetBuffer(NonAllocBuffer); }
    }

    // Update is called once per frame
    public override void FixedUpdate()
    {

        if (!NonAlloc)
        {
            if (ForceShape == FieldShape.Sphere) { hitColliders = Physics.OverlapSphere(transform.position, Radius, maskLayers); }
            if (ForceShape == FieldShape.Cube) { hitColliders = Physics.OverlapBox(transform.position, Size, transform.rotation, maskLayers); }
        }
        else
        {

            if (ForceShape == FieldShape.Sphere) { Physics.OverlapSphereNonAlloc(transform.position, Radius, hitColliders, maskLayers); }
            if (ForceShape == FieldShape.Cube) { Physics.OverlapBoxNonAlloc(transform.position, Size, hitColliders, transform.rotation, maskLayers); }
        }



        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i] != null)
            {
                go = null;
                go = hitColliders[i].GetComponent<PhysicsObject>();

                if (go != null)
                {
                    if (go.CanBeAttracted)
                    {

                        forces = new GravityForce();
                        forces.CalculateGravity(rBody, go.rBody, GravityMultiplier, NoDecay);
                        if (!go.EnableForceProcessing)
                        {
                            forces.AddGravity(go.rBody, Forcemode);

                            if (go.AlignToForce && !DisableLocking)
                            {
                                forces.AddRotationalLock(go.rBody, Forcemode, go.centerOfMass, go.AlignStrength, go.AlignLimit);
                            }
                        }
                        else
                        {
                            //Wrap force and send it over to the receiver to evaluate.
                            ProcessableForce wrappedforce = new ProcessableForce();
                            wrappedforce.PassForce(Priority, forces, Forcemode, go.rBody, DisableLocking,NoDecay);

                            if (go is PhysicsParticles)
                            {
                                wrappedforce.SelfRbody = rBody;
                                wrappedforce.Multiplier = GravityMultiplier;
                            }

                            go.AddStack(wrappedforce);
                        }


                    }

                }
                else

                if (AffectAllRigidBodies)
                {
                    rb = null;
                    rb = hitColliders[i].GetComponent<Rigidbody>();

                    if (rb != null)
                    {
                        forces = new GravityForce();
                        forces.CalculateGravity(rBody, rb, GravityMultiplier, NoDecay);
                        forces.AddGravity(rb, Forcemode);
                    }
                }
            }
        }
    }

    void SetBuffer(int NonAllocBuffer)
    {
        hitColliders = new Collider[NonAllocBuffer];
    }
}



