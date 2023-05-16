using UnityEngine;

public class Thruster : MonoBehaviour
{

    public enum ThrustMode { RocketPower, Hover };
    public ThrustMode Thrust;

    public bool Enabled;

    [Range(0, 100)]
    public float CurPower = 1f;

    public float hoverHeight = 10f;

    public Rigidbody BoundRigidBody;

    // The force applied per unit of distance below the desired height.
    public float ThrustForce = 5.0f;

    // The amount that the lifting force is reduced per unit of upward speed.
    // This damping tends to stop the object from bouncing after passing over
    // something.
    public float hoverDamp = 0.5f;

    public ForceMode Forcemode = ForceMode.Acceleration;

    // Use this for initialization
    void Start()
    {
        if (BoundRigidBody == null) { BoundRigidBody = GetComponent<Rigidbody>(); }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(CurPower > 0)
        {
            if (Thrust == ThrustMode.Hover)
            {
                RaycastHit hit;
                Ray ray = new Ray(transform.position, transform.forward);

                // Cast a ray straight downwards.
                if (Physics.Raycast(ray, out hit))
                {
                    float diff = hoverHeight - hit.distance;
                    if (diff > 0)
                    {
                        Vector3 dirspeed = BoundRigidBody.velocity - transform.forward;
                        float lift = diff * ThrustForce - dirspeed.magnitude * hoverDamp;
                        BoundRigidBody.AddForceAtPosition(-lift * transform.forward * (CurPower/100), transform.position, Forcemode);
                    }
                }
            }
            else
            {
                if (Thrust == ThrustMode.RocketPower)
                {
                    BoundRigidBody.AddForceAtPosition(-ThrustForce * transform.forward * (CurPower / 100), transform.position, Forcemode);
                    if (!Enabled) { CurPower = 0; }
                }
            }
        }

        // if (Continious) { BoundRigidBody.AddForceAtPosition(-transform.forward * power, transform.position, Forcemode); }
    }

    public void TurnOn(float val)
    {
        CurPower = val;
    }

    public void TurnOff(float val)
    {
        CurPower = val;
    }

}



