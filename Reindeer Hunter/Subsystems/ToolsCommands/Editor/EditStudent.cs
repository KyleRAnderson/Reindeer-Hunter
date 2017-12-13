using Reindeer_Hunter.Data_Classes;
using System;

namespace Reindeer_Hunter.Subsystems.ToolsCommands.Editor
{
    public class EditStudent
    {
        /// <summary>
        /// Fired when the move to match button is clicked.
        /// </summary>
        public event EventHandler<EditStudent> MoveToMatchClicked;

        /// <summary>
        /// Fired when the move to passmatch button is clicked.
        /// </summary>
        public event EventHandler<EditStudent> PassStudentButtonClicked;

        public RelayCommand MoveToMatchCommand { get; private set; }

        public RelayCommand PassCommad { get; private set; }

        public Student _Student { get; set; }

        private EventHandler<EditStudent> lastMethodToExecute;
        public EventHandler<EditStudent> MethodToExecute
        {
            set
            {
                if (lastMethodToExecute != null) MoveToMatchClicked -= lastMethodToExecute;
                MoveToMatchClicked += value;
                lastMethodToExecute = value;
            }
        }

        private EventHandler<EditStudent> lastPassMethod;
        public EventHandler<EditStudent> PassMethod
        {
            set
            {
                if (lastPassMethod != null) PassStudentButtonClicked -= lastPassMethod;
                PassStudentButtonClicked += value;
                lastPassMethod = value;
            }
        }

        public EditStudent()
        {
            MoveToMatchCommand = new RelayCommand
            {
                CanExecuteDeterminer = () => true,
                FunctionToExecute = (object parameter) => MoveToMatchClicked?.Invoke(this, this)
            };

            PassCommad = new RelayCommand
            {
                CanExecuteDeterminer = () => true,
                FunctionToExecute = (object parameter) => PassStudentButtonClicked?.Invoke(this, this)
            };

        }
    }
}
