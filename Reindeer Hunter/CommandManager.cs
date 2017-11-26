using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Reindeer_Hunter.Subsystems;

namespace Reindeer_Hunter
{
    public class CommandManager
    {
        // Necessary for some operations
        public School _School { get; set; }

        // The command subsystems.
        public FiltersAndSearch _FiltersAndSearch { get; }
        public ProcessButtonSubsystem _ProcessButtonSubsystem { get; }
        public SaveDiscardButtonSubsystem _SaveDiscard { get; }
        public PasserSubsystem _Passer { get; }
        public DataDealer _ImportExport { get; }
        public Statistics _Stats { get; }
        public Manager_Subsystem _Importer { get; }
        public Help _Help { get; }
        
        // The homepage object.
        public HomePage Home { get; private set; }

        // Event triggered when the HomePage object is received.
        public event EventHandler ParentPageSet;

        public CommandManager()
        {
            _FiltersAndSearch = new FiltersAndSearch
            {
                ManagerProperty = this
            };

            _ProcessButtonSubsystem = new ProcessButtonSubsystem
            {
                ManagerProperty = this
            };

            _SaveDiscard = new SaveDiscardButtonSubsystem
            {
                ManagerProperty = this
            };

            _Passer = new PasserSubsystem
            {
                ManagerProperty = this
            };
            _ImportExport = new DataDealer
            {
                ManagerProperty = this
            };
            _Stats = new Statistics
            {
                ManagerProperty = this
            };
            _Importer = new Manager_Subsystem
            {
                ManagerProperty = this
            };
            _Help = new Help
            {
                ManagerProperty = this
            };
        }

        public void SetHomePage(HomePage home)
        {
            Home = home;
            _School = Home.MasterWindow._School;
            ParentPageSet(this, new EventArgs());
        }
    }
}
