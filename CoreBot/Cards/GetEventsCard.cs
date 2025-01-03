using AdaptiveCards;
using CoreBot.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace CoreBot.Cards
{
    public class GetEventsCard
    {
        public static Attachment CreateCardAttachmentAsync(List<Event> events)
        {
            if (events == null || !events.Any())
            {
                // Handle no courses case
                var noCoursesCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                {
                    Body = new List<AdaptiveElement>
                        {
                            new AdaptiveTextBlock
                            {
                                Text = "No events are available at this time.",
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
                        Text = "Current Events",
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Here are the events that are planned",
                        Wrap = true
                    },
                    new AdaptiveFactSet
                    {
                        Facts = events.Select(event1 => new AdaptiveFact
                        {
                            Title = "-",
                            Value = event1.eventName + " " + event1.date
                        }).ToList()
                    }
                }
            };

            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JObject.FromObject(card)
            };

            return adaptiveCardAttachment;
        }


    }
}
