namespace Templar
{
    public static class SharedFunctions
    {
        public static float MakePositive(float input, float input2 = 0, MakePositiveOptions options = MakePositiveOptions.InvertFloat)
        {
            switch (options)
            {
                case MakePositiveOptions.InvertFloatCompared:
                    return input < 0 && input2 > 0 || input > 0 && input2 < 0 ? -input : input;
                case MakePositiveOptions.FloatCompared:
                    return input < 0 && input2 < 0 || input > 0 && input2 > 0 ? -1 : 1;
            }
            return 0;
        }
    }
}
