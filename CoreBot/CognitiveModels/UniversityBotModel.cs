﻿using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace UniversityBot.CognitiveModels
{
    public class UniversityBotModel : IRecognizerConvert
    {
        public enum Intent
        {
            GetCourses,
            EnrollStudent,
            GetSchedule,
            GetEvents,
            None
        }

        public string Text { get; set; }

        public string AlteredText { get; set; }

        public Dictionary<Intent, IntentScore> Intents { get; set; }

        public CluEntities Entities { get; set; }

        public IDictionary<string, object> Properties { get; set; }

        public void Convert(dynamic result)
        {
            var jsonResult = JsonConvert.SerializeObject(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var app = JsonConvert.DeserializeObject<UniversityBotModel>(jsonResult);

            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
        }

        public (Intent intent, double score) GetTopIntent()
        {
            var maxIntent = Intent.None;
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }

            return (maxIntent, max);
        }

        public class CluEntities
        {
            public CluEntity[] Entities;

            public string GetCapacity() => Entities.Where(e => e.Category == "capacity").ToArray().FirstOrDefault()?.Text;
            public string GetCourseCategory() => Entities.Where(e => e.Category == "courseCategory").ToArray().FirstOrDefault()?.Text;
            public string GetCourseName() => Entities.Where(e => e.Category == "courseName").ToArray().FirstOrDefault()?.Text;
            public string GetDate() => Entities.Where(e => e.Category == "Date").ToArray().FirstOrDefault()?.Text;
            public string GetEventCategorie() => Entities.Where(e => e.Category == "EventCategorie").ToArray().FirstOrDefault()?.Text;
            public string GetEventName() => Entities.Where(e => e.Category == "EventName").ToArray().FirstOrDefault()?.Text;
            public string GetEventPlace() => Entities.Where(e => e.Category == "EventPlace").ToArray().FirstOrDefault()?.Text;
            public string GetTime() => Entities.Where(e => e.Category == "Time").ToArray().FirstOrDefault()?.Text;
        }

        public class CluEntity
        {
            public string Category { get; set; }
            public string Text { get; set; }
        }
    }
}