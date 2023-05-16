using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Explosion
{
    public bool StartActive = true;
    float time;
    public float Countdown;

    protected override void Start()
    {
        ResetTime();

        if (StartActive)
        {
            Activate();
        }
    }

    private void Update()
    {
        CountDown();
    }

    protected void CountDown()
    {
        if(IsInvoking("Detonate"))
        {
            time += 1 * Time.deltaTime;
            Countdown = delay - time;
        }

    }

    public override void Activate()
    {
        ResetTime();
        Invoke("Detonate", delay);
    }

    protected void ResetTime()
    {
        time = 0;
        Countdown = delay;
    }
    
    void Defuse(bool reset = false)
    {
        if (reset) { ResetTime(); }
        CancelInvoke("Detonate");
    }

}

