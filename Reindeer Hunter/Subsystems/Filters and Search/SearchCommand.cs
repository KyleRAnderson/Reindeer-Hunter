using Reindeer_Hunter.Data_Classes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Reindeer_Hunter.Subsystems.SearchAndFilters
{
    /// <summary>
    /// Class for searching through the list of 
    /// matches and students based on user input.
    /// </summary>
    public class SearchCommand : ICommand
    {
        // The subsystem object that is managing this command.
        public FiltersAndSearch Filters_Subsystem;

        public School _School { get; set; }
        private string UserInput { get; set; }
        private SearchQuery Query;

        public event EventHandler CanExecuteChanged;

        public SearchCommand(FiltersAndSearch subsystem)
        {
            Filters_Subsystem = subsystem;
        }

        public SearchQuery CurrentQuery
        {
            get { return Query; }
        }

        /// <summary>
        /// Function for displaying error message when no results found.
        /// </summary>
        private void NotFound()
        {
            System.Windows.Forms.MessageBox.Show("The query for \"" + UserInput + "\" returned no results with given filters.",
                "Error - No results found.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }

        /// <summary>
        /// Method for figuring out what type of data the user has inputted to search for.
        /// </summary>
        /// <returns>A SearchQuery object filled with the decrypted values of the search.</returns>
        private SearchQuery DecryptString()
        {
            /* The search query can be a: 
             * Homeroom number - It would have no spaces after trim, and would be 
             * 3-4 characters in length. Would be able to parse it into an integer.
             * 
             * Student Number - No spaces (after trim) and is 6 characters in length.
             * Parseable as an integer.
             * 
             * Student Name - One space (between the first and last) and no specific length.
             * 
             * Match ID - No spaces (after trim) and a length of at least 6 characters. 
             * First few characters are "MAR" + Round#.
             * 
             * */

            // Create parameters to fill in later.
            string matchId = "";
            string studentName = "";
            int studentNo = 0;
            int homeroomNo = 0;

            // If it's empty, it's garbage
            if (UserInput.Length == 0) return null;

            // Check for matchId. Only match id has 3 strings then number.
            else if (UserInput.Length > 2 && UserInput.Substring(0, 3) == "MAR" && int.TryParse(UserInput.Substring(3, 1), out int roundNo)) matchId = UserInput;

            // Check for student number
            else if (UserInput.Substring(0, 1) == "S" && int.TryParse(UserInput.Substring(1, UserInput.Length - 1), out studentNo)) { }

            // Check for homeroom number
            else if (int.TryParse(UserInput, out homeroomNo)) { }

            // If nothing else is caught, try a student name.
            else studentName = UserInput;

            return new SearchQuery
            {
                MatchId = matchId,
                StudentName = studentName,
                StudentNo = studentNo,
                Homeroom =homeroomNo
            };
        }

        public async Task<List<Match>> Search(string searchInput, Filter filter)
        {
            UserInput = searchInput;
            List<Match> returnMatchList = null;


            if (UserInput == "")
            {
                NotFound();
            }

            UserInput = UserInput.Trim();
            Query = DecryptString();
            if (Query == null)
            {
                NotFound();
            }

            returnMatchList = _School.GetMatches(Query, filter);
            if (returnMatchList == null || returnMatchList.Count == 0)
            {
                NotFound();
                returnMatchList = null;
            }

            await Task.Delay(0);
            return returnMatchList;
        }

        /// <summary>
        /// Function to clear the query object to effectively "clear" the search.
        /// </summary>
        public void ClearSearch()
        {
            Query = null;
        }

        public bool CanExecute(object parameter)
        {
            // If matches were just made, disable search.
            if (Filters_Subsystem.ManagerProperty._ProcessButtonSubsystem.AreMatchesMade) return false;
            // For whatever reason, it is always null the first time, so we'll handle this.
            else if (parameter == null) return true;

            // Can't execute if it's empty, or if it's the default.
            if (parameter.ToString() == "" || parameter.ToString() ==
                "Search for students, homerooms or matches...") return false;
            // Otherwise, it should be enabled.
            else return true;
        }

        public async void Execute(object parameter)
        {
            // In case it's empty
            if (parameter == null)
            {
                await Task.Delay(0);
                return;
            }

            List<Match> resultsList =
                await Task.Run(() => Search(parameter.ToString(), Filters_Subsystem.GetFilters()));

            if (resultsList == null) return;
            Filters_Subsystem.SetSearchResults(resultsList);
        }

        /// <summary>
        /// Function that raises the canExecuteChanged event to refresh the executability of this command.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, new EventArgs());
        }
    }
}
