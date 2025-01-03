using CoreBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreBot.Models
{
    public class EventDataService
    {
        public static async Task<List<Event>> GetEventsAsync()
        {
            return await ApiService<List<Event>>.GetAsync($"Event");
        }

        public static async Task<Event> GetCourseByIdAsync(int id)
        {
            return await ApiService<Event>.GetAsync($"Event/{id}");
        }
    }
}
