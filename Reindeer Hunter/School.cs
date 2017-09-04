using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Reindeer_Hunter
{
    /// <summary>
    /// This class will be in charge of evereything. Holds the lists of students and other information.
    /// </summary>
    public class School
    {
        // Event raised when something about matches is changed/updated
        public event EventHandler MatchChangeEvent;

        // This dictionary will contain all data for the program
        protected static Hashtable data;
        protected static Dictionary<int, Student> student_directory;
        protected static Dictionary<string, Match> match_directory;
        protected static Hashtable misc;

        protected static DataFileIO dataFile;

        private string StudentKey = "students";

        public School()
        {
            try
            {
                // Make the new dataFile string and get the data from the data file.
                dataFile = new DataFileIO();
                data = dataFile.Read();
            }
            catch (ProgramNotSetup)
            {
                // Set up the program if it has no data yet.
                FirstTimeSetup();
                data = dataFile.Read(); // TODO Do this better?
            }

            // Declare these for simplicity and ease of use.
            student_directory = (Dictionary < int, Student> )data[StudentKey];
            match_directory = (Dictionary <string,  Match > )data["matches"];
            misc = (Hashtable)data["misc"];
        }

        /// <summary>
        /// Returns true if students exist already, false otherwise.
        /// </summary>
        /// <returns>True if at least one student exists, false otherwise. </returns>
        public bool IsData()
        {
            // True when there is discovered data.
            bool isData = false;
            Dictionary<int, List<Student>> grades = GetStudentsByGrade();
            
            foreach (KeyValuePair<int, List<Student>> pair in grades)
            {
                List<Student> grade = pair.Value;
                if (grade.Count() > 0)
                {
                    isData = true;
                    break;
                }
            }

            return isData;
        }

        /// <summary>
        /// Returns the match dictionary
        /// </summary>
        /// <returns>Match dictionary in the form of {"matchID", match}</returns>
        public Dictionary<string, Match> GetMatchDic() => match_directory;

        /// <summary>
        /// Returns a copy of the contents of match directory as a list
        /// </summary>
        /// <returns>A list of all matches</returns>
        public List<Match> GetMatchList()
        {
            List<Match> matchList = new List<Match>(match_directory.Values);
            List<Match> newMatchList = new List<Match>();
            foreach (Match match in matchList) newMatchList.Add(match.Clone());

            return newMatchList;
        }

        public void AddMatchResults(List<MatchGuiResult> matcheResults)
        {
            // Update everything
            foreach (MatchGuiResult matchResult in matcheResults)
            {
                Match match = match_directory[matchResult.MatchID];
                match.Closed = true;

                // Seems not needed, but in case two people are passed then it's needed.
                student_directory[matchResult.StuID].In = true;
                
                // if the victor is student 1, mark student 2 as not in and pass student 1
                if (matchResult.StuID == match.Id1)
                {
                    student_directory[match.Id2].In = false;
                    match.Pass1 = true;
                }
                // Otherwise, mark student 1 as out and pass student 2
                else
                {
                    student_directory[match.Id1].In = false;
                    match.Pass2 = true;
                }
            }

            Save();
            MatchChangeEvent(this, new EventArgs());
        }

        /// <summary>
        /// Returns a copy of the list of all currently open matches
        /// The Match objects have been cloned.
        /// </summary>
        /// <returns>A list of open matches as MatchDisplay Classes</returns>
        public List<Match> GetOpenMatchesList()
        {
            // Get a clone of the list of matches
            List<Match> matchList = GetMatchList();

            // Make new list for the open ones.
            List<Match> opnMatchList = new List<Match>();

            foreach (Match match in matchList)
            {
                if (!match.Closed) opnMatchList.Add(match);
            }
            return opnMatchList;
        }

        /// <summary>
        /// Get a list of all matches in the current round, open or not
        /// </summary>
        /// <returns>List of matches of the currrent round.</returns>
        public List<Match> GetCurrRoundMatches()
        {
            // Get a clone of the list of matches
            List<Match> matchList = GetMatchList();
            long currRound = GetCurrRoundNo();

            // Make new list for the open ones.
            List<Match> roundMatchList = new List<Match>();

            foreach (Match match in matchList)
            {
                if (match.Round == currRound) roundMatchList.Add(match);
            }
            return roundMatchList;
        }

        /// <summary>
        /// Returns a copy of the list of all currently closed matches
        /// The Match objects have been cloned.
        /// </summary>
        /// <returns>A list of open matches as MatchDisplay Classes</returns>
        public List<Match> GetClosedMatchesList()
        {
            // Get a clone of the list of matches
            List<Match> matchList = GetMatchList();

            // Make new list for the open ones.
            List<Match> closedMatchList = new List<Match>();

            foreach (Match match in matchList)
            {
                if (match.Closed) closedMatchList.Add(match);
            }
            return closedMatchList;
        }

        /// <summary>
        /// Used to find the current match number being used to automatically
        /// create match ids.
        /// </summary>
        /// <returns>The current match number.</returns>
        public long GetCurrMatchNo() => (long)misc["TopMatch"];

        /// <summary>
        /// Function to update the new top match id number after match creation
        /// </summary>
        /// <param name="matchNo">New match id number</param>
        public void SetCurrMatchNo(long matchNo)
        {
            misc["MatchNo"] = matchNo;
            Save();
        }

        /// <summary>
        /// Used to find the current round number. 
        /// </summary>
        /// <returns>The current round number.</returns>
        public long GetCurrRoundNo()
        {
            return (long)misc["RoundNo"];
        }

        /// <summary>
        /// Function to set the new round number to one above the current one.
        /// </summary>
        public void IncreaseCurrRoundNo()
        {
            misc["RoundNo"] = (long)misc["RoundNo"] + 1;
            long round = GetCurrRoundNo();

            foreach (KeyValuePair<int, Student> studentKeyValue in student_directory)
            {
                Student student = studentKeyValue.Value;
                if (student.In)
                {
                    student.LastRoundParticipated = round;
                }
            }

            Save();
        }

        /// <summary>
        /// Returns a dictionary of grades containing all students, in or out.
        /// </summary>
        /// <returns>A dictionary containing the student objects.
        /// Format: {key: grade, value: list of students in that grade}</returns>
        public Dictionary<int, List<Student>> GetAllStudentsByGrade()
        {
            Dictionary<int, List<Student>> studentDic = new Dictionary<int, List<Student>>
            {
                {9, new List<Student>() },
                {10, new List<Student>() },
                {11, new List<Student>() },
                {12, new List<Student>() }
            };
            foreach (KeyValuePair<int, Student> studentKeyValue in student_directory)
            {
                studentDic[studentKeyValue.Value.Grade].Add(studentKeyValue.Value);
            }
           
            return studentDic;
        }

        /// <summary>
        /// Returns the dictionary of grades containing all in students.
        /// </summary>
        /// <returns>A dictionary containing the student objects.
        /// Format: {key: grade, value: list of students in that grade}</returns>
        public Dictionary<int, List<Student>> GetStudentsByGrade()
        {
            Dictionary<int, List<Student>> studentDic = new Dictionary<int, List<Student>>
            {
                {9, new List<Student>() },
                {10, new List<Student>() },
                {11, new List<Student>() },
                {12, new List<Student>() }
            };
            foreach (KeyValuePair<int, Student> studentKeyValue in student_directory)
            {
                Student student = studentKeyValue.Value;
                if (student.In) studentDic[student.Grade].Add(student);

            }

            return studentDic;
        }

        /// <summary> 
        /// Adds students to the master student dictionary
        /// </summary>
        /// <param name="students">The list of students to add.</param>
        public void AddStudents(List<Student> students)
        {
            // So that we can rollback any changes.
            Dictionary<int, Student> safeStudent_directory = new Dictionary<int, Student>(student_directory);
            int id = 0;
            try
            {
                // Add the students to the student list
                foreach (Student student in students)
                {
                    id = student.Id;
                    safeStudent_directory.Add(student.Id, student);
                }
            }
            catch (System.ArgumentException)
            {
                System.Windows.Forms.MessageBox.Show("A student with ID " + id.ToString() + 
                    " already exists, or two students with that id were just imported.",
                    "Duplicate Student ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ReplaceOldStuDicWithNewOne(safeStudent_directory);
            Save();
        }

        /// <summary>
        /// Used just after creating a backup of the student dictionary to push the
        /// backup onto the master.
        /// </summary>
        /// <param name="newStudentDic">The backup that was made.</param>
        private void ReplaceOldStuDicWithNewOne(Dictionary<int, Student> newStudentDic)
        {
            data[StudentKey] = new Dictionary<int, Student>(newStudentDic);
            student_directory = (Dictionary < int, Student > )data[StudentKey];
        }

        /// <summary>
        /// Function to add created matches and save them.
        /// </summary>
        /// <param name="matchesToAdd">List of matches to add to the match dictionary</param>
        public void AddMatches(List<Match> matchesToAdd)
        {
            // Update the match numbers.
            SetCurrMatchNo(matchesToAdd[matchesToAdd.Count() - 1].MatchNumber);

            foreach (Match match in matchesToAdd)
            {
                // Add the matches onto the match dictionary
                match_directory.Add(match.MatchId, match);

                // Update the first student object
                student_directory[match.Id1].CurrMatchID = match.MatchId;            

                // Because there will be no student with id = 0, this is the id of a "pass" student
                if (match.Id2 != 0)
                {
                    student_directory[match.Id2].CurrMatchID = match.MatchId;
                }
            }
            Save();
            MatchChangeEvent(this, new EventArgs());
        }

        /// <summary>
        /// Saves all students added and settings changed.
        /// </summary>
        public void Save()
        {
            dataFile.Write(data);
        }

        /// <summary>
        /// Returns a boolean declaring whether or not a given match is open
        /// </summary>
        /// <param name="match">The match object that you want to verify is open.</param>
        /// <returns>Boolean, true when match is open, false otherwise.</returns>
        public bool MatchIsOpen(Match match)
        {
            return !match.Closed;
        }

        /// <summary>
        /// Determines if we're ready to go to the next round
        /// </summary>
        public bool IsReadyForNextRound()
        {
            if (GetOpenMatchesList().Count() == 0) return true;
            else return false;
        }

        /// <summary>
        /// Used to set up the file for first-time use. 
        /// </summary>
        protected void FirstTimeSetup()
        {

            // Make the lists of students TODO make this into a smarter loop of somekind
            List<Student> gr_9s = new List<Student>();
            List<Student> gr_10s = new List<Student>();
            List<Student> gr_11s = new List<Student>();
            List<Student> gr_12s = new List<Student>();

            // Create the grades dictionary
            Dictionary<int, Student> student_dic = new Dictionary<int, Student>();

            // Create new matches list
            Dictionary<string, Match> matches = new Dictionary<string, Match>();

            // Create the hasthtable that will store various other values
            Hashtable various_data = new Hashtable
            {
                // The current match number. Used for creating unique match ids
                {"TopMatch", 0 },

                // The current round number. Starts at 0 to indicate no round is in progress.
                {"RoundNo", 0 }
            };

            // Create the data dictionary
            Hashtable data = new Hashtable
            {
                { "students", student_dic },
                { "matches", matches },
                {"misc", various_data }
            };

            dataFile.Write(data);
        }
    }
}
