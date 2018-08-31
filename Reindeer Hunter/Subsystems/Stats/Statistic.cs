using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reindeer_Hunter.Subsystems.Stats
{
    public class Statistic
    {
        // The title of the statistic
        public string Title { get; private set; }

        private Func<string> GetFunc;

        public string Stat
        {
            get
            {
                return GetFunc();
            }
        }

        /// <summary>
        /// Class to be used to display a statistic in the GUI
        /// </summary>
        /// <param name="function">The function where this statistic can find its statistic data.</param>
        /// <param name="title">The Title (to be displayed in the GUI) of the statistic.</param>
        public Statistic(Func<string> function, string title)
        {
            GetFunc = function;
            Title = title;
        }
    }
}
