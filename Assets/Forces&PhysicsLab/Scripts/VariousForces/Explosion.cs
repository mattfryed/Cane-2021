using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float delay = 1f;
    public float radius = 5f;
    public float upwardsModifier;
    public float power = 10f;

    protected Vector3 explosionPos = new Vector3();
    public LayerMask maskLayers = ~0;
    public LayerMask blockLayers = ~0;
    public enum Loop { Destroy, Repeat, Nothing }
    public Loop loop;

    public bool CheckIfBlocked = true;
    protected RaycastHit[] hits;
    protected Color col = Color.green;

    public bool NonAlloc = false;
    public int NonAllocBuffer = 100;
    protected Collider[] hitColliders = new Collider[0];

    private void Awake()
    {
        if (NonAlloc) { SetBuffer(NonAllocBuffer); }
    }

    protected virtual void Start()
    {
        Activate();
    }

    public virtual void Activate()
    {
        Invoke("Detonate", delay);
    }

    protected virtual void Detonate()
    {
        explosionPos = transform.position;

        if(!NonAlloc)
        {
            hitColliders = Physics.OverlapSphere(explosionPos, radius, maskLayers);
        }
        else
        {
            Physics.OverlapSphereNonAlloc(explosionPos, radius, hitColliders, maskLayers);
        }
        

        for (int i = 0; i < hitColliders.Length; i++)
        {
            if(hitColliders[i] != null)
            {
                if (CheckIfBlocked)
                {
                    col = Color.yellow;
                    hits = Physics.RaycastAll(explosionPos, (hitColliders[i].transform.position - explosionPos), radius, blockLayers);
                    if (hits.Length > 0)
                    {
                        col = Color.green;
                        bool h = true;
                        for (int z = 0; z < hits.Length; z++)
                        {

                            if (!hits[z].transform.gameObject.Equals(hitColliders[i].transform.gameObject) && !hits[z].transform.gameObject.Equals(this.transform.gameObject))
                            {
                                h = false;
                                col = Color.red;
                            }
                        }

                        if (h)
                        { Explode(hitColliders[i]); }
                    }
                    else
                    {
                        Explode(hitColliders[i]);
                    }

                }
                else
                {
                    col = Color.green;
                    Explode(hitColliders[i]);
                }

                Debug.DrawRay(explosionPos, (hitColliders[i].transform.position - explosionPos), col);
            }

        }

        if (loop == Loop.Destroy)
        {
            Destroy(gameObject);
        }
        else if (loop == Loop.Repeat)
        {
            Activate();
        }
    }

    protected void Explode(Collider col)
    {
        Rigidbody rb = col.GetComponent<Rigidbody>();

        if (rb != null)
            rb.AddExplosionForce(power, explosionPos, radius, upwardsModifier);
    }

    void SetBuffer(int NonAllocBuffer)
    {
        hitColliders = new Collider[NonAllocBuffer];
    }
}

