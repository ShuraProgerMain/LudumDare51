using System;
using System.Collections;
using System.Threading.Tasks;
using ProjectX.Scripts.AI.Lich;
using ProjectX.Scripts.Player.Configs;
using ProjectX.Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectX.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Vector2 rotationPower = new(10, 2);
        [SerializeField] private PlayerControllerConfig playerControllerConfig;
        [SerializeField] private PlayerAttack _playerAttack;

        private PlayerInputActions _playerInputActions;
        private PlayerState _playerState;
        private Vector3 _currentRotation;
        private Vector2 _currentRotationVel;
        private static readonly int IsAttack = Animator.StringToHash("IsAttack");
        private readonly int Horizontal = Animator.StringToHash("Horizontal");
        private readonly int Vertical = Animator.StringToHash("Vertical");

        private PlayerMovement _playerMovement;
        private ThirdPersonCameraMovement _thirdPersonCameraMovement;

        private Vector2 _moveDirection;

        private void Awake()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.InGameActions.Enable();
            _playerState = new PlayerState(playerControllerConfig.maxHP, playerControllerConfig.maxStamina);

            _playerMovement = new PlayerMovement();
            _thirdPersonCameraMovement = new ThirdPersonCameraMovement();
            _playerAttack.Init(playerControllerConfig.damage);

            SubscribeAll();
        }

        private void SubscribeAll()
        {
            _playerInputActions.InGameActions.Move.performed += context =>
            {
                _moveDirection = context.ReadValue<Vector2>();
            };
            _playerInputActions.InGameActions.Move.canceled += _ => { _moveDirection = Vector2.zero; };
            _playerInputActions.InGameActions.Dash.started += Dash;
            _playerInputActions.InGameActions.Attack.started += Attack;
        }

        private void UnsubscribeAll()
        {
            _playerInputActions.InGameActions.Dash.started -= Dash;
            _playerInputActions.InGameActions.Attack.started -= Attack;
        }

        public void Update()
        {
            Move(_moveDirection);
        }


        private bool _canAttack = true;

        private float _attackSeries = 1;

        //TODO: Move to combat system
        private async void Attack(InputAction.CallbackContext _)
        {
            if (_canAttack)
            {
                _canAttack = false;
                _playerMovement.CanMove = false;
                animator.SetBool(IsAttack, true);
                animator.SetFloat("Series", _attackSeries);

                await Task.Delay((int)(1000 * playerControllerConfig.attackDelay));

                animator.SetBool(IsAttack, false);
                _playerMovement.CanMove = true;
                _canAttack = true;
                _attackSeries = _attackSeries > 0.05f ? 0f : 1f;
            }
        }

        
        Quaternion targetRotation;

        private void Move(Vector2 velocity)
        {
            if (!_playerMovement.CanMove) return;

            var newPosition =
                _playerMovement.Move(velocity, transform, playerControllerConfig.moveSpeed, Time.deltaTime);

            animator.SetFloat(Horizontal, _playerMovement.CurrentVelocity.x);
            animator.SetFloat(Vertical, _playerMovement.CurrentVelocity.y);

            if (velocity != Vector2.zero)
            {
                
                Vector3 localMovementDirection = new Vector3(velocity.x, 0f, velocity.y);
                
                Vector3 forward = Quaternion.Euler(0f, 0f, 0f) * Vector3.forward;
                forward.y = 0f;
                forward.Normalize();
                
                if (Mathf.Approximately(Vector3.Dot(localMovementDirection, Vector3.forward), -1.0f))
                {
                    targetRotation = Quaternion.LookRotation(-forward);
                }
                else
                {
                    targetRotation = Quaternion.LookRotation(localMovementDirection);
                }
                
                targetRotation = Quaternion.RotateTowards(playerTransform.rotation, targetRotation, playerControllerConfig.help * Time.deltaTime);

                playerTransform.rotation = targetRotation;
            }

            characterController.Move(newPosition);
        }

        private async void Dash(InputAction.CallbackContext _)
        {
            if (!_playerMovement.CanDash) return;
            if (TryUseStamina())
            {
                await GraduateAsync(Progress, playerControllerConfig.dashDuration);
                _playerMovement.DashStop(playerControllerConfig.dashTimeOut);

                void Progress(float progress)
                {
                    var newPosition = _playerMovement.DashRun(_moveDirection, playerTransform,
                        playerControllerConfig.dashPower, Time.deltaTime);

                    characterController.Move(newPosition);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Projectile>(out var projectile))
            {
                _playerState.currentHP -= projectile.damage;
                
                UIUpdater.Instance.UpdateHealth(Mathematic.Normalized(_playerState.currentHP, 0, _playerState.maxHP));
                Destroy(projectile.gameObject);
            }
            else if(other.TryGetComponent<Weapon>(out var weapon))
            {
                _playerState.currentHP -= weapon.Damage;
                
                UIUpdater.Instance.UpdateHealth(Mathematic.Normalized(_playerState.currentHP, 0, _playerState.maxHP));
            }

            if (_playerState.currentHP <= 0)
            {
                Death();
            }
        }

        private void Death()
        {
            animator.SetTrigger("IsDeath");
            this.enabled = false;
            UnsubscribeAll();
        }


        private bool _canGraduate = true;
        private Coroutine _restoreStamina;

        private bool TryUseStamina()
        {
            if (_playerState.currentStamina > 1f)
            {
                _canGraduate = false;
                _playerState.currentStamina -= 10f;
                UIUpdater.Instance.UpdateStamina(Mathematic.Normalized(_playerState.currentStamina, 0,
                    _playerState.maxStamina));

                if (_restoreStamina is not null) StopCoroutine(_restoreStamina);
                _restoreStamina = StartCoroutine(RestoreStamina());
                return true;
            }

            return false;
        }

        private IEnumerator RestoreStamina()
        {
            yield return new WaitForSeconds(2f);
            _canGraduate = true;
            Graduate(Progress, 2f);

            void Progress(float progress)
            {
                var normalized = Mathematic.Normalized(_playerState.currentStamina, 0, _playerState.maxStamina);
                UIUpdater.Instance.UpdateStamina(normalized);
                var value = normalized + 0.1f;
                value = Mathematic.Denormalize(value, 0, _playerState.maxStamina);

                _playerState.currentStamina = Mathf.Clamp(value, 0, _playerState.maxStamina);
            }
        }

        private void UpdateCameraRotation(Vector2 rotation)
        {
            var rotate = _thirdPersonCameraMovement.Rotate(rotation);

            followTarget.transform.rotation *= Quaternion.AngleAxis(
                rotate.x
                * rotationPower.x, Vector3.up);

            followTarget.rotation *= Quaternion.AngleAxis(
                (rotate.y * rotationPower.y)
                * -1, Vector3.right);


            followTarget.transform.localEulerAngles = _thirdPersonCameraMovement.CutRotate(followTarget.eulerAngles);
        }


        public async Task GraduateAsync(Action<float> action, float duration, bool reverse = false)
        {
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                float ratio = time / duration;
                ratio = reverse ? 1f - ratio : ratio;

                float progress = ratio;

                action.Invoke(progress);

                await Task.Yield();
            }

            action.Invoke(reverse ? 0f : 1f);
        }

        public async void Graduate(Action<float> action, float duration, bool reverse = false)
        {
            for (float time = 0f; time < duration && _canGraduate; time += Time.deltaTime)
            {
                float ratio = time / duration;
                ratio = reverse ? 1f - ratio : ratio;

                float progress = ratio;

                action.Invoke(progress);

                await Task.Yield();
            }

            action.Invoke(reverse ? 0f : 1f);
        }
    }
}