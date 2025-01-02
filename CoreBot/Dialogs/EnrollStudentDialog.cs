using CoreBot.Cards;
using CoreBot.DialogDetails;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

public class EnrollStudentDialog : ComponentDialog
{
    public EnrollStudentDialog() : base(nameof(EnrollStudentDialog))
    {
        var waterfallSteps = new WaterfallStep[]
        {
            ConfirmEnrollmentStepAsync,
            EndDialogStepAsync
        };

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> ConfirmEnrollmentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var details = (EnrollStudentDetails)stepContext.Options;

        if (details != null)
        {
            // Assuming StudentID is an integer, no need for parsing
            var studentId = details.StudentID;

            // Generate and send the card
            var card = await EnrollStudentCard.CreateCardAttachmentAsync(studentId, details.CourseTitle);
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
        }
        else
        {
            await stepContext.Context.SendActivityAsync("Invalid enrollment details.", cancellationToken: cancellationToken);
        }

        return await stepContext.NextAsync(null, cancellationToken);
    }



    private async Task<DialogTurnResult> EndDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}
