﻿using Reindeer_Hunter.Data_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Reindeer_Hunter.Commands
{
    /// <summary>
    /// Class for searching through the list of 
    /// matches and students based on user input.
    /// </summary>
    public class SearchCommand
    {
        public School _School { get; set; }
        private string UserInput { get; set; }

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

            // Check for student name. Only student name has spaces in it after trim.
            if (UserInput.Count(Char.IsWhiteSpace) > 0) studentName = UserInput;

            // Check for matchId. Only match id has 3 strings then number.
            else if (UserInput.Substring(0, 3) == "MAR" && int.TryParse(UserInput.Substring(3, 1), out int roundNo)) matchId = UserInput;

            // Check for student number
            else if (UserInput.Length == 6 && int.TryParse(UserInput, out studentNo)) { }

            // Check for homeroom number
            else if (4 <= UserInput.Length && UserInput.Length < 6 && int.TryParse(UserInput, out homeroomNo)) { }

            // Otherwise, error.
            else return null;

            return new SearchQuery
            {
                MatchId = matchId,
                StudentName = studentName,
                StudentNo = studentNo,
                Homeroom = homeroomNo
            };
        }

        public List<Match> Search(string searchInput, Filter filter)
        {
            UserInput = searchInput;
            if (UserInput == "")
            {
                NotFound();
                return null;
            }

            UserInput = UserInput.Trim();
            SearchQuery query = DecryptString();
            if (query == null)
            {
                NotFound();
                return null;
            }

            return _School.GetSearchResults(query, filter);
        }
    }
}