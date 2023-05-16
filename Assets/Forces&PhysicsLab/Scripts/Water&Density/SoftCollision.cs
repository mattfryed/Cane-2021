using UnityEngine;
using FPL_Forces;

public class SoftCollision : DensityObject {

    public enum FloatMode { PressedOutward, Sink, Ignore };
    public FloatMode NonDensity;

    public float ThrustForce = 5.0f;
    public bool DisableDensityCalculation;
    public bool CollideWithSoftCollision;

    public float PressureDamp = 0.5f;

    Rigidbody rb;
    DensityObject Do;
    Collider coll;
    bool f;
    float d1;
    float d2;

    public LayerMask maskLayers = ~0;
    public ForceMode Forcemode = ForceMode.Acceleration;

    DensityForce forces;

    // Use this for initialization
    void Awake () {
        coll = GetComponent<Collider>();
    }

    private void Start()
    {
        Softcollision = true;
    }

    private void OnTriggerStay(Collider other)
    {

        if (IsInLayerMask(other.gameObject.layer, maskLayers))
        {
            Do = other.GetComponent<DensityObject>();

            if(Do != null)
            {
                rb = Do.rBody;
                if ((Do.Softcollision && CollideWithSoftCollision) || (!Do.Softcollision))
                {
                    ExecuteCollision(rb,Do);
                }
            }
            else {
                rb = other.GetComponent<Rigidbody>();
                ExecuteCollision(rb,null);
            }

            

        }
    }

    public void ExecuteCollision(Rigidbody rb,DensityObject Do = null)
    {
        if (rb != null && (Do != null || NonDensity == FloatMode.PressedOutward || NonDensity == FloatMode.Sink))
        {
            if (NonDensity == FloatMode.PressedOutward) { f = true; } else { f = false; }

            if (Do == null || DisableDensityCalculation) { d1 = -1f; d2 = -1f; } else { d1 = Density; d2 = Do.Density; }

            forces = new DensityForce();
            forces.CalculatePressure(rBody, rb, coll, ThrustForce, d1, d2, f, rBody.centerOfMass, PressureDamp);
            forces.AddPressure(rb, rBody, Forcemode);
        }
    }

    public static bool IsInLayerMask(int layer, LayerMask layermask)
    {
        return layermask == (layermask | (1 << layer));
    }



}

