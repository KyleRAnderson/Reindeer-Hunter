using Reindeer_Hunter.Data_Classes;
using System;
using System.Collections;
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

namespace Reindeer_Hunter.DataCards
{
    /// <summary>
    /// Interaction logic for StudentCard.xaml
    /// </summary>
    public partial class StudentCard : UserControl
    {
        // The DataRow classes
        private DataRow D_StudentId = new DataRow { Property = "Id", Value = "" };
        private DataRow D_LastRound = new DataRow { Property = "Last Round", Value = "" };
        private DataRow D_Homeroom = new DataRow { Property = "Homeroom", Value = "" };
        private DataRow D_Grade = new DataRow { Property = "Grade", Value = "" };
        private DataRow D_First = new DataRow { Property = "First", Value = "" };
        private DataRow D_Last = new DataRow { Property = "Last", Value = "" };
        private DataRow Status = new DataRow { Property = "Status", Value = "" };
        private DataRow D_CurrentMatch = new DataRow { Property = "Match", Value = "" };

        // The visible properties.
        public int StudentId
        {
            get
            {
                return int.Parse(D_StudentId.Value);
            }
            set
            {
                D_StudentId.Value = value.ToString();
            }
        }
        public long LastRound
        {
            get
            {
                return long.Parse(D_LastRound.Value);
            }
            set
            {
                D_LastRound.Value = value.ToString();
            }
        }
        public int Homeroom
        {
            get
            {
                return int.Parse(D_Homeroom.Value);
            }
            set
            {
                D_Homeroom.Value = value.ToString();
            }
        }
        public int Grade
        {
            get
            {
                return int.Parse(D_Grade.Value);
            }
            set
            {
                D_Grade.Value = value.ToString();
            }
        }
        public string First
        {
            get
            {
                return D_First.Value;
            }
            set
            {
                D_First.Value = value;
            }
        }
        public string Last
        {
            get
            {
                return D_Last.Value;
            }
            set
            {
                D_Last.Value = value;
            }
        }
        public bool In
        {
            set
            {
                if (value) Status.Value = "In";
                else Status.Value = "Out";
            }
        }
        public string CurrentMatch
        {
            get
            {
                return D_CurrentMatch.Value;
            }
            set
            {
                D_CurrentMatch.Value = value;
            }
        }

        // The public one that everyone else views.
        public List<string> ParticipatedMatches
        {
            set
            {
                MatchesBox.ItemsSource = value;
            }
        }

        // Stuff for to help the displaying
        private List<DataRow> DisplayList;

        private DataCardWindow MasterWindow;

        public StudentCard(DataCardWindow window)
        {
            InitializeComponent();
            MasterWindow = window;

            // Make the DataGrid rows
            DisplayList = new List<DataRow>
            {   {D_First},
                {D_Last },
                {D_StudentId },
                {D_Grade },
                {D_Homeroom },
                {D_CurrentMatch },
                {D_LastRound }                
            };

            DataGrid.ItemsSource = DisplayList;
        }

        // To change to a match display box.
        private void MatchesBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // This happens when it's empty
            if (MatchesBox.SelectedItem == null) return;
            MasterWindow.Display(matchId: (string)MatchesBox.SelectedItem);
        }
    }
}
