using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CoreBot.Cards
{
    public static class WelcomeCard
    {
        public static Attachment CreateCardAttachment()
        {
            // Create the Adaptive Card
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveImage
                    {
                        Url = new Uri("https://i.imgur.com/YEN6J83.png"), // Update this to your preferred image
                        Size = AdaptiveImageSize.Stretch
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Welcome to University Bot!",
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Your personal assistant for managing university courses, schedules, and more.",
                        Wrap = true
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "View Courses",
                        Data = new { action = "viewCourses" }
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = "Enroll in a Course",
                        Data = new { action = "enrollCourse" }
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
