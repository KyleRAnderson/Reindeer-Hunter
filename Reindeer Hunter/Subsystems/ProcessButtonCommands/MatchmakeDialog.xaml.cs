using System;
using System.Windows;

namespace Reindeer_Hunter.Subsystems.ProcessButtonCommands
{
    /// <summary>
    /// Interaction logic for MatchmakeDialog.xaml
    /// </summary>
    public partial class MatchmakeDialog : Window
    {
        // The possible statuses.
        public static readonly int HOMEROOMS = 0;
        public static readonly int GRADES = 1;
        public static readonly int STUDENTS = 2;
        public static readonly int CANCELLED = 3;

        bool Cancelled = false;

        public RelayCommand OkCommand { get; } = new RelayCommand();
        public RelayCommand CancelCommand { get; } = new RelayCommand
        {
            CanExecuteDeterminer = () => true
        };

        public MatchmakeDialog()
        {
            InitializeComponent();

            OkCommand.CanExecuteDeterminer = CanOkCancel;

            OkCommand.FunctionToExecute = OkFunc;
            CancelCommand.FunctionToExecute = CancelFunc;
        }

        private bool CanOkCancel()
        {
            // One of the radiobuttons has to be checked to move on.
            return (Homerooms.IsChecked == true || Grades.IsChecked == true || Students.IsChecked == true);
        }

        // Just close the window. Status should take care of itself
        private void OkFunc(object parameter)
        {
            Close();
        }

        // Just close the window.
        private void CancelFunc(object parameter)
        {
            Cancelled = true;
            Close();
        }

        public int GetResult()
        {
            if (Cancelled) return CANCELLED;
            else if (Homerooms.IsChecked == true) return HOMEROOMS;
            else if (Grades.IsChecked == true) return GRADES;
            else if (Students.IsChecked == true) return STUDENTS;
            else return CANCELLED;
        }

        /// <summary>
        /// Just used to raise the can execute changed event on 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RaiseCanExecuteChanged(object sender, RoutedEventArgs e)
        {
            OkCommand.RaiseCanExecuteChanged();
        }

        public string EndDate
        {
            get
            {
                return DateBox.Text;
            }
        }
    }
}
