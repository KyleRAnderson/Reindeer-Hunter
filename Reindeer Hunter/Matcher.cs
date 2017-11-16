using Reindeer_Hunter.Data_Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Reindeer_Hunter
{
    /// <summary>
    /// This class is responsible for generating the matches. 
    /// </summary>
    public class Matcher
    {
        private long round;
        private long topMatchNo;
        private int numMatchesToCreate = 0;
        private int numMatchesCreated = 0;
        private Random rndm;
        private List<Student> students_list;
        private Dictionary<int, List<Student>> students_grade_dic;
        private Dictionary<int, Dictionary<int, List<Student>>> grade_homeroom_dic;

        //  To keep track of how long the thread took.
        private Stopwatch stopwatch;
        public Queue<Message> comms;

        /* OPERATIONS
         * Constant integers for the type of 
         * operation that is going on currently
         */
        // For when everything is being prepared.
        private readonly int SETUP = 0;
        // For while the matches are being created
        private readonly int CREATING_MATCHES = 1;
        // For anything after the matches have been created.
        private readonly int COMPLETED = 2;

        private readonly object Key;

        private string EndDate;

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
        public Matcher(long roundNo, long maxMatchNo, object key, string endDate, Queue<Message> messenger,
            Dictionary<int, List<Student>> studentsDic = null, List<Student> studentList = null,
            Dictionary<int, Dictionary<int, List<Student>>> homeroomList = null)
        {
            round = roundNo;
            topMatchNo = maxMatchNo;
            comms = messenger;

            // Set the end date to be sent back after.
            EndDate = endDate;

            Key = key;

            // The student directories
            students_list = studentList;
            grade_homeroom_dic = homeroomList;
            students_grade_dic = studentsDic;

            if (students_grade_dic == null && students_list == null && grade_homeroom_dic == null)
            {
                throw new Exception("Cannot have all keyword arguments null for Matcher.");
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
        private void SendUpdateMessage(int operation, int matchesDone = -1, int matchesToProcess = -1,
            List<Match> matchList = null)
        {
            // The update message to be sent and the percent.
            string updateMessage = "";
            double decimalPercent = 0;
            string endDate = "";

            if (operation == SETUP) updateMessage += "Pre-operations setup.";
            else if (operation == CREATING_MATCHES)
            {
                // Make sure the coder doesn't forget to update the progress once the status
                // is creating matches.
                if (matchesDone < 0 || matchesToProcess < 0)
                    throw new ArgumentException(
                        "Forgot to supply matchesDone or matchesToProcess");

                // Calculate how far we've gone.
                decimalPercent = 100 * (matchesDone / matchesToProcess);
                int percent = (int)Math.Round(decimalPercent);

                // Create the actual string message to be sent
                updateMessage += "Creating matches. " + matchesDone.ToString() +
                    "/" + matchesToProcess.ToString() +
                    " (" + percent.ToString() + "% complete).";
            }

            // If we're done.
            else
            {
                // Get the time and convert to seconds. Stop the stopwatch
                double timePassed = stopwatch.ElapsedMilliseconds;
                timePassed = timePassed / 1000;
                stopwatch.Stop();

                decimalPercent = 1.0;
                updateMessage = "Complete " + timePassed.ToString() + " seconds.";
                endDate = EndDate;

            }

            Message message = new Message(updateMessage, decimalPercent, matchList, endDate);

            // Lock it so that we have synchronized access.
            lock(Key)
            {
                comms.Enqueue(message);
            }
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

            // If the student grade directory isn't null, we're doing it by grade
            if (students_grade_dic != null)
            {
                // Make a list of students by grade, having gotten rid of the key-value pair now.
                List<List<Student>> grades = new List<List<Student>>(students_grade_dic.Values);

                // Determine how many matches we will create
                for (int a = 0; a < grades.Count; a++)
                {
                    numMatchesToCreate += (int)Math.Round(((double)grades[a].Count() / 2));
                }

                // Do this for all grades in the grades dictionary
                for (int a = 0; a < grades.Count; a++)
                {

                    // Create and add the matches for this grade.
                    matchesCreated.AddRange(MatchCreator(grades[a]));

                }
            }





            // Or, we're mixing people from different grades
            else if (students_list != null)
            {
                // Figure out how many matches we're making
                numMatchesToCreate = (int)Math.Round((double)(students_list.Count() / 2));

                // Create and add the matches.
                matchesCreated.AddRange(MatchCreator(students_list));
            }




            // Otherwise, we're mixing students between homerooms
            else
            {

                /* Get the list such that it will be [{904: (students in 904), 905: (students in 905)}, {1004: (students in 1004)}]. 
                 * Each grade has a different place in the list, and that place in the list will contain a dictionary
                 * of the homerooms in that grade.
                 */
                List<Dictionary<int, List<Student>>> list_of_grade_homeroom = new List<Dictionary<int, List<Student>>>(grade_homeroom_dic.Values);
                // Determine how many matches are going to be made
                foreach (Dictionary<int, List<Student>> grade_homeroom_dic in list_of_grade_homeroom)
                {
                    foreach (List<Student> studentList in grade_homeroom_dic.Values)
                    {
                        numMatchesToCreate += studentList.Count;
                    }
                }
                
                foreach (Dictionary<int, List<Student>> grade in list_of_grade_homeroom)
                {
                    // Make the matches and add them to the list of matches
                    matchesCreated.AddRange(MakeMatchesByHomeroom(grade));
                }
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
                // Add to the match count
                topMatchNo += 1;

                Student lucky_guy;
                do
                {
                    // Lucky because he gets passed with no effort.
                    lucky_guy = student_list[rndm.Next(0, student_list.Count())];
                }
                // If the student has been passed already, don't pass them again.
                while (lucky_guy.HasBeenPassed);

                student_list.Remove(lucky_guy);

                // Pass the student and add the passmatch to the match list
                matchesCreated.Add(PassStudent(lucky_guy, topMatchNo));
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

                // Generate and add the new match to the list
                matchesCreated.Add(GenerateMatch(student1, student2, topMatchNo));

                // Add to the number of matches created
                numMatchesCreated += 1;
            }

            return matchesCreated;
        }

        /// <summary>
        /// Method to generate matches between students of two homerooms.
        /// </summary>
        /// <param name="homeroom1">The list of students from the first homeroom</param>
        /// <param name="homeroom2">The list of students from the second homeroom</param>
        /// <returns></returns>
        private List<Match> MakeMatchesByHomeroom(Dictionary<int, List<Student>> homeroomDic)
        {
            /* 
             * Since it is possible for there to be less students in one homeroom over the other,
             * make a list and keep these students there to be matched between each other at the end.
             */
            List<Student> leftOverStudents = new List<Student>();

            // Make the homeroom dictionary into one giant list.
            List<List<Student>> homeroomList = new List<List<Student>>(homeroomDic.Values);

            // The list for the matches that are made.
            List<Match> matchesMade = new List<Match>();

            // When there is only one homeroom left, special stuff needs to happen.
            while (homeroomList.Count > 1)
            {
                // Choose the first homeroom
                List<Student> homeroom1 = homeroomList[rndm.Next(0, homeroomList.Count)];
                homeroomList.Remove(homeroom1);

                // Choose the second homeroom
                List<Student> homeroom2 = homeroomList[rndm.Next(0, homeroomList.Count)];
                homeroomList.Remove(homeroom2);

                // Loop around, matching all students in the two homerooms
                do
                {
                    SendUpdateMessage(CREATING_MATCHES, matchesDone: numMatchesCreated, matchesToProcess: numMatchesToCreate);

                    // Get student 1
                    Student student1 = homeroom1[rndm.Next(0, homeroom1.Count)];
                    homeroom1.Remove(student1);


                    // Get student 2
                    Student student2 = homeroom2[rndm.Next(0, homeroom2.Count)];
                    homeroom2.Remove(student2);

                    // Add to topmatch number
                    topMatchNo += 1;
                    // Generate the match and add it to the list
                    matchesMade.Add(GenerateMatch(student1, student2, topMatchNo));
                    // Increase our count of number of matches made.
                    numMatchesCreated += 1;

                    // Once one of the homerooms is out of students, add to the 
                    if (homeroom1.Count == 0 || homeroom2.Count == 0)
                    {
                        // Add the leftover students, and move on to the next homeroom.
                        leftOverStudents.AddRange(homeroom1);
                        leftOverStudents.AddRange(homeroom2);
                    }
                }
                while (homeroom1.Count > 0 && homeroom2.Count > 0);
            }

            /* 
             * Now deal with the leftover homeroom and leftover students
             */

            List<Student> leftoverHomeroom;
            if (homeroomList.Count > 0) leftoverHomeroom = homeroomList[0];
            else leftoverHomeroom = new List<Student>();

            if (leftoverHomeroom.Count > 0 && leftOverStudents.Count > 0)
            {
                while (leftoverHomeroom.Count > 0 && leftOverStudents.Count > 0)
                {
                    // Get two students
                    Student student1 = leftoverHomeroom[rndm.Next(0, leftoverHomeroom.Count)];
                    leftoverHomeroom.Remove(student1);
                    Student student2 = leftOverStudents[rndm.Next(0, leftOverStudents.Count)];
                    leftOverStudents.Remove(student2);

                    // Increase the top match number
                    topMatchNo += 1;

                    // Generate and add the match
                    matchesMade.Add(GenerateMatch(student1, student2, topMatchNo));
                }
            }

            // If there are still people left in the homeroom list
            if (leftoverHomeroom.Count > 0 && leftOverStudents.Count == 0)
            {
                // If there's an odd student, pass them.
                if (leftoverHomeroom.Count % 2 != 0)
                {
                    // Choose a lucky student to pass
                    Student lucky_guy = leftoverHomeroom[rndm.Next(0, homeroomList.Count)];

                    // Remove that student
                    leftoverHomeroom.Remove(lucky_guy);

                    topMatchNo += 1;

                    // Make and add the passmatch
                    matchesMade.Add(PassStudent(lucky_guy, topMatchNo));
                }

                while (leftoverHomeroom.Count > 0 )
                {
                    // Get two students
                    Student student1 = leftoverHomeroom[rndm.Next(0, leftoverHomeroom.Count)];
                    leftoverHomeroom.Remove(student1);
                    Student student2 = leftoverHomeroom[rndm.Next(0, leftoverHomeroom.Count)];
                    leftoverHomeroom.Remove(student2);

                    // Increase the top match number
                    topMatchNo += 1;

                    // Generate and add the match
                    matchesMade.Add(GenerateMatch(student1, student2, topMatchNo));
                }
            }

            if (leftoverHomeroom.Count == 0 && leftOverStudents.Count > 0)
            {
                // If there's an odd student, pass them.
                if (leftOverStudents.Count % 2 != 0)
                {
                    // Choose a lucky student to pass
                    Student lucky_guy = leftOverStudents[rndm.Next(0, leftOverStudents.Count)];

                    // Remove that student
                    leftOverStudents.Remove(lucky_guy);

                    topMatchNo += 1;

                    // Make and add the passmatch
                    matchesMade.Add(PassStudent(lucky_guy, topMatchNo));
                }

                while (leftOverStudents.Count > 0 )
                {
                    // Get two students
                    Student student1 = leftOverStudents[rndm.Next(0, leftOverStudents.Count)];
                    leftOverStudents.Remove(student1);
                    Student student2 = leftOverStudents[rndm.Next(0, leftOverStudents.Count)];
                    leftOverStudents.Remove(student2);

                    // Increase the top match number
                    topMatchNo += 1;

                    // Generate and add the match
                    matchesMade.Add(GenerateMatch(student1, student2, topMatchNo));
                }
            }

            return matchesMade;
        }

        /// <summary>
        /// Method to quickly generate a passmatch for the given student with the given
        /// id number
        /// </summary>
        /// <param name="lucky_guy">The student to make a passmatch for.</param>
        /// <param name="topMatchNo">The match number to make the id with</param>
        /// <returns></returns>
        private Match PassStudent(Student lucky_guy, long topMatchNo) {
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
                Closed = true,
                Home1 = lucky_guy.Homeroom,
                Home2 = 0,

                // Pass the lucky student
                Pass1 = true
            };

            // Generate match id
            passmatch.GenerateID(topMatchNo);

            return passmatch;
        }

        /// <summary>
        /// Method to generate a match between the two given students
        /// </summary>
        /// <param name="student1">The first student in the match</param>
        /// <param name="student2">The second student in the match</param>
        /// <param name="topMatchNo">The highest match number, used for creating the match id.</param>
        /// <returns></returns>
        private Match GenerateMatch(Student student1, Student student2, long topMatchNo)
        {
            Match match = new Match
            {

                // Set up the properties
                First1 = student1.First,
                Last1 = student1.Last,
                Id1 = student1.Id,
                Home1 = student1.Homeroom,
                First2 = student2.First,
                Last2 = student2.Last,
                Id2 = student2.Id,
                Home2 = student2.Homeroom,
                Round = round,
                Closed = false
            };

            match.GenerateID(topMatchNo);

            return match;
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
        public Message(string text, double progress, List<Match> matchList, string endDate)
        {
            MessageText = text;
            ProgressDecimal = progress;
            Matches = matchList;
            EndDate = endDate;
        }

        public string EndDate { get; } = "";
    }
}
