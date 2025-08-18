using Microsoft.VisualBasic;
using System.Globalization;

namespace ZeiterfassungWeb.Data.Models
{
    public class DailyWork
    {
        public DateOnly Date { get; set; }
        public DayOfWeek Weekday => Date.DayOfWeek;
        public string WeekdayName => Date.ToString("dddd", new CultureInfo("de-DE"));
        public TimeSpan TotalWorkTime { get; set; }
        public TimeSpan TotalBreakTime { get; set; }
        public TimeSpan TotalWorkTimeWithBreaks { get => TotalWorkTime + TotalBreakTime; }

        public DailyWork(DateTime dateTime, TimeSpan workTime, TimeSpan breakTime)
        {
            Date = DateOnly.FromDateTime(dateTime);
            TotalWorkTime = workTime;
            TotalBreakTime = breakTime;
        }
        public DailyWork()
        {

        }

        public enum DateInterval
        {
            Month,
            CalendarWeek
        }
        public static List<DailyWork> FillMissingDays(List<DailyWork> input, int year, int value, DateInterval interval)
        {
            DateOnly startDate, endDate;

            switch (interval)
            {
                case DateInterval.Month:
                    startDate = new DateOnly(year, value, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1); // Letzter Tag des Monats
                    break;

                case DateInterval.CalendarWeek:
                    startDate = FirstDateOfWeekISO8601(year, value);
                    endDate = startDate.AddDays(6); // Montag bis Sonntag
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(interval), "Unsupported interval type.");
            }

            return FillMissingDaysInternal(input, startDate, endDate);
        }

        private static List<DailyWork> FillMissingDaysInternal(List<DailyWork> input, DateOnly start, DateOnly end)
        {
            var allDates = Enumerable.Range(0, end.DayNumber - start.DayNumber)
                .Select(offset => start.AddDays(offset));

            var filledList = new List<DailyWork>(input);

            foreach (var date in allDates)
            {
                if (!filledList.Any(dw => dw.Date == date))
                {
                    filledList.Add(new DailyWork(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero, TimeSpan.Zero));
                }
            }

            return filledList.OrderBy(dw => dw.Date).ToList();
        }
        public static DateOnly FirstDateOfWeekISO8601(int year, int week)
        {
            var jan1 = new DateTime(year, 1, 1);
            var calendar = CultureInfo.InvariantCulture.Calendar;

            // Suche ersten Montag in Jahr oder davor
            var firstThursday = jan1.AddDays(3 - (int)jan1.DayOfWeek);
            int firstWeek = calendar.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            // Berechne Offset in Tagen zur gewünschten Woche
            int daysOffset = (week - 1) * 7;

            // Starte von diesem Donnerstag, springe rückwärts zum Montag
            var firstDayOfWeek = firstThursday.AddDays(-3 + daysOffset);

            return DateOnly.FromDateTime(firstDayOfWeek);
        }
    }
}
