using UnityEngine;

namespace ProjectX.Scripts.Player.Configs
{
    [CreateAssetMenu (fileName = "PlayerConfig", menuName = "ProjectX/Configs/PlayerConfig")]
    public class PlayerControllerConfig : ScriptableObject
    {
        [Header("Walk")]
        public float moveSpeed = 1f;
        public float smoothRotateTime = .2f;
        [Header("Dash")]
        public float dashDuration = .2f;
        public float dashPower = 10;
        public float dashTimeOut = 1.5f;
        [Header("Attack")]
        public float attackDelay = 1f;
    }
}
