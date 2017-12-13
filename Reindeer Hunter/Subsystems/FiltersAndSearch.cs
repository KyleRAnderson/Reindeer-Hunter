using System;
using System.Collections.Generic;
using Reindeer_Hunter.Subsystems.SearchAndFilters;
using Reindeer_Hunter.Data_Classes;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using Reindeer_Hunter.DataCards;
using System.Threading.Tasks;

namespace Reindeer_Hunter.Subsystems
{
    /// <summary>
    /// The substystem in charge of commands dealing with filters and search.
    /// It is also in charge of the MainDisplay content. 
    /// </summary>
    public class FiltersAndSearch : Subsystem, INotifyPropertyChanged
    {
        /// <summary>
        /// Fired when a match is added to the editing queue. Contains the match added.
        /// </summary>
        public event EventHandler<Match> MatchAddedToQueue;

        public static readonly int Pass1ColumnIndex = 0;
        public static readonly int Full1ColumnIndex = 1;
        public static readonly int MatchIdColumnIndex = 2;
        public static readonly int Full2ColumnIndex = 3;
        public static readonly int Pass2ColumnIndex = 4;

        public event EventHandler<Tuple<Student, Match>> StudentAddedToPassQueue;

        // The filter object that will contain the filters.
        public Filter CurrentFilters { get; set; } = new Filter();

        // The commands for this subsystem
        public ClearFiltersAndSearch ClearFiltersCommand { get; }
        public SearchCommand Searcher { get; }
        public RelayCommand PropertiesPopup { get; } = new RelayCommand
        {
            CanExecuteDeterminer = () => true
        };

        /// <summary>
        /// Simple way of getting the number of students on screen
        /// </summary>
        public int Number_Of_Matches_Being_Displayed
        {
            get
            {
                return MainDisplay_Display_List.Count;
            }
        }

        // The GUI element objects that this subsystem controls.
        private MenuItem Filter_Menu { get; set; }
        private CheckBox OpenCheckbox { get; set; }
        private CheckBox ClosedCheckbox { get; set; }
        private MenuItem RoundFilter { get; set; }
        private TextBox SearchBox { get; set; }

        private readonly string searchBox_Default_Text = "Search \"S + [Student Id]\", \"[Homeroom]\", \"[Match Number]\"";

        public List<Match> MainDisplay_Display_List { get; private set; } = new List<Match>();

        private DataGrid MainDisplay;

        public event PropertyChangedEventHandler PropertyChanged;

        public FiltersAndSearch() : base()
        {
            // Create the commands.
            ClearFiltersCommand = new ClearFiltersAndSearch(this);
            Searcher = new SearchCommand(this);
            PropertiesPopup.FunctionToExecute = PropertiesPopuper;
        }

        /// <summary>
        /// Function that does stuff whenever the manager is set by subscribing to the
        /// OnManagerSet event.
        /// In this case, it sets the MainDisplay object to the proper object.
        /// </summary>
        protected override void OnHomePageSet(object sender, EventArgs e)
        {
            // Call the base function
            base.OnHomePageSet(sender, e);

            // Set the MainDisplay object, and then set the itemssource
            MainDisplay = Manager.Home.MainDisplay;

            // Define our checkboxes and menu items
            Filter_Menu = Manager.Home.Search_Menu;
            OpenCheckbox = Manager.Home.Open_Filter;
            ClosedCheckbox = Manager.Home.Closed_Filter;
            RoundFilter = Manager.Home.Round_Filter;
            SearchBox = Manager.Home.search_box;
            // Subscribe to their events
            OpenCheckbox.Click += LoadContent;
            ClosedCheckbox.Click += LoadContent;
            SearchBox.GotFocus += Search_Box_Got_Focus;

            // Reset the filters
            ResetFilters();

            // Subscribe to match result removed event
            Manager._Passer.ResultRemoved += OnMatchResultRemoved;

            // Subscribe to increased round event
            _School.RoundIncreased += ResetFilters;

            // Subscribe to click events
            MainDisplay.MouseRightButtonDown += PopupProperties;
            MainDisplay.MouseDoubleClick += AddMatchToEditQueue;
            MainDisplay.SelectedCellsChanged += SelectedCellsChanged;

            /* Subscribe to more events that merit re-loading of the itemssource,
             * namely MatchesMade and PassingStudentsSaved.
             */
            Manager._Passer.PassingStudentsSaved += LoadContent;
            Manager._ProcessButtonSubsystem.MatchesRegistered += OnMatchesSaved;
            _School.MatchChangeEvent += OnMatchesSaved;
            Manager._ProcessButtonSubsystem.MatchesDiscarded += LoadContent;
            Manager._ProcessButtonSubsystem.MatchesMade += ShowNewMatches;

            // Subscribe to save and discard events
            Manager._SaveDiscard.Save += OnSaveDiscard;
            Manager._ProcessButtonSubsystem.MatchesDiscarded += OnSaveDiscard;

            // Give the SearchCommand the School object
            Searcher._School = _School;
        }

