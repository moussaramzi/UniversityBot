using CoreBot.Cards;
using CoreBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class GetEventsDialog : ComponentDialog
{
    private readonly ILogger _logger;

    public GetEventsDialog(ILogger<GetEventsDialog> logger) : base(nameof(GetEventsDialog))
    {
        _logger = logger;

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
            // Fetch all events
            var events = await EventDataService.GetEventsAsync() ?? new List<Event>();

            // Check if any events are available
            if (events.Count == 0)
            {
                await stepContext.Context.SendActivityAsync(
                    "No events are currently planned.",
                    cancellationToken: cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Create the adaptive card for displaying events
            var card = GetEventsCard.CreateCardAttachment(events);
            if (card != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ShowEventsStepAsync");

            await stepContext.Context.SendActivityAsync(
                "An error occurred while retrieving the events. Please try again later.",
                cancellationToken: cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }

    private async Task<DialogTurnResult> EndDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}
