namespace JboxTransfer.Server.Helpers
{
    public static class TimeHelper
    {
        public static long GetTimeStampInt64()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        public static string GetTimeStampMilli()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }
        public static string GetTimeStampMilliMinus8()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.Subtract(TimeSpan.FromHours(8)).TotalMilliseconds).ToString();
        }
        public static long GetTimeStampMilliInt64()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        public static string ToFriendlyTime(this long time)
        {
            DateTime t = new DateTime(1970, 1, 1, 0, 0, 0, 0) + TimeSpan.FromMilliseconds(time);
            return t.ToString("F");
        }

        public static string ToFriendlyTime(this int time)
        {
            DateTime t = new DateTime(1970, 1, 1, 0, 0, 0, 0) + TimeSpan.FromSeconds(time);
            return t.ToString("F");
        }
    }
}
