using System.Collections.Generic;

namespace CoreBot.Models
{
    public class Instructor
    {
        public int InstructorID { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public ICollection<Course> CoursesTaught { get; set; } = new List<Course>();
    }
}
