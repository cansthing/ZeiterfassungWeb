namespace ZeiterfassungWeb.Data
{
    public class TimeHelper
    {
        public static DateTime? RoundToNextQuarterHour(DateTime? dateTime)
        {
            if (dateTime == null) return null;

            var time = dateTime.Value;
            var minutes = time.Hour * 60 + time.Minute;
            var rounded = ((minutes + 14) / 15) * 15;

            return new DateTime(time.Year, time.Month, time.Day, rounded / 60, rounded % 60, 0);
        }

        public static DateTime? RoundToPreviousQuarterHour(DateTime? dateTime)
        {
            if (dateTime == null) return null;

            var time = dateTime.Value;
            var minutes = time.Hour * 60 + time.Minute;
            var rounded = (minutes / 15) * 15;

            return new DateTime(time.Year, time.Month, time.Day, rounded / 60, rounded % 60, 0);
        }

        public static string ToString(DateTime? dateTime, string format = "dd.MM.yyyy HH:mm")
        {
            if (dateTime == null)
            {
                return string.Empty;
            }
            DateTime time = (DateTime)dateTime;
            return time.ToString(format);
        }
        public static string ToString(TimeSpan? dateTime, string format = @"hh\:mm\:ss")
        {
            if (dateTime == null)
            {
                return string.Empty;
            }
            TimeSpan time = (TimeSpan)dateTime;
            return time.ToString(format);
        }
    }

}
