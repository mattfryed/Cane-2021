using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrassFlow.Examples {
    public class ContactFlatten: MonoBehaviour {

        public Collider grassCol;
        public int grassLayer = -1;
        public Texture2D flatTex;
        public GrassFlowRenderer grassFlow;
        public float flatStrength;
        public float flatSize;

        private void Start() {
            GrassFlowRenderer.SetPaintBrushTexture(flatTex);
        }

        Ray ray;
        RaycastHit hit;
        ContactPoint contact;
        private void OnCollisionStay(Collision collision) {
            if (enabled) {
                if (collision.transform == grassCol.transform || collision.gameObject.layer == grassLayer) {

#if UNITY_2018_3_OR_NEWER
                    contact = collision.GetContact(0);
#else
                    contact = collision.contacts[0];
#endif

                    ray = new Ray(contact.point + contact.normal * 0.1f, -contact.normal);
                    if (contact.otherCollider.Raycast(ray, out hit, 0.2f)) {
                        var gMesh = grassFlow.GetGrassMeshFromTransform(hit.transform);

                        //need to offset the hit coord of the raycast by half a pixel so that it lines up properly
                        Vector2 coord = hit.textureCoord - gMesh.paramMapHalfPixUV;

                        grassFlow.PaintParameters(gMesh, coord, flatSize, flatStrength, 0, 0, -1, 0, new Vector2(0f, 0.9f));
                    }
                }
            }
        }

    }
}
