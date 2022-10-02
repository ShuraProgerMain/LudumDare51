using ProjectX.Scripts.StateMachine;
using UnityEngine;

namespace ProjectX.Scripts.AI
{
    [CreateAssetMenu (menuName = "ProjectX/LichIdle")]
    public class LichIdle : EnemyState
    {
        public override void StartState()
        {
            _animator.SetFloat(Velocity, 0);
        }

        public override void StopState()
        {
            
        }
    }
    
    [CreateAssetMenu (menuName = "ProjectX/LichRun")]
    public class LichRun : EnemyState
    {
        public override void StartState()
        {
            _animator.SetFloat(Velocity, 1);

        }

        public override void StopState()
        {
            _animator.SetFloat(Velocity, 0);
        }
    }
    
    [CreateAssetMenu (menuName = "ProjectX/LichAttack")]
    public class LichAttack : EnemyState
    {
        public override void StartState()
        {
            
        }

        public override void StopState()
        {
            
        }
    }
    
    [CreateAssetMenu (menuName = "ProjectX/LichAttack")]
    public class LichDeath : EnemyState
    {
        public override void StartState()
        {
            
        }

        public override void StopState()
        {
            
        }
    }
}