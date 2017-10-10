using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reindeer_Hunter.Subsystems.SearchAndFilters;
using Reindeer_Hunter.Data_Classes;
using System.Windows.Controls;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Input;

namespace Reindeer_Hunter.Subsystems
{
    /// <summary>
    /// The substystem in charge of commands dealing with filters and search.
    /// It is also in charge of the MainDisplay content. 
    /// </summary>
    public class FiltersAndSearch : Subsystem, INotifyPropertyChanged
    {
        // The filter object that will contain the filters.
        public Filter CurrentFilters { get; set; } = new Filter();

        // The commands for this subsystem
        public ClearFiltersAndSearch ClearFiltersCommand { get; }
        public SearchCommand Searcher { get; }

        // The GUI element objects that this subsystem controls.
        private System.Windows.Controls.MenuItem Filter_Menu { get; set; }
        private System.Windows.Controls.CheckBox OpenCheckbox { get; set; }
        private System.Windows.Controls.CheckBox ClosedCheckbox { get; set; }
        private System.Windows.Controls.MenuItem RoundFilter { get; set; }

        public List<Match> MainDisplay_Display_List { get; private set; } = new List<Match>();

        private System.Windows.Controls.DataGrid MainDisplay;

        public event PropertyChangedEventHandler PropertyChanged;

        public FiltersAndSearch() : base()
        {
            // Create the commands.
            ClearFiltersCommand = new ClearFiltersAndSearch(this);
            Searcher = new SearchCommand(this);
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
            // Subscribe to their events
            OpenCheckbox.Click += LoadContent;
            ClosedCheckbox.Click += LoadContent;

            // Reset the filters
            ResetFilters();

            // Subscribe to match result removed event
            Manager._Passer.ResultRemoved += OnMatchResultRemoved;

            // Subscribe to increased round event
            _School.RoundIncreased += ResetFilters;

            // Subscribe to double click event
            MainDisplay.MouseDoubleClick += DoubleClickPopup;

            /* Subscribe to more events that merit re-loading of the itemssource,
             * namely MatchesMade and PassingStudentsSaved.
             */
            Manager._Passer.PassingStudentsSaved += LoadContent;
            Manager._ProcessButtonSubsystem.MatchesRegistered += OnMatchesSaved;
            Manager._ProcessButtonSubsystem.MatchesDiscarded += LoadContent;
            Manager._ProcessButtonSubsystem.MatchesMade += ShowNewMatches;

            // Subscribe to save and discard events
            Manager._SaveDiscard.Save += OnSaveDiscard;
            Manager._SaveDiscard.Discard += OnSaveDiscard;

            // Give the SearchCommand the School object
            Searcher._School = _School;
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
        }

        public void OnMatchesSaved(object sneder, EventArgs e)
        {
            // Refresh the search command.
            Searcher.RaiseCanExecuteChanged();

            // Reload the matches
            LoadContent();
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
        /// Function to reset the filters to the default.
        /// </summary>
        public void ResetFilters(object sender = null, EventArgs e = null)
        {
            // Create the filter object 
            List<long> rounds = new List<long>();

            for (long a = 1; a <= _School.GetCurrRoundNo(); a++) rounds.Add(a);

            CurrentFilters.Closed = false;
            CurrentFilters.Open = true;

            // Set the round filters and then refres the GUI for them.
            CurrentFilters.Round = rounds;
            RoundFilter.Items.Refresh();

            // Subscribe to the newly created checkboxes' events.
            foreach (System.Windows.Controls.CheckBox checkbox in CurrentFilters.RoundCheckboxes)
            {
                checkbox.Click += LoadContent;
            }

            // Notify the UI that the filters have changed and that it should update.
            PropertyChanged(this, new PropertyChangedEventArgs("CurrentFilters"));
            LoadContent(); 
        }

        /// <summary>
        /// Simple function to load content onto the MainDisplay object.
        /// </summary>
        public void LoadContent(object sender = null, EventArgs e = null)
        {
            if (Searcher.CurrentQuery == null)
            {
                MainDisplay_Display_List = _School.GetMatchesWithFilter(CurrentFilters);
            }
            else
            {
                MainDisplay_Display_List = 
                    _School.GetSearchResults(Searcher.CurrentQuery, CurrentFilters);
            }

            // Notify UI that the Main Display Match List has changed and it should update.
            PropertyChanged(this, new PropertyChangedEventArgs("MainDisplay_Display_List"));
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
        /// Called by the MainDisplay mouse double click event.
        /// Displays the popup info on the double clicked item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DoubleClickPopup(object sender, MouseButtonEventArgs e)
        {
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
            if (currentColumnIndex == 1 || currentColumnIndex == 2)
            {
                window = new DataCardWindow(Manager.Home.MasterWindow._School, studentId: match.Id1);
            }

            // Indicating they hit the match id
            else if (currentColumnIndex == 3)
            {
                // In case it is a fake match.
                if (match.MatchId == "") return;
                window = new DataCardWindow(masterWindow._School, matchId: match.MatchId);
            }

            // Indicating they hit the second student's name
            else if (currentColumnIndex == 4 || currentColumnIndex == 5)
            {
                // In case it is a pass student or a fake match
                if (match.Id2 == 0) return;
                window = new DataCardWindow(masterWindow._School, studentId: match.Id2);
            }
            else return;

            // If we didn't return, show the window as a dialog.
            window.ShowDialog();
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
    }
}
