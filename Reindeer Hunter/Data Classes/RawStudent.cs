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
    public class RawStudent
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

        /// <summary>
        /// Constructs a rawstudent from a fully-fledged student object
        /// </summary>
        /// <param name="student">The student to convert into a raw student</param>
        public static RawStudent CreateFromStudent(Student student)
        {
            return new RawStudent
            {
                First = student.First,
                Last = student.Last,
                Id = student.Id,
                Grade = student.Grade,
                Homeroom = student.Homeroom
            };
        }
    }
}
