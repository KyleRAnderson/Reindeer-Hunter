using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Reindeer_Hunter.Data_Classes
{
    public class Filter
    {
        public List<CheckBox> RoundCheckboxes { get; set; } = new List<CheckBox>();

        public List<long> Round {
            get
            {
                List<long> returnList = new List<long>();
                foreach (CheckBox checkbox in RoundCheckboxes)
                {
                    if ((bool)checkbox.IsChecked)
                        returnList.Add(long.Parse(checkbox.Content.ToString()));
                }
                return returnList;
            }
            set
            {
                RoundCheckboxes.Clear();
                foreach (long round in value)
                {
                    RoundCheckboxes.Add(new CheckBox
                    {
                        Content = round.ToString(),
                    }
                    );
                }

                /* The last checkbox in the list is the checkbox of the highest round, and
                 * should be checked by default. Don't do it if there are no rouds in the list. */
                if (RoundCheckboxes.Count() > 0 )
                    RoundCheckboxes.ElementAt(RoundCheckboxes.Count() - 1).IsChecked = true;
            }
            }
        public bool Open { get; set; }
        public bool Closed { get; set; }
    }
}
