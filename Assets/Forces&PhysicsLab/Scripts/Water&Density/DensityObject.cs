using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class DensityObject : MonoBehaviour
{
    public float Density;

    public Rigidbody rBody;

    public bool Softcollision = false;

    // Use this for initialization
    void Awake()
    {
        rBody = GetComponent<Rigidbody>();
    }


}

