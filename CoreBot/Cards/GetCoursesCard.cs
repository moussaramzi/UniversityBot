using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using CoreBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace CoreBot.Cards
{
    public class GetCoursesCard
    {
        public static async Task<Attachment> CreateCardAttachmentAsync()
        {
            // Retrieve course data
            var courses = await CourseDataService.GetCoursesAsync() ?? new List<Course>();

            // Create the adaptive card
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3))
            {
                Body = new List<AdaptiveElement>
        {
            // Title
            new AdaptiveTextBlock
            {
                Text = "Available Courses",
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Large
            },
            // Subtitle
            new AdaptiveTextBlock
            {
                Text = "Here are the courses you can enroll in:",
                Wrap = true
            }
        }
            };

            // Add course list or fallback message
            if (!courses.Any())
            {
                card.Body.Add(new AdaptiveTextBlock
                {
                    Text = "No courses are currently available.",
                    Weight = AdaptiveTextWeight.Lighter,
                    Wrap = true
                });
            }
            else
            {
                card.Body.Add(new AdaptiveContainer
                {
                    Items = courses.Select(course => new AdaptiveTextBlock
                    {
                        Text = $"• {course.Title}",
                        Wrap = true
                    }).ToList<AdaptiveElement>()
                });
            }

            // Create an adaptive card attachment
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JObject.FromObject(card)
            };

            return adaptiveCardAttachment;
        }

    }
}
