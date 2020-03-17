using System.Collections.Generic;

namespace Navred.Core.Models
{
    public class Schedule
    {
        public string From { get; set; }

        public string To { get; set; }

        public IEnumerable<string> Stops { get; set; }

        public decimal Price { get; set; }
    }
}
