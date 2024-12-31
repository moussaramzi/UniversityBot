using CoreBot.Cards;
using CoreBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System;
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
        try
        {
            // Fetch courses from the data service
            var courses = await CourseDataService.GetCoursesAsync();
            if (courses == null || courses.Count == 0)
            {
                await stepContext.Context.SendActivityAsync("No courses are currently available.", cancellationToken: cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Create the adaptive card for displaying courses
            var card = GetCoursesCard.CreateCardAttachmentAsync(courses);
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);

            return await stepContext.NextAsync(null, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the exception
            stepContext.Context.TurnState.Get<ILogger>().LogError(ex, "Error in ShowCoursesStepAsync");

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
