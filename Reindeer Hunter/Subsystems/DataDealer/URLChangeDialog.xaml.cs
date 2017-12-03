using System.Linq;
using System.Text;
using System.Windows;

namespace Reindeer_Hunter.Subsystems.ProcessButtonCommands
{
    /// <summary>
    /// Interaction logic for MatchmakeDialog.xaml
    /// </summary>
    public partial class URLChangeDialog : Window
    {
        /// <summary>
        /// True when the submit button is pressed, false otherwise.
        /// </summary>
        bool Submitted = false;

        public RelayCommand OkCommand { get; } = new RelayCommand();
        public RelayCommand CancelCommand { get; } = new RelayCommand
        {
            CanExecuteDeterminer = () => true
        };

        /// <summary>
        /// The new url, as inputted by the user.
        /// </summary>
        public string URL
        {
            get
            {
                // If the dialog wasn't submitted, return nothing
                if (!Submitted) return "";

                // Otherwise return the properly formatted url.
                else return ProcessURL(URLBox.Text);
            }
        }

        /// <summary>
        /// Convert the raw URL into something the program can use.
        /// </summary>
        /// <param name="url">The raw url/</param>
        /// <returns>The usable string url.</returns>
        private string ProcessURL(string url)
        {
            StringBuilder masterString = new StringBuilder(url);

            int relevantIndexStart = url.IndexOf('=', url.IndexOf('=') + 1);
            int lengthOfRelevance = url.Length - relevantIndexStart;

            // Remove the now irrelevant part of the URL
            masterString.Remove(relevantIndexStart, lengthOfRelevance);

            // The part of the URL we want to work with.
            string relevantURLPart = url.Substring(relevantIndexStart);

            StringBuilder relevantBuilder = new StringBuilder(relevantURLPart);

            // Since there should be 4 fields, loop 4 times.
            // stringFormatIndex is the index to insert in place of the specific stuff.
            for (int stringFormatIndex = 0; stringFormatIndex < 4; stringFormatIndex++)
            {
                string workable = relevantBuilder.ToString();

                // Skip the first equals sign. It's not what we want.
                int startIndex = workable.IndexOf("=") + 1;

                // Skip the first amperand. It's also not what we want. +1 because we want to include the "&"
                int endIndex = workable.IndexOf("&");
                // At the end of the URL, there won't be any "&". Account for this.
                if (endIndex == -1) endIndex = workable.Length;

                // Fix up the string, to have the proper parts. 
                relevantBuilder.Remove(startIndex, endIndex - startIndex);
                // TODO better
                relevantBuilder.Insert(startIndex, string.Format("{0}{1}{2}", "{", stringFormatIndex, "}"));

                // Update the workable
                workable = relevantBuilder.ToString();

                // Update start and end indices
                endIndex = workable.IndexOf('&') + 1;
                // At the end of the URL, there won't be any "&". Account for this.
                if (endIndex == 0) endIndex = workable.Length;

                // Add the processed URL portion to the master string.
                masterString.Insert(masterString.ToString().Length, workable.Substring(0, endIndex));

                // Remove the right part of the string, so that next time it won't be there.
                relevantBuilder.Remove(0, endIndex);
            }

            // Replace it with the constructed portion.

            return masterString.ToString();
        }

        public URLChangeDialog()
        {
            InitializeComponent();

            OkCommand.CanExecuteDeterminer = CanOkCancel;

            OkCommand.FunctionToExecute = OkFunc;
            CancelCommand.FunctionToExecute = CancelFunc;
        }

        private bool CanOkCancel()
        {
            // Need to have a valid url to be able to subit.
            return URLBox.Text.Length > 0 && URLBox.Text.Count(character => character == '=') == 5;
        }

        // Just close the window. Status should take care of itself
        private void OkFunc(object parameter)
        {
            Submitted = true;
            Close();
        }

        // Just close the window.
        private void CancelFunc(object parameter)
        {
            Submitted = false;
            Close();
        }

        /// <summary>
        /// Just used to raise the can execute changed event on 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RaiseCanExecuteChanged(object sender, RoutedEventArgs e)
        {
            OkCommand.RaiseCanExecuteChanged();
        }
    }
}
