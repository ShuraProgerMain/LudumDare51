using System;
using System.Collections;
using System.Threading.Tasks;
using ProjectX.Scripts.AI.Lich;
using ProjectX.Scripts.Player.Configs;
using ProjectX.Scripts.UI;
using UnityEngine;

namespace ProjectX.Scripts.Player
{
    public class PlayerState
    {
        public int maxHP;
        public float maxStamina = 100;
        public int currentHP;
        public float currentStamina = 100;
    }

    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Vector2 _rotationPower = new(10, 2);
        [SerializeField] private PlayerControllerConfig _playerControllerConfig;

        private PlayerInputActions _playerInputActions;
        private PlayerState _playerState = new PlayerState();
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

            _playerMovement = new PlayerMovement();
            _thirdPersonCameraMovement = new ThirdPersonCameraMovement();


            _playerInputActions.InGameActions.Move.performed += context =>
            {
                _moveDirection = context.ReadValue<Vector2>();
            };
            _playerInputActions.InGameActions.Move.canceled += _ => { _moveDirection = Vector2.zero; };
            _playerInputActions.InGameActions.Dash.started += _ => Dash();
            _playerInputActions.InGameActions.Attack.started += _ => Attack();
        }

        public void Update()
        {
            Move(_moveDirection);
        }

        private bool _canAttack = true;

        private float _attackSeries = 1;

        //TODO: Move to combat system
        private async void Attack()
        {
            if (_canAttack)
            {
                _canAttack = false;
                _playerMovement.CanMove = false;
                animator.SetBool(IsAttack, true);
                animator.SetFloat("Series", _attackSeries);

                await Task.Delay((int)(1000 * _playerControllerConfig.attackDelay));

                animator.SetBool(IsAttack, false);
                _playerMovement.CanMove = true;
                _canAttack = true;
                _attackSeries = _attackSeries > 0.05f ? 0f : 1f;
            }
        }

        private void Move(Vector2 velocity)
        {
            if (!_playerMovement.CanMove) return;

            var newPosition =
                _playerMovement.Move(velocity, transform, _playerControllerConfig.moveSpeed, Time.deltaTime);

            animator.SetFloat(Horizontal, velocity.x);
            animator.SetFloat(Vertical, velocity.y);

            if (velocity != Vector2.zero)
            {
                CharacterRotate(velocity);

                _currentRotation = Vector2.SmoothDamp(playerTransform.rotation.eulerAngles,
                    new Vector3(0, followTarget.eulerAngles.y, 0), ref _currentRotationVel,
                    _playerControllerConfig.smoothRotateTime);
                playerTransform.rotation = Quaternion.Euler(0, _currentRotation.y, 0);
            }

            characterController.Move(newPosition);
        }

        private async void Dash()
        {
            if (!_playerMovement.CanDash) return;
            if (TryUseStamina())
            {
                Debug.Log("Start dash");
                await GraduateAsync(Progress, _playerControllerConfig.dashDuration);
                _playerMovement.DashStop(_playerControllerConfig.dashTimeOut);

                void Progress(float progress)
                {
                    var newPosition = _playerMovement.DashRun(_moveDirection, followTarget,
                        _playerControllerConfig.dashPower, Time.deltaTime);

                    characterController.Move(newPosition);
                }
            }
        }

        private void CharacterRotate(Vector3 direction)
        {
            var move = new Vector3(direction.x, 0, direction.y);


            followTarget.forward = move;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Projectile>(out var projectile))
            {
                Destroy(other.gameObject);
                Debug.Log($"Dmg -{projectile.damage}");
            }
        }


        private bool _canGraduate = true;
        private Coroutine _restoreStamina;

        private bool TryUseStamina()
        {
            if (_playerState.currentStamina > 1f)
            {
                _canGraduate = false;
                Debug.Log($"Current Stamina {_playerState.currentStamina}");
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
                Debug.Log($"Normalized {normalized} and value {value}");

                _playerState.currentStamina = Mathf.Clamp(value, 0, _playerState.maxStamina);
            }
        }

        private void UpdateCameraRotation(Vector2 rotation)
        {
            var rotate = _thirdPersonCameraMovement.Rotate(rotation);

            followTarget.transform.rotation *= Quaternion.AngleAxis(
                rotate.x
                * _rotationPower.x, Vector3.up);

            followTarget.rotation *= Quaternion.AngleAxis(
                (rotate.y * _rotationPower.y)
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