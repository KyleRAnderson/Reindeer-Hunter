using Reindeer_Hunter.Data_Classes;
using Reindeer_Hunter.Subsystems;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Reindeer_Hunter.DataCards
{
    /// <summary>
    /// Interaction logic for StudentCard.xaml
    /// </summary>
    public partial class StudentCard : UserControl
    {
        public event EventHandler StudentDeleted;

        // Command used to submit changes made to the student.
        public RelayCommand UpdateCommand { get; } = new RelayCommand();

        // The list of the one student to display
        private List<Student> DisplayList = new List<Student>();

        public void Refresh()
        {
            DisplayList.Clear();

            // If the student is not null, add them to display
            if (_DisplayStudent != null)
            {
                DisplayList.Add(_DisplayStudent);
                ParticipatedMatches = _DisplayStudent.MatchesParticipated;
            }

            PropertyDisplay.Items.Refresh();
        }

        // The public one that everyone else views.
        public List<string> ParticipatedMatches
        {
            set
            {
                MatchesBox.ItemsSource = value;
            }
        }

        private DataCardWindow MasterWindow;

        public Student _DisplayStudent { get; set; }

        private bool DataGridEdited { get; set; }  = false;

        public StudentCard(DataCardWindow window, long RoundNo)
        {
            InitializeComponent();
            MasterWindow = window;

            // Make a new, empty list for the itemssource.
            PropertyDisplay.ItemsSource = DisplayList;

            // Set the command for the update button
            Update_Button.Command = UpdateCommand;            

            // The delete button should only be enabled when the round number is 0.
            Delete_Button.IsEnabled = RoundNo == 0;


            // Give the update command the parameters it needs to run properly.
            UpdateCommand.CanExecuteDeterminer = CanUpdate;
            UpdateCommand.FunctionToExecute = UpdateStudent;
        }

        // To change to a match display box.
        private void MatchesBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // This happens when it's empty
            if (MatchesBox.SelectedItem == null) return;
            MasterWindow.Display(matchId: (string)MatchesBox.SelectedItem);
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult confirmation = MessageBox.Show("Are you sure you\'d like to delete this studentt? " +
                "This action CANNOT BE UNDONE.", "Proceed?",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            // If the user doesn't confirm the action, return.
            if (confirmation != MessageBoxResult.Yes) return;

            MasterWindow.DeleteStudent(_DisplayStudent.Id);

            // Raise the event 
            StudentDeleted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Determines whether or not the update command can be edited.
        /// </summary>
        /// <returns></returns>
        private bool CanUpdate()
        {
            return DataGridEdited;
        }

        /// <summary>
        /// Function that the update command calls to perform the student update.
        /// Student id should never be changed.
        /// </summary>
        /// <param name="parameter"></param>
        private void UpdateStudent(object parameter)
        {
            MasterWindow._School.UpdateStudent(_DisplayStudent);
            DataGridEdited = false;

            UpdateCommand.RaiseCanExecuteChanged();
        }

        private void PropertyDisplay_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            DataGridEdited = true;

            // Update the executability of the update command.
            UpdateCommand.RaiseCanExecuteChanged();
        }
    }
}
