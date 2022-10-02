using UnityEngine;

namespace ProjectX.Scripts.AI.Lich
{
    public class Projectile : MonoBehaviour
    {
        public int damage;
        [SerializeField] private float speed;
        [SerializeField] private Rigidbody rigidbody;
        private void FixedUpdate()
        {
            rigidbody.velocity += Vector3.forward * speed;
        }
    }
}