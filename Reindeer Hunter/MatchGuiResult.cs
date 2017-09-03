using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Reindeer_Hunter
{
    /// <summary>
    /// A class to contain the results inputted from the datagrid MainDisplay
    /// on the GUI
    /// </summary>
    public class MatchGuiResult
    {
        public string MatchID { get; set; }
        public string Name { get; set; }
        public int StuID { get; set; }
        public Button ResultButton {get; set;}
        public HomePage Home { get; set; }

        public MatchGuiResult(string name)
        {
            Name = name;
            ResultButton = new Button
            {
                Content = Name,
                Width = Double.NaN, // Not a number for auto width
                Height = 20,
                FontSize = 10,
                Background = Brushes.Black,
                Foreground = Brushes.White
            };
            ResultButton.Click += Remove;
        }

        private void Remove(Object source, EventArgs e)
        {
            Home.RemoveResult(StuID);
        }
    }
}
