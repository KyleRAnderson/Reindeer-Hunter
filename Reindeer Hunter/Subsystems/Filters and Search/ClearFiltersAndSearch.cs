using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Reindeer_Hunter.Subsystems;

namespace Reindeer_Hunter.Subsystems.SearchAndFilters
{
    public class ClearFiltersAndSearch : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private FiltersAndSearch Filter_Subsystem;

        public ClearFiltersAndSearch(FiltersAndSearch subsystem)
        {
            Filter_Subsystem = subsystem;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Filter_Subsystem.ResetFilters();
        }
    }
}
