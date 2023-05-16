using System.Collections.Generic;
using UnityEngine;
using FPL_Forces;

[RequireComponent(typeof(ParticleSystem))]

public class PhysicsParticles : PhysicsObject
{
    public ParticleSystem particlesystem;
    ParticleSystem.Particle[] m_Particles = new ParticleSystem.Particle[0];
    public int ParticleBuffer;
    public bool EnableParticleProcessing;

    Vector3 worldPos;

    protected override void Awake()
    {
        base.Awake();
        particlesystem = GetComponent<ParticleSystem>();
        EnableForceProcessing = true;
        SetBuffer(ParticleBuffer);
    }
    public override void FixedUpdate()
    {

        if (CanBeAttracted)
        {
            if (gravityforces.Count > 0)
            {

                // GetParticles is allocation free because we reuse the m_Particles buffer between updates
                var numParticlesAlive = particlesystem.GetParticles(m_Particles);


                for (int ii = 0; ii < numParticlesAlive; ii++)
                {
                    for (int i = 0; i < gravityforces.Count; i++)
                    {
                        curPriority = gravityforces[i].Priority;
                        curForce = gravityforces[i];


                        if (EnableParticleProcessing)
                        {
                            if (particlesystem.main.simulationSpace == ParticleSystemSimulationSpace.Local)
                            {worldPos = transform.TransformPoint(m_Particles[ii].position); } else { worldPos = m_Particles[ii].position; }
                            
                            curForce.Force.CalculateGravity(curForce.Force.SelfPos, worldPos,
                            curForce.SelfRbody.mass, rBody.mass, 
                            curForce.Multiplier, curForce.NoDecay);
                        }

                        m_Particles[ii] = curForce.Force.AddParticleGravity(m_Particles[ii]);

                    }
                }
                gravityforces.Clear();
                curPriority = 0;

                // Apply the particle changes to the Particle System
                particlesystem.SetParticles(m_Particles, numParticlesAlive);

            }

        }
    }

    void SetBuffer(int NonAllocBuffer)
    {
        m_Particles = new ParticleSystem.Particle[NonAllocBuffer];
    }
}

