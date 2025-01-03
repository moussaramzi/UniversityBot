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

                // Create the card
                var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                {
                    Body = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Text = "Available Courses",
                            Weight = AdaptiveTextWeight.Bolder,
                            Size = AdaptiveTextSize.ExtraLarge,
                            Spacing = AdaptiveSpacing.Medium,
                            Wrap = true
                        },
                        new AdaptiveTextBlock
                        {
                            Text = "Below are the courses you can enroll in, along with their details and instructors.",
                            Wrap = true,
                            Spacing = AdaptiveSpacing.Small
                        }
                    }
                };

                // Add details for each course in separate sections
                foreach (var course in courses)
                {
                    // Create a list of instructor names
                    var instructorNames = course.Instructors != null && course.Instructors.Any()
                        ? string.Join(", ", course.Instructors.Select(i => i.Name))
                        : "No instructors assigned";

                    card.Body.Add(new AdaptiveContainer
                    {
                        Items = new List<AdaptiveElement>
                        {
                            new AdaptiveTextBlock
                            {
                                Text = course.Title,
                                Weight = AdaptiveTextWeight.Bolder,
                                Size = AdaptiveTextSize.Large,
                                Wrap = true,
                                Spacing = AdaptiveSpacing.Medium
                            },
                            new AdaptiveTextBlock
                            {
                                Text = $"Category: {course.Category}",
                                Wrap = true,
                                Spacing = AdaptiveSpacing.Small
                            },
                            new AdaptiveTextBlock
                            {
                                Text = $"Enrolled Students: {course.RegisteredStudents}/{course.Capacity}",
                                Wrap = true,
                                Spacing = AdaptiveSpacing.Small
                            },
                            new AdaptiveTextBlock
                            {
                                Text = $"Instructors: {instructorNames}",
                                Wrap = true,
                                Spacing = AdaptiveSpacing.Small
                            }
                        },
                        Separator = true,
                        Spacing = AdaptiveSpacing.Medium
                    });
                }

                return new Attachment
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JObject.FromObject(card)
                };
            });
        }
    }
}
