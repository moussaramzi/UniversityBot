using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using System.Threading;
using CoreBot.DialogDetails;
using CoreBot.Models;
using System;
using CoreBot.Cards;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace CoreBot.Dialogs
{
    public class GetEventsDialog : ComponentDialog
    {
        public GetEventsDialog() : base(nameof(GetEventsDialog)) 
        {
            var waterfallSteps = new WaterfallStep[]
            {
                ShowEventsStepAsync,
                EndDialogStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> ShowEventsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                EventDetail eventDetail = stepContext.Options as EventDetail;

                if (eventDetail == null)
                {
                    await stepContext.Context.SendActivityAsync("Invalid event details provided.", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }

                var events = await EventDataService.GetEventsAsync() ?? new List<Event>();

                if (!string.IsNullOrWhiteSpace(eventDetail.eventName))
                {
                    events = events.FindAll(e => e.eventName.Equals(eventDetail.eventName, StringComparison.OrdinalIgnoreCase));
                }

                if (eventDetail.date != DateTime.MinValue)
                {
                    events = events.FindAll(e => e.date.Date == eventDetail.date.Date);
                }

                if (!string.IsNullOrWhiteSpace(eventDetail.time))
                {
                    events = events.FindAll(e => e.time.Equals(eventDetail.time, StringComparison.OrdinalIgnoreCase));
                }

                if (!events.Any())
                {
                    await stepContext.Context.SendActivityAsync("No events are currently planned.", cancellationToken: cancellationToken);
                    return await stepContext.NextAsync(null, cancellationToken);
                }

                var card = GetEventsCard.CreateCardAttachment(events); // Ensure implementation
                if (card != null)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
                }

                return await stepContext.NextAsync(null, cancellationToken);
            }
            catch (Exception ex)
            {
                var logger = stepContext.Context.TurnState.Get<ILogger>();
                logger?.LogError(ex, "Error in ShowEventsStepAsync");

                await stepContext.Context.SendActivityAsync("An error occurred while retrieving the events. Please try again later.", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> EndDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
