using UnityEngine;

namespace FPL_Forces {

    public class MagneticForce
    {
        Vector3 SelfPos1 = new Vector3(0, 0, 0);
        Vector3 SelfPos2 = new Vector3(0, 0, 0);
        Vector3 OtherPos1 = new Vector3(0, 0, 0);
        Vector3 OtherPos2 = new Vector3(0, 0, 0);

        float nAttractForce = 0f;
        float nRepelForce = 0f;
        float sAttractForce = 0f;
        float sRepelForce = 0f;

        float ntondist = 0f;
        float ntosdist = 0f;
        float stosdist = 0f;
        float stondist = 0f;

        public void CalculateMagnetism(float MagneticMultiplier, Rigidbody rbSelf, Rigidbody rbOther, bool NoDecay = false, Vector3 SelfNorth = default(Vector3), Vector3 OtherNorth = default(Vector3), Vector3 SelfSouth = default(Vector3), Vector3 OtherSouth = default(Vector3))
        {
            UpdatePosition(rbSelf, rbOther, SelfNorth, OtherNorth, SelfSouth, OtherSouth);
            UpdateDistance(NoDecay);
            UpdateForce(rbSelf, rbOther, MagneticMultiplier);
            SelfPos1 = rbSelf.transform.position + rbSelf.transform.TransformDirection(SelfNorth);
            SelfPos2 = rbSelf.transform.position + rbSelf.transform.TransformDirection(SelfSouth);
            OtherPos1 = rbOther.transform.position + rbOther.transform.TransformDirection(OtherNorth);
            OtherPos2 = rbOther.transform.position + rbOther.transform.TransformDirection(OtherSouth);
        }

        private void UpdatePosition(Rigidbody rbSelf, Rigidbody rbOther, Vector3 SelfNorth = default(Vector3), Vector3 OtherNorth = default(Vector3), Vector3 SelfSouth = default(Vector3), Vector3 OtherSouth = default(Vector3))
        {
            SelfPos1 = rbSelf.transform.position + rbSelf.transform.TransformDirection(SelfNorth);
            SelfPos2 = rbSelf.transform.position + rbSelf.transform.TransformDirection(SelfSouth);
            OtherPos1 = rbOther.transform.position + rbOther.transform.TransformDirection(OtherNorth);
            OtherPos2 = rbOther.transform.position + rbOther.transform.TransformDirection(OtherSouth);
        }

        private void UpdateDistance(bool NoDecay)
        {
            if (!NoDecay)
            {
                ntondist = Mathf.Pow(Vector3.Distance(OtherPos1, SelfPos1), 2);
                ntosdist = Mathf.Pow(Vector3.Distance(OtherPos2, SelfPos1), 2);
                stosdist = Mathf.Pow(Vector3.Distance(OtherPos2, SelfPos2), 2);
                stondist = Mathf.Pow(Vector3.Distance(OtherPos2, SelfPos1), 2);
            }
            else
            {
                ntondist = 1f;
                ntosdist = 1f;
                stosdist = 1f;
                stondist = 1f;
            }
        }

        private void UpdateForce(Rigidbody rbSelf, Rigidbody rbOther, float MagneticMultiplier = 1f)
        {
            nAttractForce = MagneticMultiplier * ((rbSelf.mass * rbOther.mass) / ntosdist);
            nRepelForce = -MagneticMultiplier * ((rbSelf.mass * rbOther.mass) / ntondist);
            sAttractForce = MagneticMultiplier * ((rbSelf.mass * rbOther.mass) / stondist);
            sRepelForce = -MagneticMultiplier * ((rbSelf.mass * rbOther.mass) / stosdist);
        }

        public void AddMagneticForce(Rigidbody rb, ForceMode Forcemode = ForceMode.Acceleration)
        {
                if (ntosdist > 0) { rb.AddForceAtPosition((SelfPos2 - OtherPos1) * nAttractForce, OtherPos1, Forcemode); }
                if (ntondist > 0) { rb.AddForceAtPosition((SelfPos1 - OtherPos1) * nRepelForce, OtherPos1, Forcemode); }
                if (stosdist > 0) { rb.AddForceAtPosition((SelfPos2 - OtherPos2) * sRepelForce, OtherPos2, Forcemode); }
                if (stondist > 0) { rb.AddForceAtPosition((SelfPos1 - OtherPos2) * sAttractForce, OtherPos2, Forcemode); }
        }

        public void ClearSettings()
        {
            SelfPos1 = new Vector3(0, 0, 0);
            SelfPos2 = new Vector3(0, 0, 0);
            OtherPos1 = new Vector3(0, 0, 0);
            OtherPos2 = new Vector3(0, 0, 0);

            nAttractForce = 0f;
            nRepelForce = 0f;
            sAttractForce = 0f;
            sRepelForce = 0f;

            ntondist = 0f;
            ntosdist = 0f;
            stosdist = 0f;
            stondist = 0f;
        }
    }

}
