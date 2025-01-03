using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using System.Threading;
using CoreBot.DialogDetails;
using CoreBot.Models;
using System;
using CoreBot.Cards;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;

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

                var events = await EventDataService.GetEventsAsync();

                if (eventDetail.eventName != null)
                {
                    events = events.FindAll(e => e.eventName.Equals(eventDetail.eventName, StringComparison.OrdinalIgnoreCase));
                }

                if (eventDetail.date != DateTime.MinValue)
                {
                    events = events.FindAll(e => e.date.Date == eventDetail.date.Date);
                }

                if (eventDetail.time != null)
                {
                    events = events.FindAll(e => e.time.Equals(eventDetail.time, StringComparison.OrdinalIgnoreCase));
                }

                if (events == null || events.Count == 0)
                {
                    await stepContext.Context.SendActivityAsync(
                        $"No events are currently planned.",
                        cancellationToken: cancellationToken);
                    return await stepContext.NextAsync(null, cancellationToken);
                }

                // Create the adaptive card for displaying courses
                var card = GetEventsCard.CreateCardAttachmentAsync(events);
                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);

            }
            catch (Exception ex)
            {
                // Log the exception
                stepContext.Context.TurnState.Get<ILogger>().LogError(ex, "Error in ShowEventsStepAsync");

                // Notify the user of the error
                await stepContext.Context.SendActivityAsync(
                    "An error occurred while retrieving the courses. Please try again later.",
                    cancellationToken: cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> EndDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
