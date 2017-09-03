using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reindeer_Hunter
{
    /// <summary>
    /// This class is responsible for generating the matches. 
    /// </summary>
    public class Matcher
    {
        protected static Dictionary<int, List<Student>> students_dic;
        protected static long round;
        protected static long topMatchNo;
        protected static int numMatchesToCreate = 0;
        protected static int numMatchesCreated = 0;
        protected static Random rndm;
        protected static List<Student> students_list;

        //  To keep track of how long the thread took.
        protected static Stopwatch stopwatch;
        public static Queue<Message> comms;

        /* OPERATIONS
         * Constant integers for the type of 
         * operation that is going on currently
         */
        // For when everything is being prepared.
        protected readonly int SETUP = 0;
        // For while the matches are being created
        protected readonly int CREATING_MATCHES = 1;
        // For anything after the matches have been created.
        protected readonly int COMPLETED = 2;

        /// <summary>
        /// Generates the matches composing of two students against each other.
        /// </summary>
        /// <param name="roundNo">The current round number</param>
        /// <param name="maxMatchNo">The highest match id number currently</param>
        /// <param name="messenger">The queue object to communicate with</param>
        /// <param name="studentsDic">A dictionary of students sorted into their grades.
        /// Provide this if you wish to create matches between students in the same grade, else
        /// leave it null and provide studentList instead.</param>
        /// <param name="studentList">A list of students in this round. Provide this when 
        /// creating matches between students of possibly different grades.</param>
        public Matcher(long roundNo, long maxMatchNo, Queue<Message> messenger,
            Dictionary<int, List<Student>> studentsDic = null, List<Student> studentList = null)
        {
            students_dic = studentsDic;
            round = roundNo;
            topMatchNo = maxMatchNo;
            comms = messenger;
            students_list = studentList;

            if (students_dic == null && students_list == null)
            {
                throw new Exception("Cannot have two null arguments for Matcher.");
            }

        }

        /// <summary>
        /// Simple function to send string updates to the gui. 
        /// Uses information on the current operation taking place,
        /// and if the current operation is the creating matches
        /// operation, will send a percentage of how many
        /// are complete.
        /// </summary>
        /// <param name="matchesDone"> The number of matches processed
        /// Should be provided during the CREATING MATCHES operation</param>
        /// <param name="matchesToProcess"> The number of matches to process.</param>
        /// <param name="operation"> The integer operation that is under way.
        /// See "OPERATIONS"</param>
        /// <param name="matchList"> The list of matches to be sent back to the main thread.</param>
        /// <param name="timeStarted"> The time in miliseconds when this process started.</param>
        private void SendUpdateMessage(int operation, int matchesDone = -1, int matchesToProcess = -1,
            List<Match> matchList = null)
        {
            // The update message to be sent and the percent.
            string updateMessage = "";
            double decimalPercent = 0;

            if (operation == SETUP) updateMessage += "Pre-operations setup.";
            else if (operation == CREATING_MATCHES)
            {
                // Make sure the coder doesn't forget to update the progress once the status
                // is creating matches.
                if (matchesDone < 0 || matchesToProcess < 0)
                    throw new System.ArgumentException(
                        "Forgot to supply matchesDone or matchesToProcess");

                // TODO do the math in a better way
                // Calculate how far we've gone.
                decimalPercent = 100 * (matchesDone / matchesToProcess);
                int percent = (int)Math.Round(decimalPercent);

                // Create the actual string message to be sent
                updateMessage += "Creating matches. " + matchesDone.ToString() +
                    "/" + matchesToProcess.ToString() +
                    " (" + percent.ToString() + "% complete).";
            }

            else
            {
                // Get the time and convert to seconds. Stop the stopwatch
                double timePassed = stopwatch.ElapsedMilliseconds;
                timePassed = timePassed / 1000;
                stopwatch.Stop();

                decimalPercent = 1.0;
                updateMessage = "Complete " + timePassed.ToString() + " seconds.";

            }

            Message message = new Message(updateMessage, decimalPercent, matchList);
            comms.Enqueue(message);
        }

        /// <summary>
        /// The actual function that generates the matches based on the students inputted.
        /// This should be run as a background process as it will take some time. 
        /// </summary>
        /// <param name="students">The dictionary of all the school's students to generate
        /// matches for. </param>
        /// <param name="round"> The round number that we're at. </param>
        public void Generate()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            SendUpdateMessage(SETUP);

            // Create a new list for the matches created
            List<Match> matchesCreated = new List<Match>();

            // Make the random class
            rndm = new Random();

            if (students_dic != null)
            {
                // Determine how many matches we will create
                for (int b = 9; b < 13; b++)
                {
                    // TODO better way plz.
                    double temp = students_dic[b].Count() / 2;
                    numMatchesToCreate += (int)Math.Round(temp);
                }

                // Do this for grades 9-12
                for (int a = 9; a < 13; a++)
                {

                    // Duplicate the student lists, but don't clone the objects.
                    matchesCreated.AddRange(MatchCreator(students_dic[a]));

                }
            }

            else
            {
                // TODO better way
                double temp = students_list.Count() / 2;
                numMatchesToCreate = (int)Math.Round(temp);

                matchesCreated.AddRange(MatchCreator(students_list));
            }


            SendUpdateMessage(operation: COMPLETED, matchList: matchesCreated);
        }

        /// <summary>
        /// Used by Generate() function to actually create the matches
        /// </summary>
        /// <param name="">The list of students to make matches with</param>
        /// <returns>Returns a list of created matches.</returns>
        private List<Match> MatchCreator(List<Student> students)
        {
            List<Match> matchesCreated = new List<Match>();
            List<Student> student_list = new List<Student>(students);

            // Handling for odd numbers of students
            if (student_list.Count() % 2 > 0)
            {
                // Lucky because he gets passed with no effort.
                Student lucky_guy = student_list[rndm.Next(0, student_list.Count())];
                student_list.Remove(lucky_guy);

                // Make the lucky guy a pass match
                Match passmatch = new Match
                {
                    First1 = lucky_guy.First,
                    Last1 = lucky_guy.Last,
                    Id1 = lucky_guy.Id,
                    First2 = "Pass",
                    Last2 = "Pass",
                    Id2 = 0,
                    Round = round,
                    Closed = true
                };

                topMatchNo += 1;

                // Pass the lucky student
                passmatch.Pass1 = true;

                // Generate match id
                passmatch.GenerateID(topMatchNo);

                matchesCreated.Add(passmatch);
            }
            // Begin the selection 
            while (student_list.Count() > 0)
            {
                SendUpdateMessage(CREATING_MATCHES, matchesToProcess: numMatchesToCreate, matchesDone: numMatchesCreated);

                Student student1 = student_list[rndm.Next(0, student_list.Count())];
                Student student2 = student_list[rndm.Next(0, student_list.Count())];

                // Make sure we don't pair someone with him/herself.
                while (student2 == student1)
                {
                    student2 = student_list[rndm.Next(0, student_list.Count())];
                }

                // Remove these students from the student list
                student_list.Remove(student1);
                student_list.Remove(student2);

                // Add to the match id number
                topMatchNo += 1;

                Match match = new Match
                {

                    // Set up the properties
                    First1 = student1.First,
                    Last1 = student1.Last,
                    Id1 = student1.Id,
                    First2 = student2.First,
                    Last2 = student2.Last,
                    Id2 = student2.Id,
                    Round = round,
                    Closed = false
                };

                match.GenerateID(topMatchNo);

                // Generate and add the new match to the list
                matchesCreated.Add(match);

                // Add to the number of matches created
                numMatchesCreated += 1;
            }

            return matchesCreated;
        }
    }

    /// <summary>
    /// Class for sending messages across the queue.
    /// </summary>
    public class Message
    {
        public string MessageText { get; }
        public double ProgressDecimal { get; }
        // List of matches created, null when process not complete
        public List<Match> Matches { get; }
        public Message(string text, double progress, List<Match> matchList)
        {
            MessageText = text;
            ProgressDecimal = progress;
            Matches = matchList;
        }
    }
}
