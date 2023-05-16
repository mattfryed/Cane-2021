using UnityEngine;

namespace FPL_Forces {

    public class DensityForce
    {
        ForceMode Forcemode;

        public bool StaticUpward = false;

        float willfloat;
        Vector3 Pos1 = new Vector3(0, 0, 0);
        Vector3 Pos2 = new Vector3(0, 0, 0);
        float lift;
        Vector3 direction = new Vector3(0, 0, 0);
        float maxhitdistance;
        float hitdistance;
        float diff;

        Vector3 pos = new Vector3(0, 0, 0);

        public enum FloatMode { Float, Sink, Calculate };
        FloatMode Float;

        public void CalculatePressure(Rigidbody rbSelf, Rigidbody rbOther, Collider coll, float PressureMultiplier = 1f,  float Density1 = -1f,float Density2 = -1f, bool F = true,  Vector3 centerofmass = default(Vector3), float PressureDamp = 1f, float minimumPressure = 0f)
        {
            if (StaticUpward) { StaticUpward = false; }
            UpdatePosition(rbSelf, rbOther, centerofmass);
            UpdateFloat(F, Density1, Density2, minimumPressure);
            UpdateColliderPenetration(coll, rbSelf, rbOther);
            UpdateForce(rbOther, PressureMultiplier,PressureDamp);
        }

        private void UpdatePosition(Rigidbody rbSelf, Rigidbody rbOther, Vector3 centerofmass, Collider coll = null)
        {
            if(!StaticUpward)
            {
                Pos1 = rbSelf.transform.position + (rbSelf.transform.TransformDirection(centerofmass));
                Pos2 = rbOther.transform.position;
            }
            else
            {
                Pos2 = rbOther.transform.position;
                Pos1 = new Vector3(rbOther.transform.position.x, coll.bounds.min.y, rbOther.transform.position.z);
            }
        }

        private void UpdateFloat(bool F, float Density1, float Density2, float minimumPressure = 0f)
        {
            if (Density1 >= 0 || Density2 >= 0)
            {
                Float = FloatMode.Calculate;
                if (!StaticUpward) { willfloat = Density1 - Density2; } else { willfloat = Mathf.Clamp(Density1 - Density2, 0, Mathf.Infinity) + minimumPressure; }
            }
            else {
                willfloat = 1f;
                if (F) { Float = FloatMode.Float; }
                else { Float = FloatMode.Sink; }
            }

        }

        private void UpdateColliderPenetration(Collider coll, Rigidbody rbSelf, Rigidbody rbOther, float SurfaceHeight = 0f)
        {
            if (!StaticUpward)
            {
                Physics.ComputePenetration(coll, rbSelf.transform.position, rbSelf.transform.rotation, rbOther.GetComponent<Collider>(), rbOther.transform.position, rbOther.transform.rotation, out direction, out maxhitdistance);
                direction = (Pos2 - Pos1).normalized;
            }
            else
            {
                direction = -Physics.gravity.normalized;
                maxhitdistance = (coll.bounds.min.y + SurfaceHeight) - coll.bounds.min.y;
                hitdistance = Vector3.Distance(Pos2, Pos1);
            }

        }

        private void UpdateForce(Rigidbody rbOther,float PressureMultiplier = 1f,float PressureDamp = 1f)
        {
            diff = ((maxhitdistance+0) - hitdistance);
            
            if (diff > 0)
            {
                Vector3 dirspeed = rbOther.velocity - direction;
                lift = diff * (PressureMultiplier * willfloat) - dirspeed.magnitude * (1 - PressureDamp);
            }
        }

        public void AddPressure(Rigidbody rbOther, Rigidbody rbSelf, ForceMode Forcemode)
        {
            if (diff > 0)
            {

                if (StaticUpward)
                {
                    if (lift > 0) { rbOther.AddForce((lift * direction), Forcemode); }
                }
                else if (Float == FloatMode.Calculate)
                {
                    if (lift > 0) { rbOther.AddForce((lift * direction), Forcemode); }
                    else { rbSelf.AddForce((lift * direction), Forcemode); }
                }
                else if (Float == FloatMode.Float)
                {
                    rbOther.AddForce(lift * direction, Forcemode);
                }
                else if (Float == FloatMode.Sink)
                {
                    rbSelf.AddForce(-(lift * direction), Forcemode);
                }
            }

        }

        public void ClearSettings()
        {
        Forcemode  = ForceMode.Force;
        StaticUpward = false;
        willfloat = 0f;
        Pos1 = new Vector3(0, 0, 0);
        Pos2 = new Vector3(0, 0, 0);
            lift = 0f;
            direction = new Vector3(0, 0, 0);
            maxhitdistance = 0f;
            hitdistance = 0f;
            diff = 0f;
        }

        public void CalculatePressureGlobal(Rigidbody rbSelf, Rigidbody rbOther, Collider coll, float PressureMultiplier = 1f, float Density1 = -1f, float Density2 = -1f, bool F = true, float PressureDamp = 1f, float minimumPressure = 0f, float SurfaceHeight = 10f)
        {
            if (!StaticUpward) { StaticUpward = true; }

            UpdatePosition(rbSelf, rbOther, new Vector3(0,0,0), coll);
            UpdateFloat(F, Density1, Density2, minimumPressure);
            UpdateColliderPenetration(coll, rbSelf, rbOther, SurfaceHeight);
            UpdateForce(rbOther, PressureMultiplier,PressureDamp);
        }

        public void AddRotatingPressure(Rigidbody rbOther)
        {
            if(StaticUpward)
            {
                if (diff > 0 && rbOther.GetComponent<MeshFilter>())
                {
                    MeshFilter mesh = rbOther.GetComponent<MeshFilter>();
                    Vector3[] vertices = mesh.mesh.vertices;
                    float lowest = Mathf.Infinity;
                    float highest = Mathf.Infinity;

                    Vector3 temp = new Vector3(0, 0, 0);
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vector3 or = Pos1;
                        float d1 = Vector3.Distance(rbOther.transform.TransformPoint(vertices[i]), or);
                        if (d1 < lowest) { lowest = d1; temp = rbOther.transform.TransformPoint(vertices[i]); }
                        if (d1 > highest) { highest = d1; }
                        i++;
                    }
                    pos = temp;
                    pos = temp + (rbOther.transform.position - temp) / 2f;
                    rbOther.AddForceAtPosition((lift * direction) / lift, pos, Forcemode);
                }
            }
        }
    }

}
