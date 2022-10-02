namespace ProjectX.Scripts.Player
{
    public class PlayerState
    {
        public int maxHP = 10;
        public float maxStamina = 100;
        public int currentHP = 10;
        public float currentStamina = 100;

        public PlayerState(int maxHP, float maxStamina)
        {
            this.maxHP = maxHP;
            currentHP = this.maxHP;
            this.maxStamina = maxStamina;
            currentStamina = this.maxStamina;
        }
    }
}