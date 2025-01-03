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
        public static async Task<Attachment> CreateCardAttachmentAsync(List<Course> courses)
        {
            return await Task.Run(() =>
            {
                if (courses == null || !courses.Any())
                {
                    // Handle no courses case
                    var noCoursesCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveTextBlock
                            {
                                Text = "No courses are available at this time.",
                                Weight = AdaptiveTextWeight.Bolder,
                                Size = AdaptiveTextSize.Large,
                                Wrap = true
                            }
                        }
                    };

                    return new Attachment
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JObject.FromObject(noCoursesCard)
                    };
                }

                // Normal card creation logic
                var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                {
                    Body = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Text = "Available Courses",
                            Weight = AdaptiveTextWeight.Bolder,
                            Size = AdaptiveTextSize.Large
                        },
                        new AdaptiveTextBlock
                        {
                            Text = "Here are the courses you can enroll in:",
                            Wrap = true
                        },
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

                return new Attachment
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JObject.FromObject(card)
                };
            });
        }
    }
}
