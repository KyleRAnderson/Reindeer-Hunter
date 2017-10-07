using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }
    }
}
