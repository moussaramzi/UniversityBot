using AdaptiveCards;
using CoreBot.Cards;
using CoreBot.DialogDetails;
using CoreBot.Models;
using CoreBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UniversityBot.CognitiveModels;
using Microsoft.Extensions.Logging; // Add this namespace

public class EnrollStudentDialog : ComponentDialog
{
    private readonly ILogger _logger;

    public EnrollStudentDialog(ILogger<EnrollStudentDialog> logger) : base(nameof(EnrollStudentDialog))
    {
        _logger = logger;

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
        AddDialog(new TextPrompt("EmailPrompt", EmailValidation));
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> InitializeEnrollmentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var details = stepContext.Options as EnrollStudentDetails ?? new EnrollStudentDetails();

        // Attempt to retrieve details from CLU recognizer result if available
        var activity = stepContext.Context.Activity;
        if (activity.Properties.TryGetValue("RecognizerResult", out var recognizerResultObj))
        {
            var recognizerResult = recognizerResultObj.ToObject<UniversityBotModel>();

            if (recognizerResult != null)
            {
                // Log the recognizer result for debugging
                _logger.LogInformation($"Recognizer Result: {System.Text.Json.JsonSerializer.Serialize(recognizerResult)}");

                details.FirstName ??= recognizerResult.Entities.GetFirstName();
                details.LastName ??= recognizerResult.Entities.GetLastName();
            }
            else
            {
                _logger.LogWarning("Recognizer result was null or could not be deserialized.");
            }
        }
        else
        {
            _logger.LogWarning("RecognizerResult property not found in activity.");
        }

        // Store details in stepContext.Values
        stepContext.Values["details"] = details;
        return await stepContext.NextAsync(null, cancellationToken);
    }





private async Task<DialogTurnResult> CollectFirstNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var details = (EnrollStudentDetails)stepContext.Values["details"];

        // Only ask for the first name if it's not already provided
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
        details.FirstName ??= stepContext.Result as string;

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
        details.LastName ??= stepContext.Result as string;

        if (string.IsNullOrEmpty(details.StudentMail))
        {
            return await stepContext.PromptAsync("EmailPrompt", new PromptOptions
            {
                Prompt = MessageFactory.Text("What is your email address?")
            }, cancellationToken);
        }

        return await stepContext.NextAsync(details.StudentMail, cancellationToken);
    }

    private async Task<DialogTurnResult> CollectCoursesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var details = (EnrollStudentDetails)stepContext.Values["details"];
        details.StudentMail ??= stepContext.Result as string;

        if (details.CourseTitles == null || !details.CourseTitles.Any())
        {
            // Fetch the list of available courses
            var availableCourses = await CourseDataService.GetCoursesAsync();
            if (availableCourses == null || !availableCourses.Any())
            {
                await stepContext.Context.SendActivityAsync("No courses are available at this time.");
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            // Generate the adaptive card with selectable courses
            var courseSelectionCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock
                {
                    Text = "Select the course(s) you'd like to enroll in:",
                    Weight = AdaptiveTextWeight.Bolder,
                    Size = AdaptiveTextSize.Large,
                    Wrap = true
                }
            }
            };

            // Add a toggle for each course
            courseSelectionCard.Body.AddRange(availableCourses.Select(course => new AdaptiveToggleInput
            {
                Id = $"course_{course.Title.Replace(" ", "_")}",
                Title = course.Title,
                Value = "false" // Default state
            }));

            // Add a submit action to capture the user's selections
            courseSelectionCard.Actions.Add(new AdaptiveSubmitAction
            {
                Title = "Submit",
                Data = new { Action = "SelectCourses" }
            });

            // Send the card to the user
            var cardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JObject.FromObject(courseSelectionCard)
            };

            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);

            return Dialog.EndOfTurn;
        }

        return await stepContext.NextAsync(details.CourseTitles, cancellationToken);
    }



    private async Task<DialogTurnResult> ConfirmEnrollmentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var details = (EnrollStudentDetails)stepContext.Values["details"];
        var activity = stepContext.Context.Activity;

        if (activity.Value is JObject userInput)
        {
            // Process the selected courses
            details.CourseTitles = userInput.Properties()
                .Where(prop => prop.Name.StartsWith("course_") && (bool)prop.Value)
                .Select(prop => prop.Name.Replace("course_", "").Replace("_", " "))
                .ToList();
        }

        if (!details.CourseTitles.Any())
        {
            await stepContext.Context.SendActivityAsync("You didn't select any courses. Please try again.");
            return await stepContext.ReplaceDialogAsync(nameof(EnrollStudentDialog), details, cancellationToken);
        }

        var cardAttachment = EnrollStudentCard.CreateConfirmationCard(details);
        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);

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
                await ApiService<object>.PostAsync<object>("students/enroll", details);
                await stepContext.Context.SendActivityAsync("Enrollment successful!");
            }
            catch (Exception ex)
            {
                await stepContext.Context.SendActivityAsync($"Enrollment failed: {ex.Message}");
            }
        }
        else
        {
            await stepContext.Context.SendActivityAsync("Enrollment process canceled.");
        }

        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

    private async Task<bool> EmailValidation(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
    {
        const string EmailValidationError = "The email you entered is not valid. Please try again.";
        var email = promptContext.Recognized.Value;

        if (Regex.IsMatch(email, @"^[\w\.-]+@[a-zA-Z\d\.-]+\.[a-zA-Z]{2,}$"))
        {
            return true;
        }

        await promptContext.Context.SendActivityAsync(EmailValidationError, cancellationToken: cancellationToken);
        return false;
    }
}
