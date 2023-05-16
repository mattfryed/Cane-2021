using UnityEngine;

namespace InteractiveGrass.Scripts
{
    public class Movement : MonoBehaviour
    {
        [SerializeField] private float _speed;
        void Update()
        {
            var direction = Vector3.zero;
            if(Input.GetKey(KeyCode.W)) direction += Vector3.forward;
            if(Input.GetKey(KeyCode.S)) direction += Vector3.back;
            if(Input.GetKey(KeyCode.A)) direction += Vector3.left;
            if(Input.GetKey(KeyCode.D)) direction += Vector3.right;
            direction *= _speed;
            transform.position += direction;
        }
    }
}
