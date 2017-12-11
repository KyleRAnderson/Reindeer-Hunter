using Reindeer_Hunter.Data_Classes;
using Reindeer_Hunter.ThreadMonitors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Reindeer_Hunter.Subsystems.ToolsCommands.Editor
{
    public class Editor : INotifyPropertyChanged
    {
        public event EventHandler StudentMoved;
        public event PropertyChangedEventHandler PropertyChanged;

        private long round;
        private long topMatchNo;
        private long originalTopMatchNo;

        private MatchEditor EditWindow { get; set; }

        private DataGrid Display1;
        private DataGrid Display2;
        private DataGrid Display3;

        /// <summary>
        /// School object used for various operations.
        /// </summary>
        School _School;

        private List<Match> inputtedMatches;

        /// <summary>
        /// The first datagrid's contents, full of the students
        /// that will be eliminated if left in the datagrid.
        /// </summary>
        public List<EditStudent> Table1Students { get; private set; }
        /// <summary>
        /// The students in the second datagrid, which will make matches randomly.
        /// </summary>
        public List<EditStudent> StudentsToRandomize { get; private set; } = new List<EditStudent>();
        /// <summary>
        /// The students in the third datagrid, full of finalized matches.
        /// </summary>
        public List<Match> MatchesMade { get; private set; } = new List<Match>();
        private Dictionary<int, EditStudent> studentsInMatches = new Dictionary<int, EditStudent>();

        #region RelayCommands
        public RelayCommand RandomizeCommand { get; } = new RelayCommand();
        public RelayCommand AddToRandomizeCommand { get; } = new RelayCommand();
        public RelayCommand SaveCloseCommand { get; } = new RelayCommand();
        public RelayCommand SavePrintCommand { get; } = new RelayCommand();
        public RelayCommand DiscardCloseCommand { get; } = new RelayCommand();
        public RelayCommand DeleteMatchCommand { get; } = new RelayCommand();
        #endregion

        public Editor()
        {
            StudentMoved += Refresh;
        }

        #region Student Table Stuff
        /// <summary>
        /// Function to move the given edit student back to the student table.
        /// </summary>
        /// <param name="student">The student to move to the student table.</param>
        /// <param name="refresh">Whether or not to refresh, to avoid multiple
        /// refreshes if doing this in bulk. Defaults to true, to refresh.</param>
        private void MoveToStudentTable(EditStudent student, bool refresh = true)
        {
            Table1Students.Add(student);
            student.MethodToExecute = MoveToMatch;

            if (refresh) StudentMoved?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Match Stuff
        /// <summary>
        /// Moves the given student into a match in the match table.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="student"></param>
        private void MoveToMatch(object sender, EditStudent student)
        {
            if (MatchesMade.Count >= 1 && MatchesMade[MatchesMade.Count - 1].Id2 == 0)
            {
                Match match = MatchesMade[MatchesMade.Count - 1];
                match.Id2 = student._Student.Id;
                match.Last2 = student._Student.Last;
                match.First2 = student._Student.First;
                match.Home2 = student._Student.Homeroom;
                match.Grade2 = student._Student.Grade;
            }
            else
            {
                topMatchNo += 1;
                MatchesMade.Add(
                    Matcher.PassStudent(student._Student, topMatchNo, round));
            }

            studentsInMatches.Add(student._Student.Id, student);

            Table1Students.Remove(student);
            StudentMoved?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Deletes the selected match from the match table,
        /// and places the students involved in it back in the
        /// student table.
        /// </summary>
        /// <param name="parameter"></param>
        private void DeleteMatch(object parameter)
        {
            if (Display3.SelectedCells.Count == 0) return;

            Match matchToDelete = (Match)Display3.SelectedItem;
            MatchesMade.Remove(matchToDelete);
            topMatchNo = originalTopMatchNo;

            // Re-generate the match ids.
            foreach (Match match in MatchesMade)
            {
                topMatchNo++;
                match.GenerateID(topMatchNo);
            }

            // Move the first student back.
            EditStudent student1 = studentsInMatches[matchToDelete.Id1];
            MoveToStudentTable(student1, false);
            studentsInMatches.Remove(student1._Student.Id);

            // Move the second student back, if they are real.
            if (matchToDelete.Id2 != 0)
            {
                EditStudent student2 = studentsInMatches[matchToDelete.Id2];
                MoveToStudentTable(student2, false);
                studentsInMatches.Remove(student1._Student.Id);
            }

            StudentMoved?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Randomize Stuff
        private async void Randomize(object parameter)
        {
            await Task.Run(RandomizeThread);

            StudentMoved?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Performs the necessary operations for randomize in an async thread.
        /// </summary>
        /// <returns></returns>
        private async Task RandomizeThread()
        {
            List<Student> studentsToRandomize = StudentsToRandomize
                .Select(student => student._Student)
                .ToList();

            List<Match> matchesToAdd = await Matcher.MakeMatches(studentsToRandomize, topMatchNo, round);

            // Add new matches to the matches table
            MatchesMade.AddRange(matchesToAdd);

            // Add to the topmatch number
            topMatchNo += matchesToAdd.Count;

            // Clear the students from the randomize table.
            StudentsToRandomize.Clear();
        }

        private void AddToRandomize(object obj)
        {
            foreach (EditStudent student in Display1.SelectedItems)
            {
                StudentsToRandomize.Add(student);
                student.MethodToExecute = RemoveFromRandomize;
                Table1Students.Remove(student);
            }

            StudentMoved?.Invoke(this, new EventArgs());
        }

        private void RemoveFromRandomize(object sender, EditStudent student)
        {
            StudentsToRandomize.Remove(student);
            student.MethodToExecute = MoveToMatch;
            Table1Students.Add(student);

            StudentMoved?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Setup
        public async void Setup(School school, List<Match> matchesToEdit, MatchEditor parentWindow)
        {
            EditWindow = parentWindow;
            _School = school;
            round = _School.GetCurrRoundNo();
            topMatchNo = _School.CurrMatchNo;
            originalTopMatchNo = _School.CurrMatchNo;
            inputtedMatches = matchesToEdit;

            Display1 = EditWindow.StudentGrid;
            Display2 = EditWindow.RandomizeGrid;
            Display3 = EditWindow.MatchGrid;

            // Update the delete match command when the selected cells change.
            Display3.SelectedCellsChanged += (object sender, SelectedCellsChangedEventArgs e) => DeleteMatchCommand.RaiseCanExecuteChanged();

            Table1Students = await GetStudentsFromMatches(inputtedMatches);

            #region Setting up relaycommands
            // Randomize command
            RandomizeCommand.CanExecuteDeterminer = () => StudentsToRandomize.Count > 0;
            RandomizeCommand.FunctionToExecute = Randomize;

            // Save and close
            SaveCloseCommand.CanExecuteDeterminer = CanSave;
            SaveCloseCommand.FunctionToExecute = SaveClose;

            // Print and close
            SavePrintCommand.CanExecuteDeterminer = CanSave;
            SavePrintCommand.FunctionToExecute = PrintClose;

            // Discard and close
            DiscardCloseCommand.CanExecuteDeterminer = () => true;
            DiscardCloseCommand.FunctionToExecute = DiscardClose;

            // Add to randomize
            AddToRandomizeCommand.CanExecuteDeterminer = () => true;
            AddToRandomizeCommand.FunctionToExecute = AddToRandomize;

            // Delete Match Command
            DeleteMatchCommand.CanExecuteDeterminer = () => Display3.SelectedCells.Count > 0;
            DeleteMatchCommand.FunctionToExecute = DeleteMatch;

            #endregion

            Refresh();
        }

        private async Task<List<EditStudent>> GetStudentsFromMatches(List<Match> matches)
        {
            List<EditStudent> returnable = new List<EditStudent>();
            foreach (Match match in matches)
            {
                returnable.AddRange(_School.GetStudentsInMatch(match)
                    .Select(student => new EditStudent
                    {
                        _Student = student,
                        MethodToExecute = MoveToMatch

                    })
                    .ToList()
                    );
            }

            await Task.Delay(0);
            return returnable;
        }
        #endregion

        #region Save and Close
        private void SaveClose(object parameter)
        {
            Save();

            EditWindow.Close();
        }

        /// <summary>
        /// Function to commit the changes made in this edit window
        /// to the School object, and save all changes.
        /// </summary>
        private async void Save()
        {
            // Eliminate the students left in the student table still.
            List<Student> studentsToEliminate =
                Table1Students
                .Select(student => student._Student)
                .ToList();

            await _School.EliminateStudents(studentsToEliminate);

            // Add the generated matches
            await _School.AddEditedMatches(MatchesMade);
        }

        private bool CanSave()
        {
            return MatchesMade.Count > 0 && StudentsToRandomize.Count == 0;
        }

        /// <summary>
        /// Function to save changes and print the new matches.
        /// </summary>
        /// <param name="parameter"></param>
        private void PrintClose(object parameter)
        {
            Save();

            new InstantPrintHandler(_School, ProcessButtonSubsystem.CurrentInstance, MatchesMade);

            EditWindow.Close();
        }

        /// <summary>
        /// Function to discard changes and close the dialog.
        /// </summary>
        /// <param name="parameter"></param>
        private void DiscardClose(object parameter)
        {
            EditWindow.Close();
        }
        #endregion

        /// <summary>
        /// Refreshes the relaycommands and the GUI
        /// </summary>
        private void Refresh(object sender = null, EventArgs e = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs("Table1Students"));
            PropertyChanged(this, new PropertyChangedEventArgs("StudentsToRandomize"));
            PropertyChanged(this, new PropertyChangedEventArgs("MatchesMade"));

            // Refresh commands
            AddToRandomizeCommand.RaiseCanExecuteChanged();
            RandomizeCommand.RaiseCanExecuteChanged();
            SaveCloseCommand.RaiseCanExecuteChanged();
            SavePrintCommand.RaiseCanExecuteChanged();
            DiscardCloseCommand.RaiseCanExecuteChanged();

            Display1.Items.Refresh();
            Display2.Items.Refresh();
            Display3.Items.Refresh();
        }
    }
}
