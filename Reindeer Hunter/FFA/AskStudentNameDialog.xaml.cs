using Reindeer_Hunter.Data_Classes;
using Reindeer_Hunter.Subsystems;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Reindeer_Hunter.FFA
{
    /// <summary>
    /// Interaction logic for AskStudentNameDialog.xaml
    /// </summary>
    public partial class AskStudentNameDialog : Window
    {

        public List<Victor> DisplayVictors;
        public int KilledStudentId { get; private set; }
        private List<MenuItem> Selections;

        public AskStudentNameDialog(List<Victor> victors, Victor killedStudent, 
            RelayCommand cancelCommand, RelayCommand submitCommand)
        {
            InitializeComponent();

            // Set the button's commands.
            CancelButton.Command = cancelCommand;
            SubmitButton.Command = submitCommand;

            // Update the title with the pinned student's name.
            Title = String.Format("Who pinned {0}?", killedStudent.FullName);
            DisplayVictors = victors;

            KilledStudentId = killedStudent.Id;

            // Construct the list of menuitems
            Selections = new List<MenuItem>();
            foreach (Victor victor in victors)
            {
                Selections.Add(new MenuItem
                {
                    Header = victor.FullName
                });
            }

            // Display it.
            SelectionBox.ItemsSource = Selections;
        }

        public AskStudentNameDialog(List<Victor> victors, string questionToAsk,
            RelayCommand cancelCommand, RelayCommand submitCommand)
        {
            InitializeComponent();

            // Set the button's commands.
            CancelButton.Command = cancelCommand;
            SubmitButton.Command = submitCommand;

            // Disable the cancel command
            CancelButton.IsEnabled = false;

            // Update the title with the pinned student's name.
            Title = questionToAsk;
            DisplayVictors = victors;

            KilledStudentId = 0;

            // Construct the list of menuitems
            Selections = new List<MenuItem>();
            foreach (Victor victor in victors)
            {
                Selections.Add(new MenuItem
                {
                    Header = victor.FullName
                });
            }

            // Display It
            SelectionBox.ItemsSource = Selections;
        }

        /// <summary>
        /// Function used when trying to find out the Id of a victor given its index.
        /// </summary>
        /// <param name="index">The index of the student in the list.</param>
        /// <returns>An integer that is the student's Id.</returns>
        public int GetVictorIdByIndex(int index)
        {
            return DisplayVictors[index].Id;
        }
    }
}
