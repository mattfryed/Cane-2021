using UnityEngine;

namespace FPL_Forces {

    public class GravityForce
    {
        public Vector3 SelfPos = new Vector3(0,0,0);
        public Vector3 OtherPos = new Vector3(0, 0, 0);
        float force = 0f;
        float distance = 0f;
        public Vector3 direction = new Vector3(0, 0, 0);

        public void CalculateGravity(Rigidbody rbSelf, Rigidbody rbOther, float GravityMultiplier = 1f, bool NoDecay = false,Vector3 dir = default(Vector3))
        {
            UpdatePosition(rbSelf, rbOther); 
            UpdateDirection(dir);
            UpdateDistance(NoDecay);
            UpdateForce(rbSelf, rbOther, GravityMultiplier);
        }

        public void CalculateGravity(Vector3 Self, Vector3 Other, float massSelf, float massOther, float GravityMultiplier = 1f, bool NoDecay = false, Vector3 dir = default(Vector3))
        {
            UpdatePosition(Self,Other);
            UpdateDirection(dir);
            UpdateDistance(NoDecay);
            UpdateForce(massSelf, massOther, GravityMultiplier);
        }

        private void UpdatePosition(Rigidbody rbSelf, Rigidbody rbOther,bool NoDecay = false)
        {
        if (!NoDecay)
        {
            SelfPos = rbSelf.transform.position + (rbSelf.transform.TransformDirection(rbSelf.centerOfMass));
            OtherPos = rbOther.transform.position + (rbOther.transform.TransformDirection(rbOther.centerOfMass));
        }
        else
        {
           OtherPos = rbOther.transform.position;
        }

        }

        public void UpdatePosition(Vector3 Self, Vector3 Other)
        {
            SelfPos = Self;
            OtherPos = Other;
        }

        private void UpdateDirection(Vector3 dir = default(Vector3))
        {
            if (dir != default(Vector3)) { direction = dir; } else { direction = SelfPos - OtherPos; }
        }

        private float UpdateDistance(bool NoDecay = true)
        {

        if (!NoDecay){ distance = Mathf.Pow(Vector3.Distance(OtherPos, SelfPos),2); }
        else { distance = 1; }

        return distance;
        }

        private void UpdateForce(Rigidbody rbSelf, Rigidbody rbOther, float GravityMultiplier = 1f)
        {
            force = GravityMultiplier * ((rbSelf.mass * rbOther.mass) / distance);

        }

        private void UpdateForce(float massSelf, float massOther, float GravityMultiplier = 1f)
        {
            force = GravityMultiplier * ((massSelf * massOther) / distance);

        }

        public void AddGravity(Rigidbody rb, ForceMode Forcemode = ForceMode.Force)
        {
            if (distance > 0)
            {
                    rb.AddForce(direction * force, Forcemode);
            }
        }

        public ParticleSystem.Particle AddParticleGravity(ParticleSystem.Particle particle)
        {
            if (distance > 0)
            {
                particle.velocity += (direction*force);
            }

            return particle;
        }

        public void AddRotationalLock(Rigidbody rb,  ForceMode Forcemode = ForceMode.Acceleration, Vector3 OtherCenterOfMass = default(Vector3),float Strength = 1f, float Limit = 0f)
        {
            if (distance > 0)
            {
                Vector3 rbcm = rb.transform.TransformDirection(OtherCenterOfMass);
                float angleDiff = Vector3.Angle(rbcm, direction);
                if (Limit != 0f) { angleDiff = Mathf.Clamp(angleDiff - Limit, 0, Mathf.Infinity); }
                Vector3 cross = Vector3.Cross(rbcm, direction);
                rb.AddTorque((cross * angleDiff * force)*Strength, Forcemode);
            }
        }

        public void ClearSettings()
        {
            SelfPos = new Vector3(0, 0, 0);
            OtherPos = new Vector3(0, 0, 0);
            force = 1f;
            distance = 0f;
            direction = new Vector3(0, 0, 0);
        }

    }

}
