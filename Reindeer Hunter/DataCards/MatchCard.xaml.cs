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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Reindeer_Hunter.DataCards
{
    /// <summary>
    /// Interaction logic for MatchCarx.xaml
    /// </summary>
    public partial class MatchCard : UserControl
    {
        // The display DataRow objects

            // The first student's stuff
            private DataRow D_Id1 = new DataRow { Property = "Student 1 ID", Value = "" };
            private DataRow D_First1 = new DataRow { Property = "Student 1 First", Value = "" };
            private DataRow D_Last1 = new DataRow { Property = "Student 1 Last", Value = "" };
            private DataRow D_Home1 = new DataRow { Property = "Student 1 Homeroom #", Value = "" };
            private DataRow D_Status1 = new DataRow { Property = "Student 1 Status", Value = "" };

            // The second student's stuff
            private DataRow D_Id2 = new DataRow { Property = "Student 2 ID", Value = "" };
            private DataRow D_First2 = new DataRow { Property = "Student 2 First", Value = "" };
            private DataRow D_Last2 = new DataRow { Property = "Student 2 Last", Value = "" };
            private DataRow D_Home2 = new DataRow { Property = "Student 2 Homeroom #", Value = "" };
            private DataRow D_Status2 = new DataRow { Property = "Student 2 Status", Value = "" };

            // The match's data
            private DataRow D_Round = new DataRow { Property = "Round", Value = "" };
            private DataRow D_ID = new DataRow { Property = "Match ID", Value = "" };
            private DataRow D_Closed = new DataRow { Property = "Match Status", Value = "" };

        // The public properties where they can be set by external objects

            // Student 1
            public int Id1
            {
                get
                {
                    return int.Parse(D_Id1.Value);
                }
                set
                {
                    D_Id1.Value = value.ToString();
                }
            }
            public string First1
            {
                get
                {
                    return D_First1.Value;
                }
                set
                {
                    D_First1.Value = value;
                }
            }
            public string Last1
            {
                get
                {
                    return D_Last1.Value;
                }
                set
                {
                    D_Last1.Value = value;
                }
            }
            public int Home1
            {
                get
                {
                    return int.Parse(D_Home1.Value);
                }
                set
                {
                    D_Home1.Value = value.ToString();
                }
            }
            public bool Pass1
            {
                get
                {
                    return (D_Status1.Value == "Won");
                }
                set
                {
                    if (value) D_Status1.Value = "Won";
                    else D_Status1.Value = "";
                }
            }

            // The match's stuff
            public string MatchId
            {
                get
                {
                    return D_ID.Value;
                }
                set
                {
                    D_ID.Value = value;
                }
            }
            public bool Closed
            {
                get
                {
                return (D_Closed.Value == "Closed");
                }
                set
                {
                    if (value) D_Closed.Value = "Closed";
                    else D_Closed.Value = "Open";
                }
            }
            public long Round
            {
                get
                {
                return long.Parse(D_Round.Value);
                }
                set
                {
                    D_Round.Value = value.ToString();
                }
            }

            // Student 2
            public int Id2
            {
                get
                {
                    return int.Parse(D_Id2.Value);
                }
                set
                {
                    D_Id2.Value = value.ToString();
                }
            }
            public string First2
            {
                get
                {
                    return D_First2.Value;
                }
                set
                {
                    D_First2.Value = value;
                }
            }
            public string Last2
            {
                get
                {
                    return D_Last2.Value;
                }
                set
                {
                    D_Last2.Value = value;
                }
            }
            public int Home2
            {
                get
                {
                    return int.Parse(D_Home2.Value);
                }
                set
                {
                    D_Home2.Value = value.ToString();
                }
            }
            public bool Pass2
            {
                get
                {
                    return (D_Status2.Value == "Won");
                }
                set
                {
                    if (value) D_Status2.Value = "Won";
                    else D_Status2.Value = "";
                }
            }

        private long CurrRound;
        // This is used to control whether or not the reopen button is enabled.
        public long CurrentRound
        {
            get
            {
                return CurrRound;
            }
            set
            {
                CurrRound = value;
                // Make sure that the match is of the current round, is closed and is not a pass
                // match before reopening.
                Reopen_Button.IsEnabled = (Round == CurrRound && Closed && Id2 != 0);
            }
        }



        // The list of these properties to be displayed
        private List<DataRow> DisplayList;

        private DataCardWindow MasterWindow;

        public MatchCard(DataCardWindow window)
        {
            InitializeComponent();
            MasterWindow = window;

            DisplayList = new List<DataRow>
            {
                {D_Id1 },
                {D_First1 },
                {D_Last1 },
                {D_Home1 },
                {D_Status1 },
                // Empty Row
                {new DataRow{Property = "", Value = ""} },
                {D_Closed },
                {D_Round },
                {D_ID },
                // Empty Row
                {new DataRow{Property = "", Value = ""} },
                {D_Id2 },
                {D_First2 },
                { D_Last2 },
                {D_Home2 },
                {D_Status2 }
            };

            DataGrid.ItemsSource = DisplayList;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Figure out what row is selected.
            int displayIndex = DataGrid.Items.IndexOf(DataGrid.CurrentCell.Item);

            // < 5 indicating that it is one of the first student's rows. 
            if (displayIndex < 5 && DataGrid.CurrentCell.Column.DisplayIndex == 1)
            {
                // Display the first student.
                MasterWindow.Display(studentId: Id1);
            }

            // > 9 means it's in one of the second student's rows.
            else if (displayIndex > 9 && DataGrid.CurrentCell.Column.DisplayIndex == 1)
            {
                // Display the second student
                MasterWindow.Display(studentId: Id2);
            }
        }

        private void Reopen_Button_Click(object sender, RoutedEventArgs e)
        {
            MasterWindow.ReopenMatch(MatchId);

            // Quick ol' refresh
            DataGrid.Items.Refresh();
        }
    }
}
