using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reindeer_Hunter.Data_Classes
{
    public class Filter
    {
        public List<long> Rounds { get; set; }
        public bool Open { get; set; }
        public bool Closed { get; set; }
    }
}
