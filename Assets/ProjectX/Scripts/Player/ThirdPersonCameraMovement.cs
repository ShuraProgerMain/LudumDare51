using UnityEngine;

namespace ProjectX.Scripts.Player
{
    internal class ThirdPersonCameraMovement
    {
        private Vector2 _smoothRotationVelocity;
        private Vector3 _currentRotationVelocity;

        public Vector3 Rotate(Vector2 rotation)
        {
            _currentRotationVelocity =
                Vector2.SmoothDamp(_currentRotationVelocity, rotation, ref _smoothRotationVelocity, .2f);

            return _currentRotationVelocity;
        }

        public Vector3 CutRotate(Vector3 eulerAngles)
        {
            var angles = eulerAngles;
            angles.z = 0;

            var angle = eulerAngles.x;

            //Clamp the Up/Down rotation
            if (angle is > 180 and < 340)
            {
                angles.x = 340;
            }
            else if (angle is < 180 and > 40)
            {
                angles.x = 40;
            }

            return angles;
        }
    }
}