        private void SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            // If Invalid or passmatch, return.
            if (!MainDisplay.CurrentCell.IsValid || School.IsPassMatch((Match)MainDisplay.CurrentItem)) return;

            Match match = (Match)MainDisplay.CurrentItem;

            bool isStudent1 = MainDisplay.CurrentCell.Column.DisplayIndex == Pass1ColumnIndex;
            bool isStudent2 = MainDisplay.CurrentCell.Column.DisplayIndex == Pass2ColumnIndex;

            Student student;
            if (isStudent1)
            {
                student = _School.GetStudent(match.Id1);
            }
            else if (isStudent2)
            {
                student = _School.GetStudent(match.Id2);
            }
            else return;

            /* Clear the selection. When this happens, the SelectedCellsChanged event is called
             * again and so therefore we need to unsubscribe then re-subscribe
             */
            MainDisplay.SelectedCellsChanged -= SelectedCellsChanged;
            MainDisplay.SelectedCells.Clear();
            MainDisplay.SelectedCellsChanged += SelectedCellsChanged;

            StudentAddedToPassQueue?.Invoke(this, new Tuple<Student, Match>(student, match));
        }

        private void Search_Box_Got_Focus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == searchBox_Default_Text) SearchBox.Clear();
        }

        /// <summary>
        /// Function called either on the save or discard events. 
        /// It just re-enabled the Filter drop down if it was disabled 
        /// by the ShowNewMatches function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnSaveDiscard(object sender, EventArgs e)
        {
            Filter_Menu.IsEnabled = true;
            Searcher.RaiseCanExecuteChanged();
        }

        public void OnMatchesSaved(object sneder, EventArgs e)
        {
            // Refresh the search command.
            Searcher.RaiseCanExecuteChanged();

            // Reload the matches
            LoadContent();
        }

        public void RefreshDisplay()
        {
            MainDisplay.Items.Refresh();
        }

        /// <summary>
        /// Gets the currently set filters.
        /// </summary>
        /// <returns></returns>
        public Filter GetFilters()
        {
            return CurrentFilters;
        }

        /// <summary>
        /// Async Function to reset the filters to the default.
        /// </summary>
        public async void ResetFilters(object sender = null, EventArgs e = null)
        {
            await ResetFiltersAsync();
        }

        /// <summary>
        /// Async Function to reset the filters to the default.
        /// </summary>
        public async Task ResetFiltersAsync()
        {

            await Task.Delay(0);
            // Create the filter object 
            List<long> rounds = new List<long>();

            for (long a = 1; a <= _School.GetCurrRoundNo(); a++) rounds.Add(a);

            CurrentFilters.Closed = false;
            CurrentFilters.Open = true;
            Searcher.ClearSearch();

            // Clear the searchbox text
            SearchBox.Text = searchBox_Default_Text;

            // Set the round filters and then refres the GUI for them.
            CurrentFilters.Round = rounds;
            RoundFilter.Items.Refresh();

            // Subscribe to the newly created checkboxes' events.
            foreach (CheckBox checkbox in CurrentFilters.RoundCheckboxes)
            {
                checkbox.Click += LoadContent;
            }

            // Notify the UI that the filters have changed and that it should update.
            PropertyChanged(this, new PropertyChangedEventArgs("CurrentFilters"));

            LoadContent();

            await Task.Delay(0);
        }

        /// <summary>
        /// Simple function to load content onto the MainDisplay object.
        /// </summary>
        private async void LoadContent(object sender = null, EventArgs e = null)
        {
            MainDisplay_Display_List = await GetMatches();
            // Notify UI that the Main Display Match List has changed and it should update.
            PropertyChanged(this, new PropertyChangedEventArgs("MainDisplay_Display_List"));
        }

        public async Task<List<Match>> GetMatches()
        {
            List<Match> returnable;

            if (Searcher.CurrentQuery == null)
            {
                returnable = _School.GetMatches(CurrentFilters);
            }
            else
            {
                returnable = 
                    _School.GetMatches(Searcher.CurrentQuery, CurrentFilters);
            }

            await Task.Delay(0);
            return returnable;
        }

        /// <summary>
        /// Function subscribed to the ResultRemoved event, to update the maindisplay
        /// on the match that was removed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="result"></param>
        private void OnMatchResultRemoved(object sender, EventArgs e)
        {
            MatchGuiResult result = (MatchGuiResult)e;
            foreach (Match match in MainDisplay_Display_List)
            {
                if (match.MatchId == result.MatchID)
                {
                    if (result.StuID == match.Id1) match.Pass1 = false;
                    else match.Pass2 = false;
                    MainDisplay.Items.Refresh();
                    return;
                }
            }
        }

        /// <summary>
        /// Function to set the search results of a search so that the MainDisplay shows themm
        /// </summary>
        /// <param name="searchResults">The search results to display.</param>
        public void SetSearchResults(List<Match> searchResults)
        {
            MainDisplay_Display_List = searchResults;
            PropertyChanged(this, new PropertyChangedEventArgs("MainDisplay_Display_List"));
        }

        /// <summary>
        /// Used as a relay for the relay command to trigger the DoubleClickPopup function
        /// </summary>
        /// <param name="parameter"></param>
        private void PropertiesPopuper(object parameter) { PopupProperties(); }

        /// <summary>
        /// Called by the MainDisplay mouse double click event.
        /// Displays the popup info on the double clicked item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PopupProperties(object sender = null, MouseButtonEventArgs e = null)
        {
            // Do nothing if no cell is selected.
            if (!MainDisplay.CurrentCell.IsValid) return;

            // This error is produced when the user presses the sort buttons repeatedly, so avoid crash. 
            Match match;
            try
            {
                match = (Match)MainDisplay.CurrentCell.Item;
            }
            catch (InvalidCastException)
            {
                return;
            }
            
            int currentColumnIndex = MainDisplay.CurrentCell.Column.DisplayIndex;

            StartupWindow masterWindow = Manager.Home.MasterWindow;
            DataCardWindow window;
            // Indicating that the user double clicked on the first student's name
            if (currentColumnIndex == Full1ColumnIndex)
            {
                window = new DataCardWindow(Manager.Home.MasterWindow._School, studentId: match.Id1);
            }

            // Indicating they hit the match id
            else if (currentColumnIndex == MatchIdColumnIndex)
            {
                // In case it is a fake match.
                if (match.MatchId == "") return;
                window = new DataCardWindow(masterWindow._School, matchId: match.MatchId);
            }

            // Indicating they hit the second student's name
            else if (currentColumnIndex == Full2ColumnIndex)
            {
                // In case it is a pass student or a fake match
                if (match.Id2 == 0) return;
                window = new DataCardWindow(masterWindow._School, studentId: match.Id2);
            }
            else return;

            // If we didn't return, show the window as a dialog.
            window.ShowDialog();

            // If a student is deleted, clear the filters
            if (window.CloseStatus == DataCardWindow.STUDENT_DELETED) ResetFilters();
        }

        /// <summary>
        /// Function called by the MatchesMade event (which fires once matches are
        /// made but not yet saved) to display the newly made matches.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ShowNewMatches(object sender, NewMatches_EventArgs e)
        {
            // Disable the filters to avoid issues
            Filter_Menu.IsEnabled = false;

            // Refresh the search button to disable it too
            Searcher.RaiseCanExecuteChanged(); 

            // Get the new matches from the EventArgs and tell the GUI that the property changed
            MainDisplay_Display_List = e.NewMatches;
            PropertyChanged(this, new PropertyChangedEventArgs("MainDisplay_Display_List"));
        }

        /// <summary>
        /// Adds the match to the match edit queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddMatchToEditQueue(object sender, MouseButtonEventArgs e)
        {
            int column = MainDisplay.CurrentCell.Column.DisplayIndex;
            // Do nothing if no cell is selected.
            if (!MainDisplay.CurrentCell.IsValid || column == Pass1ColumnIndex || column == Pass2ColumnIndex) return;

            // This error is produced when the user presses the sort buttons repeatedly, so avoid crash. 
            Match match;
            try
            {
                match = (Match)MainDisplay.CurrentCell.Item;
            }
            catch (InvalidCastException)
            {
                return;
            }

            if (column < MatchIdColumnIndex)
            {
                Student studentToDisplay = _School.GetStudent(match.Id1);
                if (studentToDisplay != null)
                {
                    match = _School.CreateFakeMatch(studentToDisplay);
                    match.MatchId = studentToDisplay.FullName;
                }
            }
            else if (column > MatchIdColumnIndex)
            {
                Student studentToDisplay = _School.GetStudent(match.Id2);
                if (studentToDisplay != null)
                {
                    match = _School.CreateFakeMatch(studentToDisplay);
                    match.MatchId = studentToDisplay.FullName;
                }
            }

            // If all is good, call the event.
            MatchAddedToQueue?.Invoke(this, match);
        }
    }
}
