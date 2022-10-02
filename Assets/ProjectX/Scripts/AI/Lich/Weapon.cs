using UnityEngine;

namespace ProjectX.Scripts.AI.Lich
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private Collider collider;
        [SerializeField] private int damage;
        private bool _canGetDamage = true;
        public int Damage
        {
            get
            {
                _canGetDamage = false;
                return !_canGetDamage ? damage : 0;
            }
        }

        public void StartDamage()
        {
            collider.enabled = true;
        }

        public void StopDamage()
        {
            collider.enabled = false;
            _canGetDamage = true;
        }
    }
}