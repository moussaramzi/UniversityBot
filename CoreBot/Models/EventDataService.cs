using CoreBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreBot.Models
{
    public class EventDataService
    {
        public static async Task<List<Event>> GetEventsAsync()
        {
<<<<<<< HEAD
            return await ApiService<List<Event>>.GetAsync($"event");
=======
            return await ApiService<List<Event>>.GetAsync($"Event");
>>>>>>> 42e90adbf6ec4c37c05b0f822e580a2bafe6f62a
        }

        public static async Task<Event> GetCourseByIdAsync(int id)
        {
<<<<<<< HEAD
            return await ApiService<Event>.GetAsync($"event/{id}");
=======
            return await ApiService<Event>.GetAsync($"Event/{id}");
>>>>>>> 42e90adbf6ec4c37c05b0f822e580a2bafe6f62a
        }
    }
}
