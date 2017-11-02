using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Security.Cryptography;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace Reindeer_Hunter.Subsystems
{
    public class DataDealer : Subsystem
    {
        // The import and export command that this subsystem takes care of. Also, the erase command.
        public RelayCommand Import { get; private set; }
        public RelayCommand Export { get; private set; }
        public RelayCommand Erase { get; private set; }

        public DataDealer() : base()
        {
            Import = new RelayCommand
            {
                FunctionToExecute = Importfunc,
                CanExecuteDeterminer = () => true
            };

            Export = new RelayCommand
            {
                FunctionToExecute = Exportfunc,
                CanExecuteDeterminer = () => true
            };

            Erase = new RelayCommand
            {
                FunctionToExecute = EraseFunc,
                CanExecuteDeterminer = () => true
            };
        }

        /// <summary>
        /// The import function
        /// </summary>
        /// <param name="parameter"></param>
        public void Importfunc(object parameter)
        {
            // Make sure the user is informed about what's about to happen
            MessageBoxResult result = MessageBox.Show("WARNING - This will overwrite all current information being stored. " +
                "If you want to save the current information but would like to view the imported information, " +
                "export it first and the import. \nProceed?", "WARNING", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            // If they wish to abort, abort them.
            if (result != MessageBoxResult.OK) return;

            OpenFileDialog askLoc = new OpenFileDialog
            {
                // Open the file dialog to the user's directory
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),

                // Filter only for comma-seperated value files. 
                Filter = "json files (*.json)|*.json",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            askLoc.ShowDialog();

            string openLoc = askLoc.FileName;

            // In case the user cancels
            if (openLoc == null || openLoc == "") return;

            Manager._School.DataFile.Import(openLoc);
        }

        /// <summary>
        /// The export function.
        /// </summary>
        /// <param name="parameter"></param>
        public void Exportfunc(object parameter)
        {
            Manager._School.DataFile.Export();
        }

        /// <summary>
        /// The erase function.
        /// </summary>
        /// <param name="parameter"></param>
        public void EraseFunc(object parameter)
        {
            // Warn the user, ask them for permission to continue
            MessageBoxResult result = MessageBox.Show("WARNING - This will overwrite all current information being stored. " +
               "If you want to save the current information but would like to view the imported information, " +
               "export it first and the import. \nProceed?", "WARNING", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (result != MessageBoxResult.OK) return;

            Manager._School.DataFile.EraseData();

            // Once data is erased, prompt user to restart or to quit
            result = MessageBox.Show("Data Erased. Restart?", "Restart?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes) Manager._School.DataFile.RestartApplication();
            else Manager._School.DataFile.QuitApplication();
        }
    }
}
