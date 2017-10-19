using System.Windows.Controls;

namespace Reindeer_Hunter.FFA
{
    /// <summary>
    /// Interaction logic for FreeForAll.xaml
    /// </summary>
    public partial class FreeForAll : UserControl
    {
        public FreeForAll(School school)
        {
            InitializeComponent();

            // Give the victorhandler object some necessary data
            ((VictorHandler)DataContext).DataHandler = school.GetDataFile();
            ((VictorHandler)DataContext)._School = school;
            ((VictorHandler)DataContext).SetParentPage(this);

            // Give this Usercontrol focus
            Focusable = true;
            Focus();
        }
    }
}
