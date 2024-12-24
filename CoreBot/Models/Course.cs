using System.Collections.Generic;

namespace CoreBot.Models
{
    public class Course
    {
        public int CourseID { get; set; }
        public string Title { get; set; }
        public int InstructorID { get; set; }
        public string Category { get; set; }
        public int Capacity { get; set; }
        public int RegisteredStudents { get; set; }
        public ICollection<Student> EnrolledStudents { get; set; } = new List<Student>();
        public ICollection<Instructor> Instructors { get; set; } = new List<Instructor>();
    }
}
