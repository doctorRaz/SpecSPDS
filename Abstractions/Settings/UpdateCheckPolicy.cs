namespace dRz.Abstractions.Settings
{
    public static class UpdateCheckPolicy
    {
        public static int NormalizeIntervalDays(int intervalDays)
        {
            if (intervalDays < 1) return 1;
            if (intervalDays > 30) return 30;
            return intervalDays;
        }
    }
}
