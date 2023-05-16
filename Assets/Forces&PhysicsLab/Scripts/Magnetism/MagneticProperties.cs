using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class MagneticProperties : MonoBehaviour {

    public Rigidbody rBody;

    public bool CanBeAttracted = true;

    public Vector3 NorthPole = new Vector3(0, 0, 0);
    public Vector3 SouthPole = new Vector3(0, 0, 0);

    protected virtual void Awake()
    {
        rBody = GetComponent<Rigidbody>();

        NorthPole = rBody.centerOfMass;
        SouthPole = rBody.centerOfMass;
    }
}
