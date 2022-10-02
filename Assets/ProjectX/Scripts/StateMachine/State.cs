using ProjectX.Scripts.Player.Configs;
using UnityEngine;

namespace ProjectX.Scripts.StateMachine
{
    public enum StateType
    {
        Idle = 0,
        Run = 1,
        Attack = 2,
        Death = 3
    }
    
    public abstract class State : ScriptableObject
    {
        [SerializeField] protected StateType state;

        public StateType CurrentState => state;

        public abstract void StartState();
        public abstract void StopState();
    }

    public abstract class EnemyState : State
    {
        protected EnemyEntityConfig _config;
        protected Animator _animator;
        
        [Header("AnimationKeys")]
        protected readonly int Velocity = Animator.StringToHash("Velocity");
        protected readonly int IsAttack = Animator.StringToHash("IsAttack");
        protected readonly int IsDeath = Animator.StringToHash("IsDeath");


        public void Init(EnemyEntityConfig config, Animator animator)
        {
            _config = config;
            _animator = animator;
        }
    }
}