namespace ProjectX
{
    public static class Mathematic
    {
        public static float Normalized(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }
    
        public static double Normalized(double value, double min, double max)
        {
            return (value - min) / (max - min);
        }

        public static float Denormalize(float normalized, float min, float max)
        {
            return (normalized * (max - min) + min);
        }
    
        public static int Denormalize(int normalized, int min, float max)
        {
            return (int)(normalized * (max - min) + min);
        }
    }
}