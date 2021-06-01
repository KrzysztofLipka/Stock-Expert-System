using System;
using System.Collections.Generic;
using System.Text;

namespace DataSerializer.Models
{
    class NbpResponse
    {
        public string Table { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public List<NbpRate> Rates { get; set; }

    }
}
