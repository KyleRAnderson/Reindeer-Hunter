using Reindeer_Hunter.Data_Classes;
using System.Windows.Controls;
using System.Windows.Media;
using System;
using System.Windows;

namespace Reindeer_Hunter.Subsystems.Passer
{
    public class MatchButton
    {
        public Button _Button { get; private set; }
        public Match _Match { get; private set; }
        public event EventHandler<MatchButton> ButtonClicked;

        public string MatchId
        {
            get
            {
                return _Match.MatchId;
            }
        }

        public MatchButton(Match match = null)
        {
            _Match = match;

            _Button = new Button
            {
                Content = MatchId,
                Width = double.NaN, // Not a number for auto width
                Height = 20,
                FontSize = 10,
                Background = Brushes.Black,
                Foreground = Brushes.White
            };

            _Button.Click += ClickHandler;
        }

        private void ClickHandler(object sender, RoutedEventArgs e)
        {
            ButtonClicked?.Invoke(this, this);
        }

        public void Refresh()
        {
            // If button object is null, do nothing
            if (_Button == null) return;
            _Button.Content = MatchId;
        }

        /// <summary>
        /// Converts the MatchButton object to a Match object
        /// </summary>
        /// <returns>The equivalent Match object.</returns>
        public Match ToMatch()
        {
            return _Match;
        }
    }
}
