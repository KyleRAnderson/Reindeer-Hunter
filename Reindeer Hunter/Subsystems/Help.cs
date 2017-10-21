using System;
using System.Diagnostics;

namespace Reindeer_Hunter.Subsystems
{
    public class Help : Subsystem
    {
        public RelayCommand OpenUserManual { get; } = new RelayCommand();
        private DataFileIO _DataManager;

        protected override void OnHomePageSet(object sender, EventArgs e)
        {
            base.OnHomePageSet(sender, e);

            OpenUserManual.CanExecuteDeterminer = CanOpenManual;
            OpenUserManual.FunctionToExecute = OpenManual;

            // Set the data manager
            _DataManager = _School.DataFile;

            Refresh();
        }

        private bool CanOpenManual()
        {
            return (_School != null && _DataManager != null && _DataManager.ManualExists);
        }

        private void OpenManual(object parameter)
        {
            Process.Start(_DataManager.ManualLoc);
        }

        /// <summary>
        /// Function that just refreshes all of the CanExecute methods of the commands
        /// this subsystem runs
        /// </summary>
        private void Refresh(object sender = null, EventArgs e = null)
        {
            OpenUserManual.RaiseCanExecuteChanged();
        }
    }
}
