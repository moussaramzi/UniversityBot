using CoreBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreBot.Models
{
    public class CourseDataService
    {
        public static async Task<List<Course>> GetCoursesAsync()
        {
            return await ApiService<List<Course>>.GetAsync($"Courses");
        }

        public static async Task<Course> GetCourseByIdAsync(int id)
        {
            return await ApiService<Course>.GetAsync($"Courses/{id}");
        }
    }
}
