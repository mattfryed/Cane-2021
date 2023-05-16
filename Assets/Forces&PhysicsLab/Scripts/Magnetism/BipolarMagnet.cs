using UnityEngine;
using FPL_Forces;

[RequireComponent(typeof(Rigidbody))]


public class BipolarMagnet : MagneticProperties
{

    public float MagneticMultiplier = 1f;

    public float Radius;

    public bool NonAlloc = false;
    public int NonAllocBuffer = 100;
    public bool NoDecay = false;
    public LayerMask maskLayers = ~0;

    public ForceMode Forcemode = ForceMode.Acceleration;

    MagneticProperties mo;

    MagneticForce forces;

    Collider[] hitColliders = new Collider[0];

    protected override void Awake()
    {
        rBody = GetComponent<Rigidbody>();
        if (NonAlloc) { SetBuffer(NonAllocBuffer); }
    }

    void FixedUpdate()
    {

        Vector3 ncm = transform.TransformDirection(NorthPole);
        Vector3 scm = transform.TransformDirection(SouthPole);

        if (!NonAlloc)
        {
            hitColliders = Physics.OverlapCapsule(transform.position + scm, transform.position + ncm, Radius);
        }
        else
        {
            Physics.OverlapCapsuleNonAlloc(transform.position + scm, transform.position + ncm, Radius, hitColliders);
        }

        for (int i = 0; i < hitColliders.Length; i++)
        {
            if(hitColliders[i] != null)
            {
                mo = hitColliders[i].GetComponent<MagneticProperties>();

                if (mo != null)
                {
                    if (mo.CanBeAttracted)
                    {
                        forces = new MagneticForce();
                        forces.CalculateMagnetism(MagneticMultiplier, rBody, mo.rBody, NoDecay, NorthPole, mo.NorthPole, SouthPole, mo.SouthPole);
                        forces.AddMagneticForce(mo.rBody, Forcemode);
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
