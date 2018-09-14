using Reindeer_Hunter.Hunt;
using System;
using System.Collections.Generic;

namespace Reindeer_Hunter.Data_Classes
{
    public class NewMatches_EventArgs : EventArgs
    {
        public List<Match> NewMatches { get; set; }

        /// <summary>
        /// Class used to pass the new match objects in the MatchesMade event.
        /// </summary>
        /// <param name="newMatches"></param>
        public NewMatches_EventArgs(List<Match> newMatches)
        {
            NewMatches = newMatches;
        }
    }
}
