using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using CoreBot.DialogDetails;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Cards
{
    public static class EnrollStudentCard
    {
        public static async Task<Attachment> CreateCardAttachmentAsync(EnrollStudentDetails details)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Confirm Enrollment",
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large
                    },
                    new AdaptiveTextBlock
                    {
                        Text = $"First Name: {details.FirstName}",
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = $"Last Name: {details.LastName}",
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = $"Email: {details.StudentMail}",
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Courses:",
                        Weight = AdaptiveTextWeight.Bolder,
                        Wrap = true
                    },
                    new AdaptiveFactSet
                    {
                        Facts = details.CourseTitles.Select(course => new AdaptiveFact
                        {
                            Title = "-",
                            Value = course
                        }).ToList()
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Confirm",
                        Data = new { action = "confirmEnrollment" }
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = "Cancel",
                        Data = new { action = "cancelEnrollment" }
                    }
                }
            };

            return new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JObject.FromObject(card)
            };
        }
    }
}
