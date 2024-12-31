using CoreBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreBot.Models
{
    public class InstructorDataService
    {
        public static async Task<List<Instructor>> GetInstructorsAsync()
        {
            return await ApiService<List<Instructor>>.GetAsync($"instructors");
        }

        public static async Task<Instructor> GetInstructorByIdAsync(int id)
        {
            return await ApiService<Instructor>.GetAsync($"instructors/{id}");
        }
    }
}
