using System;
using System.Threading.Tasks;
using ProjectX.Scripts.Player.Configs;
using UnityEngine;

namespace ProjectX.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Vector2 _rotationPower = new Vector2(10, 2);
        [SerializeField] private PlayerControllerConfig _playerControllerConfig;

        private PlayerInputActions _playerInputActions;
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
            _playerInputActions.InGameActions.Move.canceled += _ =>
            {
                _moveDirection = Vector2.zero;
            };
            _playerInputActions.InGameActions.Dash.started += _ => Dash();
        }

        public void Update()
        {
            Move(_moveDirection);
        }

        private Vector3 _currentRotation;
        private Vector2 _currentRotationVel;

        private void Move(Vector2 velocity)
        {
            var newPosition = _playerMovement.Move(velocity, transform, _playerControllerConfig.moveSpeed, Time.deltaTime);

            if (velocity != Vector2.zero)
            {
                CharacterRotate(velocity);

                _currentRotation = Vector2.SmoothDamp(playerTransform.rotation.eulerAngles,
                    new Vector3(0, followTarget.eulerAngles.y, 0), ref _currentRotationVel, _playerControllerConfig.smoothRotateTime);
                playerTransform.rotation = Quaternion.Euler(0, _currentRotation.y, 0);
            }

            characterController.Move(newPosition);
        }

        private async void Dash()
        {
            await GraduateAsync(Progress, _playerControllerConfig.dashDuration);
            _playerMovement.DashStop(_playerControllerConfig.dashTimeOut);

            void Progress(float progress)
            {
                var newPosition = _playerMovement.DashRun(_moveDirection, followTarget, _playerControllerConfig.dashPower, Time.deltaTime);
                
                characterController.Move(newPosition);
            }
        }
        
        private void CharacterRotate(Vector3 direction)
        {
            var move = new Vector3(direction.x, 0, direction.y);


            followTarget.forward = move;
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
    }
    
    
}