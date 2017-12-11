using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Reindeer_Hunter.Data_Classes
{
    /// <summary>
    /// A class to contain the results inputted from the datagrid MainDisplay
    /// on the GUI
    /// </summary>
    public class MatchGuiResult : EventArgs
    {
        // Fired when the button corresponding to this result is clicked.
        public event EventHandler ResultButtonClick;

        public string MatchID { get; set; }
        public string Name { get; set; }
        public int StuID { get; set; }
        public Button ResultButton {get; set;}

        public MatchGuiResult(string name)
        {
            Name = name;
            ResultButton = new Button
            {
                Content = Name,
                Width = double.NaN, // Not a number for auto width
                Height = 20,
                FontSize = 10,
                Background = Brushes.Black,
                Foreground = Brushes.White
            };

            ResultButton.Click += RaiseResultButtonClick;
        }

        public void RaiseResultButtonClick (object sender, EventArgs e)
        {
            ResultButtonClick(this, new EventArgs());
        }
    }
}
