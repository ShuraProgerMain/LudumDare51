using UnityEngine;
using UnityEngine.UI;

namespace ProjectX.Scripts.UI
{
    public class UIUpdater : MonoBehaviour
    {
        public static UIUpdater Instance;

        [SerializeField] private Image stamina;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public void UpdateStamina(float value)
        {
            stamina.fillAmount = value;
        }
    }
}