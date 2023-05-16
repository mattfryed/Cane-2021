using UnityEngine;

public class ThrusterBlastZone : MonoBehaviour {

    public float BlastMultiplier = 1f;
    public Thruster T;
    Rigidbody rb;
    Vector3 direction = new Vector3(0, 0, 0);
    float distance;

    public float Radius = 6f;
    public Vector3 cm = new Vector3(0, 0, 0);

    public bool Wobble;

    [Range(0,1)]
    public float Turbulence;
    public float Frequency;
    public float WobbleCurrent;
    Vector3 Force;

    public float ForceLimit = 6000;
    // Use this for initialization
    void Awake () {
        T = GetComponent<Thruster>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        if(T.CurPower > 0)
        {
            Collider[] hitColliders = new Collider[0];


            if (T.Thrust == Thruster.ThrustMode.Hover)
            {
                cm = transform.TransformDirection(new Vector3(0, 0, T.hoverHeight));
                hitColliders = Physics.OverlapSphere(transform.position + cm, T.hoverHeight);
            }
            else
            {
                cm = transform.TransformDirection(new Vector3(0, 0, Radius));
                hitColliders = Physics.OverlapSphere(transform.position + cm, Radius);
            }


            for (int i = 0; i < hitColliders.Length; i++)
            {
                rb = hitColliders[i].GetComponent<Rigidbody>();

                if (rb != null && rb != T.BoundRigidBody)
                {
                    distance = Mathf.Pow(Vector3.Distance(rb.transform.position, transform.position), 2);
                    direction = transform.position - rb.transform.position;
                    var halfWayVector = (direction + T.transform.forward).normalized;

                    Force = -((halfWayVector * ((T.ThrustForce * T.CurPower) / rb.mass)) / distance) * BlastMultiplier;


                    Force = Vector3.ClampMagnitude(Force, ForceLimit);
                    if (Force.magnitude > 0)
                    {
                        if (Wobble && Turbulence > 0)
                        {
                            WobbleCurrent = Mathf.PingPong((Time.time + (distance)) * Frequency, Turbulence);
                            rb.AddForce(Force * WobbleCurrent, T.Forcemode);
                        }
                        else
                        {
                            rb.AddForce(Force, T.Forcemode);
                        }
                    }


                }
            }
        }


    }

}
