using System;
namespace DSS.Rules.Library
{
    public static class TimeService
    {
        public static bool isMorning(Event e) 
        {

            DateTime morningStart = DateTime.UtcNow;
            DateTime morningEnd = DateTime.UtcNow;

            morningStart = morningStart.Date.Add(new TimeSpan(6, 0, 0));
            morningEnd = morningEnd.Date.Add(new TimeSpan(11, 0, 0));

            DateTime eventTime = UnixTimestampToDateTime(e.annotations.timestamp);

            return morningStart <= eventTime && morningEnd >= eventTime;


            //TODO: fix time zones
            //morningStart = TimeZoneInfo.ConvertTime(morningStart, TimeZoneInfo.Utc, tzInfo);
           // morningEnd = TimeZoneInfo.ConvertTime(morningEnd, TimeZoneInfo.Utc, tzInfo);


        }

        public static DateTime UnixTimestampToDateTime(double unixTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (dateTime.ToUniversalTime() - unixStart).Ticks;
            return (double)unixTimeStampInTicks / TimeSpan.TicksPerSecond;
        }

    }
}
