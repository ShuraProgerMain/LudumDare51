using System.Threading.Tasks;
using ProjectX.Scripts.AI.Lich;
using ProjectX.Scripts.Player.Configs;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectX.Scripts.AI
{
    public class EnemyEntityState
    {
        public int maxHP;
        public int currentHP;

        public EnemyEntityState(int maxHP)
        {
            this.maxHP = maxHP;
            currentHP = this.maxHP;
        }
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EnemyEntity : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] protected NavMeshAgent navMesh;
        [SerializeField] protected EnemyEntityConfig enemyEntityConfig;

        private EnemyEntityState _state;

        [Header("AnimationKeys")] 
        private readonly int Velocity = Animator.StringToHash("Velocity");
        private readonly int IsAttack = Animator.StringToHash("IsAttack");
        private readonly int IsDeath = Animator.StringToHash("IsDeath");

        [SerializeField]private bool _canMove = true;
        [SerializeField]private bool _isMoving = false;
        [SerializeField]private bool _canAttack = true;

        private void Awake()
        {
            InitEnemy();
            IdleAnimation();
        }

        private void InitEnemy()
        {
            IdleAnimation();

            _state = new EnemyEntityState(enemyEntityConfig.maxHP);

            navMesh.stoppingDistance = enemyEntityConfig.attackDistance;
        }
        
        private Vector3 _currentRotation;
        private Vector2 _currentRotationVel;
        protected void Move(Vector3 destination)
        {
            if (!_canAttack)
            {
                _canAttack = true;
                _isMoving = false;
            }
            
            if (_canMove)
            {
                RunAnimation();
                _isMoving = true;
                navMesh.SetDestination(destination);

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
            _isMoving = false;
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

        private async void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Weapon>(out var weapon))
            {
                _state.currentHP -= weapon.Damage;   
            }

            if (_state.currentHP <= 0)
            {
                DeathAnimation();
                this.enabled = false;
                await Task.Delay(2000);
                gameObject.SetActive(false);
            }
        }
    }
}