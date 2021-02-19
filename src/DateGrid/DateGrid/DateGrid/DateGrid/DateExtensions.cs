using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DateGrid.DateGrid
{
    public static class DateExtensions
    {
        public static DateTime FirstDateOfWeek(this DateTime date, DayOfWeek firstDayOfWeek)
        {
            var diff = (7 + (date.DayOfWeek - firstDayOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        public static List<string> DayHeaders(this DateTime date, DayOfWeek firstDayOfWeek, string format = "ddd")
        {
            var firstDateOfWeek = date.FirstDateOfWeek(firstDayOfWeek);
            var lastDateOfWeek = firstDateOfWeek.AddDays(6);

            var days = new List<DateTime>();

            for (DateTime d = firstDateOfWeek; d <= lastDateOfWeek; d = d.AddDays(1))
            {
                days.Add(d);
            }

            return days.Select(d => d.ToString(format)).ToList();
        }

        public static string MonthHeader(DateTime startDate, DateTime endDate, string singleMonthFormat = "MMMM", string multiMonthformat = "MMM")
        {
            if (startDate.Month != endDate.Month)
            {
                return $"{startDate.ToString(multiMonthformat, CultureInfo.CurrentCulture)} / {endDate.ToString(multiMonthformat, CultureInfo.CurrentCulture)}";
            }

            return startDate.ToString(singleMonthFormat, CultureInfo.CurrentCulture);
        }
    }
}
