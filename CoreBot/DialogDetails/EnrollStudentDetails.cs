namespace CoreBot.DialogDetails
{
    using System.Collections.Generic;

    public class EnrollStudentDetails
    {
        public int StudentID { get; set; } // Optional, used if tracking student IDs
        public string FirstName { get; set; } // User's first name
        public string LastName { get; set; } // User's last name
        public string StudentMail { get; set; } // User's email address
        public List<string> CourseTitles { get; set; } = new List<string>(); // List of course titles for enrollment
    }
}
