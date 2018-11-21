using Reindeer_Hunter.Data_Classes;
using System.Windows.Controls;
using System.Windows.Media;

namespace Reindeer_Hunter.Subsystems.Passer
{
    class PasserButton : Button
    {
        public PassingStudent MatchResult { get; private set; }

        public PasserButton(PassingStudent result) : base()
        {
            Content = result.AffectedStudent.FullName;
            Width = double.NaN; // Not a number for auto width
            Height = 20;
            FontSize = 10;
            Background = Brushes.Black;
            Foreground = Brushes.White;
            MatchResult = result;
        }
    }
}
