using ProjectX.Scripts.AI.Lich;
using UnityEngine;

namespace ProjectX.Scripts.Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private Weapon weapon;
        public void Init(int damage)
        {
            weapon.Damage = damage;
        }
        
        public void StartDamage()
        {
            weapon.StartDamage();
        }

        public void StopDamage()
        {
            weapon.StopDamage();
        }
    }
}
