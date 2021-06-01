using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockExpertSystemBackend.Utils
{
    public static class DateUtils
    {

        public static List<DateTime> EachCalendarDayInRange(DateTime startDate, int range) {
            List<DateTime> result = new List<DateTime>();
            DateTime currentDate = startDate;

            for (int i = 0; i < range; i++) {
                result.Add(currentDate);
                currentDate = currentDate.AddDays(1);
            }

            return result;
        }

        
        public static IEnumerable<DateTime> EachCalendarDay(DateTime startDate, DateTime endDate)
        {
            for (var date = startDate.Date; date.Date <= endDate.Date; date = date.AddDays(1)) yield
            return date;
        }

        public static int ConvertDateRangeToNumberOfDays(string range) => range switch
        {
            "1 day" => 1,
            "1 week" => 7,
            "2 weeks" => 14,
            "1 month" => 31,
            _ => throw new ArgumentOutOfRangeException($"Not expected direction value: {range}"),
        };
    }
}
