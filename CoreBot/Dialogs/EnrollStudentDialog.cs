using CoreBot.Cards;
using CoreBot.DialogDetails;
using CoreBot.Models;
using CoreBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class EnrollStudentDialog : ComponentDialog
{
    public EnrollStudentDialog() : base(nameof(EnrollStudentDialog))
    {
        var waterfallSteps = new WaterfallStep[]
        {
            InitializeEnrollmentStepAsync,
            CollectFirstNameStepAsync,
            CollectLastNameStepAsync,
            CollectEmailStepAsync,
            CollectCoursesStepAsync,
            ConfirmEnrollmentStepAsync,
            FinalizeEnrollmentStepAsync
        };

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> InitializeEnrollmentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var details = stepContext.Options as EnrollStudentDetails ?? new EnrollStudentDetails();

        // Pre-fill the name from user input or activity context
        var fullName = stepContext.Context.Activity.From?.Name;
        if (!string.IsNullOrEmpty(fullName) && !fullName.Equals("user", StringComparison.OrdinalIgnoreCase))
        {
            var nameParts = fullName.Split(' ');
            details.FirstName = string.IsNullOrEmpty(details.FirstName) ? nameParts[0] : details.FirstName;
            details.LastName = string.IsNullOrEmpty(details.LastName) && nameParts.Length > 1 ? nameParts[1] : details.LastName;
        }

        stepContext.Values["details"] = details;
        return await stepContext.NextAsync(null, cancellationToken);
    }

    private async Task<DialogTurnResult> CollectFirstNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var details = (EnrollStudentDetails)stepContext.Values["details"];

        if (string.IsNullOrEmpty(details.FirstName))
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("What is your first name?")
            }, cancellationToken);
        }

        return await stepContext.NextAsync(details.FirstName, cancellationToken);
    }

    private async Task<DialogTurnResult> CollectLastNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var details = (EnrollStudentDetails)stepContext.Values["details"];
        details.FirstName ??= (string)stepContext.Result;

        if (string.IsNullOrEmpty(details.LastName))
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("What is your last name?")
            }, cancellationToken);
        }

        return await stepContext.NextAsync(details.LastName, cancellationToken);
    }

    private async Task<DialogTurnResult> CollectEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var details = (EnrollStudentDetails)stepContext.Values["details"];
        details.LastName ??= (string)stepContext.Result;

        if (string.IsNullOrEmpty(details.StudentMail))
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("What is your email address?")
            }, cancellationToken);
        }

        return await stepContext.NextAsync(details.StudentMail, cancellationToken);
    }

    private async Task<DialogTurnResult> CollectCoursesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var details = (EnrollStudentDetails)stepContext.Values["details"];
        details.StudentMail ??= (string)stepContext.Result;

        // Fetch the list of available courses
        var availableCourses = await CourseDataService.GetCoursesAsync();
        var availableCourseTitles = availableCourses.Select(c => c.Title).ToList();

        if (details.CourseTitles == null || !details.CourseTitles.Any())
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Which course(s) would you like to enroll in? (Separate multiple courses with commas)")
            }, cancellationToken);
        }

        // Validate user input
        var courseInput = stepContext.Result as string;
        var providedCourses = courseInput?.Split(',').Select(c => c.Trim()).ToList();

        if (providedCourses != null && providedCourses.Any())
        {
            var validCourses = providedCourses.Where(course => availableCourseTitles.Contains(course, StringComparer.OrdinalIgnoreCase)).ToList();
            var invalidCourses = providedCourses.Except(validCourses, StringComparer.OrdinalIgnoreCase).ToList();

            if (invalidCourses.Any())
            {
                await stepContext.Context.SendActivityAsync($"The following courses are not available: {string.Join(", ", invalidCourses)}");
                await stepContext.Context.SendActivityAsync($"Available courses are: {string.Join(", ", availableCourseTitles)}");
                return await stepContext.ReplaceDialogAsync(nameof(EnrollStudentDialog), details, cancellationToken);
            }

            details.CourseTitles = validCourses;
        }
        else
        {
            await stepContext.Context.SendActivityAsync("No valid courses were provided. Please try again.");
            return await stepContext.ReplaceDialogAsync(nameof(EnrollStudentDialog), details, cancellationToken);
        }

        return await stepContext.NextAsync(details.CourseTitles, cancellationToken);
    }


    private async Task<DialogTurnResult> ConfirmEnrollmentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var details = (EnrollStudentDetails)stepContext.Values["details"];
        var courseInput = stepContext.Result as string;

        if (!string.IsNullOrEmpty(courseInput))
        {
            details.CourseTitles = courseInput.Split(',').Select(course => course.Trim()).ToList();
        }

        var confirmationCard = await EnrollStudentCard.CreateCardAttachmentAsync(details);
        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(confirmationCard), cancellationToken);

        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        {
            Prompt = MessageFactory.Text("Does this look correct? (yes/no)")
        }, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalizeEnrollmentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var confirmation = (string)stepContext.Result;

        if (confirmation?.ToLower() == "yes")
        {
            var details = (EnrollStudentDetails)stepContext.Values["details"];
            try
            {
                var result = await ApiService<object>.PostAsync<object>("students/enroll", details);
                await stepContext.Context.SendActivityAsync("Enrollment successful!");
            }
            catch (Exception ex)
            {
                await stepContext.Context.SendActivityAsync($"Enrollment failed: {ex.Message}");
            }
        }
        else
        {
            await stepContext.Context.SendActivityAsync("Okay, let's try again.");
            return await stepContext.ReplaceDialogAsync(nameof(EnrollStudentDialog), null, cancellationToken);
        }

        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}
