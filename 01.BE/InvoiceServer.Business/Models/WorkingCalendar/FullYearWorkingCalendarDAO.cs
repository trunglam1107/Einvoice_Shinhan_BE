using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    /// <summary>
    /// Interactive with Database
    /// </summary>
    public class FullYearWorkingCalendarDAO
    {
        public FullYearWorkingCalendarDAO()
        {
            Years = new List<int>();
            WorkingCalendars = new List<WorkingCalendarDAO>();
        }

        [JsonProperty("years")]
        public IList<int> Years { get; set; }

        [JsonProperty("workingCalendars")]
        public IList<WorkingCalendarDAO> WorkingCalendars { get; set; }
    }
}
