using UnityEngine;

[RequireComponent(typeof(Rigidbody))]


public class RigidBodyWindZone : MonoBehaviour
{
    public WindZone master;
    public enum Mode {Directional, Spherical}
    public Mode mode;
    public float Multiplier = 1;
    public float Main = 1f;
    public float Turbulence = 1f;
    public float PulseMagnitude = 0.5f;
    public float PulseFrequency = 0.01f;

    public float Radius = 50f;

    public LayerMask maskLayers = ~0;

    float windForce;
    float force;

    float WobbleCurrent;
    float distance = 1;
    Vector3 Force;

    public bool NonAlloc = false;
    public int NonAllocBuffer = 100;
    Collider[] hitColliders = new Collider[0];

    public ForceMode Forcemode = ForceMode.Force;

    Rigidbody rb;
    Vector3 dir = new Vector3(0, 0, 0);

    private void Awake()
    {
        if (NonAlloc) { SetBuffer(NonAllocBuffer); }
    }

    void Update()
    {
        if (Turbulence > 0) { windForce = Main + Mathf.PingPong(Time.deltaTime * (Turbulence*PulseFrequency), Turbulence); } else { windForce = Main; }
        //windForce = Main;

        if (master != null)
        {
            if (master.mode == 0) { mode = Mode.Directional; } else {
                mode = Mode.Spherical;
                Radius = master.radius;
            }

            Main = master.windMain;
            Turbulence = master.windTurbulence;
            PulseMagnitude = master.windPulseMagnitude;
            PulseFrequency = master.windPulseFrequency;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (!NonAlloc)
        {
            Physics.OverlapSphereNonAlloc(transform.position, Radius,hitColliders, maskLayers);
        }
        else
        {
            hitColliders = Physics.OverlapSphere(transform.position, Radius, maskLayers);
        }

        

        for (int i = 0; i < hitColliders.Length; i++)
        {
            rb = hitColliders[i].gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
            distance = Vector3.Distance(rb.transform.position, transform.position);
            if (PulseMagnitude > 0) { WobbleCurrent = Mathf.PingPong(((Time.time + distance) * (PulseMagnitude*PulseFrequency)), PulseMagnitude); } else { WobbleCurrent = 0; }

                if (mode == Mode.Directional) { dir = transform.forward; }
            else { dir = hitColliders[i].transform.position - transform.position; }

            Force = dir * ((windForce / rb.mass)) * Multiplier;

            rb.AddForce(Force * WobbleCurrent, Forcemode);
            }
        }

    }


    void SetBuffer(int NonAllocBuffer)
    {
        hitColliders = new Collider[NonAllocBuffer];
    }


}



