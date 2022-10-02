using UnityEngine;

namespace ProjectX.Scripts.AI.Lich
{
    public class Projectile : MonoBehaviour
    {
        public int damage;
        [SerializeField] private float speed;
        private void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}