using System;
using System.Linq;
using System.Threading.Tasks;
using ProjectX.Scripts.Player.Configs;
using ProjectX.Scripts.StateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectX.Scripts.AI
{
    public class EnemyEntityData
    {
        public int maxHP;
        public int currentHP;
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EnemyEntity : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] protected NavMeshAgent navMesh;
        [SerializeField] protected EnemyEntityConfig enemyEntityConfig;

        private EnemyEntityData _data = new EnemyEntityData();

        [Header("AnimationKeys")] 
        private readonly int Velocity = Animator.StringToHash("Velocity");
        private readonly int IsAttack = Animator.StringToHash("IsAttack");
        private readonly int IsDeath = Animator.StringToHash("IsDeath");

        private bool _canMove = true;
        private bool _isMoving = false;
        private bool _canAttack = true;

        private void Awake()
        {
            InitEnemy();
            ChangeState(StateType.Idle);
        }

        private void InitEnemy()
        {
            IdleAnimation();
            
            _data.maxHP = enemyEntityConfig.maxHP;
            _data.currentHP = enemyEntityConfig.maxHP;

            navMesh.stoppingDistance = enemyEntityConfig.attackDistance;
        }

        protected void ChangeState(StateType stateType)
        {
            switch (stateType)
            {
                case StateType.Idle:
                    IdleAnimation();
                    break;
                case StateType.Run:
                    RunAnimation();
                    break;
                case StateType.Attack:
                    AttackAnimation();
                    break;
                case StateType.Death:
                    DeathAnimation();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null);
            }

            GC.Collect();
        }

        protected void Move(Vector3 destination)
        {
            if (!_isMoving && _canMove)
            {
                RunAnimation();
                _isMoving = true;
                navMesh.SetDestination(destination);
                transform.LookAt(destination);
            }
        }

        protected void Attack(Vector3 destination)
        {
            Debug.Log("Attack");
            transform.LookAt(destination);
            IdleAnimation();
            AttackAnimation();
        }

        private void IdleAnimation()
        {
            animator.SetFloat(Velocity, 0);
        }

        private void RunAnimation()
        {
            animator.SetFloat(Velocity, 1);
        }

        private async void AttackAnimation()
        {
            if (_canAttack)
            {
                _canAttack = false;
                _canMove = false;
                
                animator.SetBool(IsAttack, true);
                await Task.Delay(1000);
                animator.SetBool(IsAttack, false);
                _canMove = true;
                await Task.Delay((int)(1000 * enemyEntityConfig.attackDelay));
                _canAttack = true;
            }
        }

        private void DeathAnimation()
        {
            animator.SetTrigger(IsDeath);
        }
    }
}