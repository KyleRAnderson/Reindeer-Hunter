using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reindeer_Hunter.Data_Classes
{
    public class PrintMessage
    {
        // Percentage done.
        public double Progress { get; set; }

        // Message to display
        public string Message { get; set; }

        public string Path { get; set; }
    }
}
