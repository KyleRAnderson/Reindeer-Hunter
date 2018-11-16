using Reindeer_Hunter.Hunt;
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

        private Dictionary<string, EditStudent> student_directory;

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
        
        /// <summary>
        /// A dictionary of all the students in matches currently, so we know who's been dealt with.
        /// </summary>
        private Dictionary<string, EditStudent> studentsInMatches = new Dictionary<string, EditStudent>();

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
            if (MatchesMade.Count >= 1 && string.IsNullOrEmpty(MatchesMade[MatchesMade.Count - 1].Id2))
            {
                int index = MatchesMade.Count - 1;
                EditStudent student1 = student_directory[MatchesMade[index].Id1];
                MatchesMade.RemoveAt(index);
                MatchesMade.Insert(index, Matcher.GenerateMatch(student1._Student, student._Student, topMatchNo, round));
            }
            else
            {
                topMatchNo++;
                MatchesMade.Add(
                    Matcher.PassStudent(student._Student, topMatchNo, round));
            }

            studentsInMatches.Add(student._Student.Id, student);

            Table1Students.Remove(student);
            StudentMoved?.Invoke(this, new EventArgs());
        }

        private void MoveToPassMatch(object sender, EditStudent student)
        {
            topMatchNo++;
            MatchesMade.Add(
                Matcher.PassStudent(student._Student, topMatchNo, round));

            studentsInMatches.Add(student._Student.Id, student);


            Table1Students.Remove(student);
            StudentMoved?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Just calls the DeleteMatchesAsync method properly.
        /// </summary>
        /// <param name="parameter"></param>
        private async void DeleteMatches(object parameter)
        {
            await Task.Run(DeleteMatchesAsync);
        }

        /// <summary>
        /// Deletes the selected match from the match table,
        /// and places the students involved in it back in the
        /// student table.
        /// </summary>
        /// <param name="parameter"></param>
        private async Task DeleteMatchesAsync()
        {
            if (Display3.SelectedCells.Count == 0) return;

            List<Match> matchesToDelete = new List<Match>(Display3.SelectedItems.Cast<Match>());

            // Get rid of the matches, re-assign the students.
            foreach (Match matchToDelete in matchesToDelete)
            {
                MatchesMade.Remove(matchToDelete);

                // Move the first student back.
                EditStudent student1 = studentsInMatches[matchToDelete.Id1];
                MoveToStudentTable(student1, false);
                studentsInMatches.Remove(student1._Student.Id);

                // Move the second student back, if they are real.
                if (!string.IsNullOrEmpty(matchToDelete.Id2))
                {
                    EditStudent student2 = studentsInMatches[matchToDelete.Id2];
                    MoveToStudentTable(student2, false);
                    studentsInMatches.Remove(student2._Student.Id);
                }
            }

            // Reset the match ids.
            topMatchNo = originalTopMatchNo;

            // Re-generate the match ids.
            foreach (Match match in MatchesMade)
            {
                topMatchNo++;
                match.GenerateID(topMatchNo);
            }

            StudentMoved?.Invoke(this, new EventArgs());

            await Task.Delay(0);
        }
        #endregion

        #region Randomize Stuff
        private async void Randomize(object parameter)
        {
            await RandomizeStudentsAsync();

            StudentMoved?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Performs the necessary operations for randomize in an async thread.
        /// </summary>
        /// <returns></returns>
        private async Task RandomizeStudentsAsync()
        {
            List<Student> studentsToRandomize = StudentsToRandomize
                .Select(student => student._Student)
                .ToList();

            List<Match> matchesToAdd = await Task.Run(() => Matcher.MakeMatches(studentsToRandomize, topMatchNo, round));

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

            Table1Students = await Task.Run(() => GetStudentsFromMatches(inputtedMatches));

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
            DeleteMatchCommand.FunctionToExecute = DeleteMatches;

            #endregion

            Refresh();
        }

        private async Task<List<EditStudent>> GetStudentsFromMatches(List<Match> matches)
        {
            Dictionary<string, EditStudent> returnable = new Dictionary<string, EditStudent>();
            foreach (Match match in matches)
            {
                List<Student> students = _School.GetStudentsInMatch(match);
                foreach (Student student in students)
                {
                    if (returnable.ContainsKey(student.Id)) continue;

                    returnable.Add(student.Id, new EditStudent
                    {
                        _Student = student,
                        MethodToExecute = MoveToMatch,
                        PassMethod = MoveToPassMatch
                    });
                }
            }

            student_directory = returnable;

            await Task.Delay(0);
            return returnable.Values.ToList();
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
