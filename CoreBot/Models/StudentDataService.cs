using System.Collections.Generic;
using System.Threading.Tasks;
using CoreBot.Models;
using CoreBot.Services;

namespace UniversityBot.DataServices
{
    public class StudentDataService
    {
        public static async Task<List<Student>> GetStudentsAsync()
        {
            return await ApiService<List<Student>>.GetAsync($"Students");
        }

        public static async Task<Student> GetStudentByIdAsync(int id)
        {
            return await ApiService<Student>.GetAsync($"Students/{id}");
        }

        public static async Task<bool> EnrollStudentAsync(int studentId, string courseTitle)
        {
            var payload = new { StudentId = studentId, CourseTitle = courseTitle };
            return await ApiService<bool>.PostAsync<bool>($"Students/enroll", payload);
        }


    }
}