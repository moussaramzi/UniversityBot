using CoreBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreBot.Models
{
    public class CourseDataService
    {
        public static async Task<List<Course>> GetCoursesAsync()
        {
            return await ApiService<List<Course>>.GetAsync($"courses");
        }

        public static async Task<Course> GetCourseByIdAsync(int id)
        {
            return await ApiService<Course>.GetAsync($"courses/{id}");
        }
    }
}
