using System;
using ProjectX.Scripts.Player;
using UnityEngine;

namespace ProjectX.Scripts.AI.Lich
{
    public class AI : EnemyEntity
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private Transform shotPoint;
        [SerializeField] private Projectile prefab;
        private void Update()
        {
            var distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= enemyEntityConfig.attackDistance)
            {
                Attack(player.transform.position);
            }
            else if (distance < 10f)
            {
                Move(player.transform.position);
            }
        }

        public void ShootLich()
        {
            var bullet = Instantiate(prefab, shotPoint.position, Quaternion.identity);
            bullet.transform.rotation = shotPoint.rotation;
            bullet.damage = enemyEntityConfig.attackDamage;
            Destroy(bullet.gameObject, 10);
        }
    }
}
