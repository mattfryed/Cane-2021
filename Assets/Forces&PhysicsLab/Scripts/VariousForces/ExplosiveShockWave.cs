using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveShockWave : Bomb 
{
    public float ShockWaveTime;
    public float speed;
    float curRadius;
    bool execute;

    protected override void Detonate()
    {
        execute = true;
        speed =  radius / ShockWaveTime;
    }

    public void Update()
    {

        curRadius += speed * Time.deltaTime;
        if(execute)
        {
            explosionPos = transform.position;

            if (!NonAlloc)
            {
                hitColliders = Physics.OverlapSphere(explosionPos, curRadius, maskLayers);
            }
            else
            {
                Physics.OverlapSphereNonAlloc(explosionPos, curRadius, hitColliders, maskLayers);
            }


            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i] != null)
                {
                    if (CheckIfBlocked)
                    {
                        col = Color.yellow;
                        hits = Physics.RaycastAll(explosionPos, (hitColliders[i].transform.position - explosionPos), curRadius, blockLayers);
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

        }
        if(curRadius >= radius)
        {
            ResetTime();
            curRadius = 0;
            execute = false;

            if (loop == Loop.Destroy)
            {
                Destroy(gameObject);
            }
            else if (loop == Loop.Repeat)
            {
                Activate();
            }
        }

    }


}

