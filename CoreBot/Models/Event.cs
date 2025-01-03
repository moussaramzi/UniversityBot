using System;

namespace CoreBot.Models
{
    public class Event
    {
        public int eventID { get; set; }
        public string eventName { get; set; }
        public DateTime date { get; set; }
        public string time { get; set; }
    }
}
