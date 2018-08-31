using Reindeer_Hunter.Data_Classes;
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
using System.Windows.Shapes;

namespace Reindeer_Hunter.FFA
{
    /// <summary>
    /// Interaction logic for DataCard.xaml
    /// A non-interactive window that merely displays victor information.
    /// </summary>
    public partial class DataCard : Window
    {
        public Dictionary<string, string> DataDisplaySource { get; set; }

        /// <summary>
        /// The constructor for this class.
        /// </summary>
        /// <param name="victorToDisplay">The victor object who's information this 
        /// will display.</param>
        /// <param name="kills">A list of strings containing the names of the victors that
        /// this victor has gotten out.</param>
        public DataCard(Victor victorToDisplay, List<string> kills)
        {
            InitializeComponent();

            // Set the title
            Title = victorToDisplay.FullName;

            // Compile the kills into one single string
            string victorsKilled = String.Join(", ", kills);
            
            // Create the DataDisplaySource and add the info to it.
            DataDisplaySource = new Dictionary<string, string>
            {
                {"Name", victorToDisplay.FullName },
                {"Student ID", victorToDisplay.Id.ToString() },
                {"Homeroom", victorToDisplay.Homeroom.ToString() },
                {"Grade", victorToDisplay.Grade.ToString() },
                {"Status", victorToDisplay.Status },
                {"Number of Pins", victorToDisplay.NumKills.ToString() },
                {"Pins",  victorsKilled}
            };

            DataDisplay.Items.Refresh();
        }
    }
}
