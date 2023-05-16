using System.Collections.Generic;
using UnityEngine;
using FPL_Forces;

[RequireComponent(typeof(Rigidbody))]

public class PhysicsObject : MonoBehaviour {

    public Rigidbody rBody;

    public Vector3 centerOfMass = new Vector3(0, 0, 0);
    protected Vector3 cm = new Vector3(0, 0, 0);


    public bool CanBeAttracted = true;

    public bool AlignToForce = false;
    public float AlignStrength = 1f;
    public float AlignLimit = 0f;

    public bool EnableForceProcessing;
    protected int curPriority;
    protected ProcessableForce curForce;
    public List<ProcessableForce> gravityforces = new List<ProcessableForce>();

    protected virtual void Awake()
    {
        rBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        SetCenterOfMass(centerOfMass);
    }

    public void SetCenterOfMass(Vector3 center)
    {
        cm = transform.TransformDirection(center);
        rBody.centerOfMass = center;
    }

    public void SetAutoCenterOfMass()
    {
        rBody.ResetCenterOfMass();
        centerOfMass = rBody.centerOfMass;
    }

    public void SetAlignToForce(bool align, float strength = 1f, float limit = 0f)
    {
        AlignToForce = align;
        AlignStrength = strength;
        AlignLimit = limit;
    }

    public virtual void FixedUpdate()
    {
        if(EnableForceProcessing && CanBeAttracted && gravityforces.Count > 0)
        {
            for (int i = 0; i < gravityforces.Count; i++)
            {
                if(gravityforces[i] != null)
                {
                    if (gravityforces[i].Priority >= curPriority)
                    {
                        curPriority = gravityforces[i].Priority;
                        curForce = gravityforces[i];                    
                    }
                }

            }


            curForce.Force.AddGravity(curForce.OtherRbody, curForce.Forcemode);
            gravityforces.Clear();
            curPriority = 0;

        }
    }

    public void AddStack(ProcessableForce wrappedforces)
    {
        gravityforces.Add(wrappedforces);
    }


}

public class ProcessableForce
    {
    public int Priority = 0;

    public GravityForce Force;

    public Rigidbody SelfRbody;
    public Rigidbody OtherRbody;
    public ForceMode Forcemode;

    public bool NoDecay = false;
    public bool DisableAlign = false;
    public float Multiplier = 1f;
    public Vector3 Direction = default(Vector3);

    public void PassForce(int priority, GravityForce force, ForceMode forcemode = ForceMode.Acceleration, Rigidbody otherrb = null, bool disablealign = false, bool nodecay = false)
    {
        Force = force;
        if (otherrb != null) { OtherRbody = otherrb; }
        Forcemode = forcemode;
        Priority = priority;
        DisableAlign = disablealign;
        NoDecay = nodecay;
    }

}
