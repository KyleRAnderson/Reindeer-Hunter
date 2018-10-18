using System;
using System.Windows;

namespace Reindeer_Hunter.Subsystems.ProcessButtonCommands
{
    /// <summary>
    /// Interaction logic for MatchmakeDialog.xaml
    /// </summary>
    public partial class MatchmakeDialog : Window
    {

        public enum MatchmakeStatus { Homerooms, Grades, Students, Cancelled };

        /// <summary>
        /// True after the submit button has been pressed, false otherwise.
        /// </summary>
        bool Submitted = false;

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
            Submitted = true;
            Close();
        }

        // Just close the window.
        private void CancelFunc(object parameter)
        {
            Submitted = false;
            Close();
        }

        public MatchmakeStatus GetResult()
        {
            if (!Submitted) return MatchmakeStatus.Cancelled;
            else if (Homerooms.IsChecked == true) return MatchmakeStatus.Homerooms;
            else if (Grades.IsChecked == true) return MatchmakeStatus.Grades;
            else if (Students.IsChecked == true) return MatchmakeStatus.Students;
            else return MatchmakeStatus.Cancelled;
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
