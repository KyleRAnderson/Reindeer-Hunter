using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace Reindeer_Hunter.Data_Classes
{
    /// <summary>
    /// Class that represents each newly imported student.
    /// </summary>
    [DelimitedRecord(",")]
    [IgnoreFirst(1)]
    public class ImportedStudent
    {
        [FieldNotEmpty]
        public string First;
        [FieldNotEmpty]
        public string Last;
        [FieldNotEmpty]
        public int Id;
        [FieldNotEmpty]
        public int Grade;
        [FieldNotEmpty]
        public int Homeroom;
    }
}
