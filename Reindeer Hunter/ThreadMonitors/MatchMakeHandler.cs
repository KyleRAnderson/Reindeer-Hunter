using Reindeer_Hunter.Subsystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Reindeer_Hunter.ThreadMonitors
{
    public class MatchMakeHandler
    {
        /* Used in the lock blocks to make sure that only one thread accesses
         * Sensitive stuff at once */
        private readonly object Key = new object();

        private Queue<Message> comms;
        private School school;
        private Thread matchMakeThread;
        private Matcher matcher;
        private ProcessButtonSubsystem subsystem;

        public MatchMakeHandler(School school, ProcessButtonSubsystem subsystemInCharge)
        {
            subsystem = subsystemInCharge;

            // Create the queue for communications purposes.
            comms = new Queue<Message>();

            // Instantiate school object for simplicity
            this.school = school;

            // Create the matchmaker and then assign the thread target to it
            // +1 to current round because we want next round's matches.
            Matcher matcher;
            if (!school.IsCombineTime())
            {
                matcher = new Matcher(school.GetCurrRoundNo() + 1, school.CurrMatchNo, Key,
                    comms, studentsDic: school.GetStudentsByGrade());
            }
            else
            {
                matcher = new Matcher(school.GetCurrRoundNo() + 1, school.CurrMatchNo, Key,
                    comms, studentList: school.GetAllParticipatingStudents());
            }

            matchMakeThread = new Thread(matcher.Generate)
            {
                Name = "Matchmaker"
            };
            matchMakeThread.Start();

            // Put the execute function into the mainloop to be executed
            CompositionTarget.Rendering += MatchmakeMonitor;
        }

        public void MatchmakeMonitor(object sender, EventArgs e)
        {
            // Lock it so we don't get problems.
            lock (Key)
            {
                // Don't do anything if no new data has been sent. 
                if (comms.Count() <= 0) return;

                // Convert queue to list and retrieve last value
                List<Message> returnList = comms.ToList<Message>();
                Message returnValue = returnList[returnList.Count() - 1];

                // Clear queue
                comms.Clear();

                // Update the progress displayed in the GUI
                subsystem.UpdateOperationStatus(returnValue.MessageText, returnValue.ProgressDecimal);

                // Once the matches have been added, let's do stuff.
                if (returnValue.Matches != null)
                {
                    //  Terminate the thread
                    matchMakeThread.Join();

                    subsystem.NewMatches = returnValue.Matches;

                    school.CurrMatchNo = returnValue.Matches[returnValue.Matches.Count -1].MatchNumber;

                    // Unsubscribe from the rendering event
                    CompositionTarget.Rendering -= MatchmakeMonitor;
                }
            }
        }
    }
}
