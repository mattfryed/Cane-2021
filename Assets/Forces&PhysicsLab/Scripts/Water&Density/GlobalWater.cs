using UnityEngine;
using FPL_Forces;

[RequireComponent(typeof(BoxCollider))]

public class GlobalWater : DensityObject
{

    public enum FloatMode { Floats, Sinks };
    public FloatMode NonDensity;

    public float ThrustForce = 5.0f;

    public float SurfaceHeight = 10f;

    public float PressureDamp = 0.5f;
    public bool ApplyRotatingPressure = true;

    [Range(0f, 0.25f)]
    public float minimumPressure = 0.01f;

    Rigidbody rb;
    DensityObject Do;
    public Collider coll;

    Vector3 direction;
    float maxhitdistance;

    float willfloat;

    public LayerMask maskLayers = ~0;
    public ForceMode Forcemode = ForceMode.Acceleration;

    DensityForce forces;

    bool f;
    float d1;
    float d2;
    Vector3 hbp; 

    // Use this for initialization
    void Start()
    {
        coll = GetComponent<Collider>();
    }

    public static bool IsInLayerMask(int layer, LayerMask layermask)
    {
        return layermask == (layermask | (1 << layer));
    }

    private void OnTriggerStay(Collider other)
    {

        if (IsInLayerMask(other.gameObject.layer, maskLayers))
        {
            
            Do = other.GetComponent<DensityObject>();
            if (Do != null) { rb = Do.rBody; } else { rb = other.GetComponent<Rigidbody>(); }

            if (rb != null && (Do != null || NonDensity == FloatMode.Floats))
            {

                if (Do == null) { d1 = -1f; d2 = -1f;  } else { d1 = Density; d2 = Do.Density; }
                if (NonDensity == FloatMode.Floats) { f = true; } else { f = false; }

                forces = new DensityForce();

                forces.CalculatePressureGlobal(rBody, rb, coll, ThrustForce, d1, d2, f, PressureDamp, minimumPressure,SurfaceHeight);
                forces.AddPressure(rb, rBody, Forcemode);
                if (ApplyRotatingPressure) { forces.AddRotatingPressure(rb); }

            }
        }
    }
}
   

