using System.Threading.Tasks;
using ProjectX.Scripts.Player.Configs;
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
            IdleAnimation();
        }

        private void InitEnemy()
        {
            IdleAnimation();
            
            _data.maxHP = enemyEntityConfig.maxHP;
            _data.currentHP = enemyEntityConfig.maxHP;

            navMesh.stoppingDistance = enemyEntityConfig.attackDistance;
        }
        
        private Vector3 _currentRotation;
        private Vector2 _currentRotationVel;
        protected void Move(Vector3 destination)
        {
            if (!_isMoving && _canMove)
            {
                RunAnimation();
                _isMoving = true;
                navMesh.SetDestination(destination);
                // transform.LookAt(destination);
                // _currentRotation = Vector2.SmoothDamp(transform.rotation.eulerAngles,
                //     new Vector3(destination.x, destination.y, 0), ref _currentRotationVel,
                //     2f);
                // transform.rotation = Quaternion.Euler(0, _currentRotation.y, 0);
             
                Rotate(destination, enemyEntityConfig.rotationSpeed);
            }
        }

        private void Rotate(Vector3 destination, float speed = 1)
        {
            var look = Quaternion.LookRotation(destination - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, look, speed * Time.deltaTime);
        }

        protected void Attack(Vector3 destination)
        {
            Rotate(destination, enemyEntityConfig.rotationSpeed);
            IdleAnimation();
            AttackAnimation(destination);
        }

        private void IdleAnimation()
        {
            animator.SetFloat(Velocity, 0);
        }

        private void RunAnimation()
        {
            animator.SetFloat(Velocity, 1);
        }

        private async void AttackAnimation(Vector3 destination)
        {
            if (_canAttack)
            {
                _canAttack = false;
                _canMove = false;
                
                animator.SetBool(IsAttack, true);
                Rotate(destination, enemyEntityConfig.rotationSpeedOnAttack);
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