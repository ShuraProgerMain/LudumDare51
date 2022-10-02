using UnityEngine;

namespace ProjectX.Scripts.Player.Configs
{
    [CreateAssetMenu (fileName = "PlayerConfig", menuName = "ProjectX/PlayerConfig")]
    public class PlayerControllerConfig : ScriptableObject
    {
        public float moveSpeed = 1f;
        public float smoothRotateTime = .2f;
        public float dashDuration = .2f;
        public float dashPower = 10;
        public float dashTimeOut = 1.5f;
    }
}
