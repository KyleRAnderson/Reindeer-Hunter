using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Reindeer_Hunter.Subsystems
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Action<object> FunctionToExecute { get; set; }
        public Func<bool> CanExecuteDeterminer { get; set; }

        public bool CanExecute(object parameter)
        {
            if (CanExecuteDeterminer == null) return false;
            else return CanExecuteDeterminer.Invoke();
        }

        public void Execute(object parameter)
        {
            if (FunctionToExecute == null) return;
            FunctionToExecute(parameter);
        }

        /// <summary>
        /// Simple function to raise canExecute changed.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, new EventArgs());
        }
    }
}