namespace Ofgem_Web_LAF_InternalPortal.Extensions
{
    public static class LafLocalTimezone
    {

        public static DateTime ToLocalTime(DateTime utcDateTime)
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tzi);

            return localTime;
        }


    }
}
