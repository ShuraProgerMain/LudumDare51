using UnityEngine;

namespace ProjectX.Scripts.Player.Configs
{
    [CreateAssetMenu (fileName = "PlayerConfig", menuName = "ProjectX/Configs/PlayerConfig")]
    public class PlayerControllerConfig : ScriptableObject
    {
        public int maxHP = 10;
        public float maxStamina = 100;
        public int damage = 2;
        
        [Header("Walk")]
        public float moveSpeed = 1f;
        [Range(.15f, 2f)]
        public float smoothRotateTime = .15f;
        public float help = .2f;
        [Header("Dash")]
        public float dashDuration = .2f;
        public float dashPower = 10;
        public float dashTimeOut = 1.5f;
        [Header("Attack")]
        public float attackDelay = 1f;
    }
}
