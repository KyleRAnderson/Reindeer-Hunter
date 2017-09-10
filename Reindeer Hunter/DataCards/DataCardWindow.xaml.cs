using Reindeer_Hunter.Data_Classes;
using Reindeer_Hunter.DataCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Reindeer_Hunter
{
    /// <summary>
    /// Interaction logic for StudentDataCard.xaml
    /// </summary>
    public partial class DataCardWindow : Window
    {
        private School _School;
        private MatchCard _MatchCard;
        private StudentCard _StudentCard;

        private MatchCard MatchPage
        {
            get
            {
                if (_MatchCard == null) _MatchCard = new MatchCard(this);
                return _MatchCard;
            }
        }
        private StudentCard StudentPage
        {
            get
            {
                if (_StudentCard == null) _StudentCard = new StudentCard(this);
                return _StudentCard;
            }
        }

        /// <summary>
        /// The Window for showing student or match data cards on
        /// </summary>
        /// <param name="school">The school object to be worked with</param>
        /// <param name="studentId">The ID of the student we're displaying, else if it's a match leave blank</param>
        /// <param name="matchId">The match id of the match we're displaying, else if it's a student leave blank.</param>
        public DataCardWindow(School school, int studentId = 0, string matchId = "")
        {
            InitializeComponent();

            // Save the scool object to our own variable
            _School = school;

            Display(studentId, matchId);
        }

        public void Display(int studentId = 0, string matchId = "")
        { 
            if (studentId != 0)
            {
                SetPage(StudentPage);
                SetStudentContent(_School.GetStudent(studentId));
            }

            else
            {
                SetPage(MatchPage);
                SetMatchContent(_School.GetMatch(matchId));
            }
        }

        public void SetPage(UserControl page)
        {
            Content = page;
        }

        /// <summary>
        /// Sets up the match page with the given match data
        /// </summary>
        /// <param name="match">The match data object to set things up with.</param>
        private void SetMatchContent(Match match)
        {
            // Student 1
            MatchPage.Id1 = match.Id1;
            MatchPage.First1 = match.First1;
            MatchPage.Last1 = match.Last1;
            MatchPage.Home1 = match.Home1;
            MatchPage.Pass1 = match.Pass1;

            // Match details
            MatchPage.Round = match.Round;
            MatchPage.MatchId = match.MatchId;
            MatchPage.Closed = match.Closed;

            // Student 2
            MatchPage.Id2 = match.Id2;
            MatchPage.First2 = match.First2;
            MatchPage.Last2 = match.Last2;
            MatchPage.Home2 = match.Home2;
            MatchPage.Pass2 = match.Pass2;

            // The current round data
            MatchPage.CurrentRound = _School.GetCurrRoundNo();
        }

        /// <summary>
        /// Sets up the student page with the given student data
        /// </summary>
        /// <param name="student">The student data object to set things up with.</param>
        private void SetStudentContent(Student student)
        {
            StudentPage.StudentId = student.Id;
            StudentPage.LastRound = student.LastRoundParticipated;
            StudentPage.Homeroom = student.Homeroom;
            StudentPage.Grade = student.Grade;
            StudentPage.First = student.First;
            StudentPage.Last = student.Last;
            StudentPage.In = student.In;
            StudentPage.CurrentMatch = student.CurrMatchID;
            StudentPage.ParticipatedMatches = student.MatchesParticipated;
        }

        /// <summary>
        /// Function for reopening the given match
        /// </summary> 
        /// <param name="MatchId">The match to reopen.</param>
        public void ReopenMatch(string MatchId)
        {
            // Make school do the change
            _School.ReopenMatch(MatchId);

            // Reload the match data card
            Display(matchId: MatchId);
        }
    }
}
