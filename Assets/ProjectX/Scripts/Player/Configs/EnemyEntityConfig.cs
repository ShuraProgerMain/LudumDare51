using UnityEngine;

namespace ProjectX.Scripts.Player.Configs
{
    [CreateAssetMenu (fileName = "PlayerConfig", menuName = "ProjectX/Configs/Enemy")]
    public class EnemyEntityConfig : ScriptableObject
    {
        [Header("Move")]
        public float moveSpeed = 1f;
        public int maxHP;
        [Header("Attack")]
        public float attackDistance = 1f;
        public float attackDelay = 1f;
        public int attackDamage = 1;
    }
}