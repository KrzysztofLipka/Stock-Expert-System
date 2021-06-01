using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DataSerializer.Models
{
    public class NbpRate : IWritableAsRow
    {
        public string No { get; set; }
        public DateTime EffectiveDate { get; set; }
        
        public decimal Mid { get; set; }

        public string ToCommaSeparatedRow()
        {
            return EffectiveDate.ToString() + "," + Mid.ToString(CultureInfo.GetCultureInfo("en-GB"));
        }
    }
}
