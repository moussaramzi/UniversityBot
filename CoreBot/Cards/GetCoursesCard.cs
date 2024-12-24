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
            var courses = await CourseDataService.GetCoursesAsync();

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    // Title TextBlock
                    new AdaptiveTextBlock
                    {
                        Text = "Available Courses",
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large
                    },
                    // Subtitle TextBlock
                    new AdaptiveTextBlock
                    {
                        Text = "Here are the courses you can enroll in:",
                        Wrap = true
                    },
                    // List courses
                    new AdaptiveFactSet
                    {
                        Facts = courses.Select(course => new AdaptiveFact
                        {
                            Title = "-",
                            Value = course.Title
                        }).ToList()
                    }
                }
            };

            // Create an attachment
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JObject.FromObject(card)
            };

            return adaptiveCardAttachment;
        }
    }
}
