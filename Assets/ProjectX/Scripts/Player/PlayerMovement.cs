using UnityEditor.VersionControl;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace ProjectX.Scripts.Player
{
    internal class PlayerMovement
    {
        private Vector2 _smoothInputVelocity;
        private Vector2 _currentVelocity;
        private Vector2 _currentDashVelocity;

        private bool _canMove = true;
        private bool _canDash = true;
        private bool _dashProcess = false;

        public bool CanMove
        {
            get => _canMove;
            set
            {
                _canMove = value;
            }
        }

        public bool CanDash => _canDash;


        public Vector3 Move(Vector2 velocity, Transform directTransform, float speed, float deltaTime)
        {
            if (CanMove)
            {
                _currentVelocity = Vector2.SmoothDamp(_currentVelocity, velocity, ref _smoothInputVelocity, .2f);

                var moveSpeed = speed * deltaTime;
                var newPosition = (directTransform.forward * _currentVelocity.y * moveSpeed) +
                                  (directTransform.right * _currentVelocity.x * moveSpeed);
                newPosition.y = 0;

                return newPosition;
            }
            
            return Vector3.zero;
        }

        public Vector3 DashRun(Vector2 velocity, Transform directTransform, float power, float deltaTime)
        {
            if (CanDash || _dashProcess)
            {
                _canDash = false;
                CanMove = false;
                _dashProcess = true;
                
                //Нужно толкать прямо и все
                var dashPower = power * deltaTime;
                var newPosition = (directTransform.forward  * dashPower);
                newPosition.y = 0;

                return newPosition;
            }
            
            return Vector3.zero;
        }

        public async void DashStop(float timeOut = 1.5f)
        {
            CanMove = true;
            _dashProcess = false;

            await Task.Delay((int)(1000 * timeOut));
            _canDash = true;
        }
    }
}