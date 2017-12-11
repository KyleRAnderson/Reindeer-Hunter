using Reindeer_Hunter.Data_Classes;
using System;

namespace Reindeer_Hunter.Subsystems.ToolsCommands.Editor
{
    public class EditStudent
    {
        public event EventHandler<EditStudent> ButtonClicked;

        public RelayCommand ButtonCommand { get; private set; }

        public Student _Student { get; set; }

        private EventHandler<EditStudent> lastMethodToExecute;
        public EventHandler<EditStudent> MethodToExecute
        {
            set
            {
                if (lastMethodToExecute != null) ButtonClicked -= lastMethodToExecute;
                ButtonClicked += value;
                lastMethodToExecute = value;
            }
        }

        public EditStudent()
        {
            ButtonCommand = new RelayCommand
            {
                CanExecuteDeterminer = () => true,
                FunctionToExecute = RaiseButtonClicked
            };
        }

        private void RaiseButtonClicked(object parameter)
        {
            ButtonClicked?.Invoke(this, this);
        }
    }
}
