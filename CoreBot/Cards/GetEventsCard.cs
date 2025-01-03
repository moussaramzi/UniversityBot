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
        public static Attachment CreateCardAttachment(List<Event> events)
        {
            if (events == null || !events.Any())
            {
                return CreateNoEventsCard();
            }

            // Create card for available events
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
                        Text = "Here are the events that are planned:",
                        Wrap = true
                    },
                    new AdaptiveFactSet
                    {
                        Facts = events.Select(evnt => new AdaptiveFact
                        {
                            Title = string.Empty, // Use if meaningful titles can be added
                            Value = $"{evnt.eventName} on {evnt.date:MMMM dd, yyyy} at {evnt.time}"
                        }).ToList()
                    }
                }
            };

            return new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JObject.FromObject(card)
            };
        }

        private static Attachment CreateNoEventsCard()
        {
            var noEventsCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
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
                Content = JObject.FromObject(noEventsCard)
            };
        }
    }
}
