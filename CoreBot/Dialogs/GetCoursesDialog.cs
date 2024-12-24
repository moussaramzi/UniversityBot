using CoreBot.Cards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

public class GetCoursesDialog : ComponentDialog
{
    public GetCoursesDialog() : base(nameof(GetCoursesDialog))
    {
        var waterfallSteps = new WaterfallStep[]
        {
            ShowCoursesStepAsync,
            EndDialogStepAsync
        };

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> ShowCoursesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var card = await GetCoursesCard.CreateCardAttachmentAsync();
        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
        return await stepContext.NextAsync(null, cancellationToken);
    }

    private async Task<DialogTurnResult> EndDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}
