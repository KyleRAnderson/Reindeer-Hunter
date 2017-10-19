using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Reindeer_Hunter.Subsystems;
using Reindeer_Hunter.ThreadMonitors;

namespace Reindeer_Hunter.Subsystems.ProcessButtonCommands
{
    public class Process : ICommand
    {
        InstantPrintHandler Printer;

        // The subsystem in charge of this command
        public ProcessButtonSubsystem ProcessButtonSubsystem { get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            // We can always execute
            return true;
        }

        /// <summary>
        /// Function that begins either matchmaking or the 
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            int status = int.Parse(parameter.ToString());

            // Determine what action to take using the status.

            // Matchmake
            if (status == ProcessButtonSubsystem.MATCHMAKING)
            {
                // Create matchmaker class.
                MatchMakeHandler matcher = new MatchMakeHandler(
                    ProcessButtonSubsystem.ManagerProperty._School, 
                    ProcessButtonSubsystem);
            }

            // Go to the FFA page
            else if (status == ProcessButtonSubsystem.FFA)
            {
                ProcessButtonSubsystem.GoToFFA();
            }

            // Instant Print
            else
            {
                // If we're currently printing, don't print twice.

                if (Printer != null && Printer.IsPrinting) return;

                 Printer = new InstantPrintHandler(
                    ProcessButtonSubsystem.ManagerProperty._School,
                    ProcessButtonSubsystem);
            }
        }
    }
}
