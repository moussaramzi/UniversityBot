using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using UniversityBot.CognitiveModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.DialogDetails;
using CoreBot;
using Newtonsoft.Json.Linq;
using System.Text;

namespace UniversityBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly ILogger _logger;
        private readonly UniversityBotCLURecognizer _recognizer;

        public MainDialog(
            GetCoursesDialog getCoursesDialog,
            EnrollStudentDialog enrollStudentDialog,
            UniversityBotCLURecognizer recognizer,
            ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _logger = logger;
            _recognizer = recognizer;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(getCoursesDialog);
            AddDialog(enrollStudentDialog);

            var waterfallSteps = new WaterfallStep[]
            {
                FirstActionStepAsync,
                ActionActStepAsync,
                FinalStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FirstActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                throw new InvalidOperationException("Error: Recognizer not configured properly.");
            }

            // Default prompt if no action is detected
            var messageText = stepContext.Options?.ToString() ?? "How can I assist you today?";
            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) },
                cancellationToken);
        }

        private async Task<DialogTurnResult> ActionActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var activity = stepContext.Context.Activity;

                // Handle Adaptive Card Submit Action
                if (activity.Value != null)
                {
                    return await HandleAdaptiveCardSubmitAsync(activity.Value, stepContext, cancellationToken);
                }

                // Handle Recognizer Intent
                var result = await _recognizer.RecognizeAsync<UniversityBotModel>(stepContext.Context, cancellationToken);

                switch (result.GetTopIntent().intent)
                {
                    case UniversityBotModel.Intent.GetCourses:
                        return await stepContext.BeginDialogAsync(nameof(GetCoursesDialog), cancellationToken: cancellationToken);

                    case UniversityBotModel.Intent.EnrollStudent:
                        var enrollDetails = new EnrollStudentDetails
                        {
                            StudentID = GenerateStudentId(8),
                            CourseTitle = result.Entities.GetCourseName()
                        };
                        return await stepContext.BeginDialogAsync(nameof(EnrollStudentDialog), enrollDetails, cancellationToken: cancellationToken);

                    default:
                        await stepContext.Context.SendActivityAsync("I'm sorry, I didn't understand that. Can you try rephrasing?", cancellationToken: cancellationToken);
                        return await stepContext.NextAsync(null, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ActionActStepAsync");
                await stepContext.Context.SendActivityAsync("The bot encountered an error while processing your request.", cancellationToken: cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> HandleAdaptiveCardSubmitAsync(object value, WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                if (value is JObject actionData)
                {
                    var action = actionData["action"]?.ToString();

                    switch (action)
                    {
                        case "viewCourses":
                            return await stepContext.BeginDialogAsync(nameof(GetCoursesDialog), cancellationToken: cancellationToken);

                        case "enrollCourse":
                            return await stepContext.BeginDialogAsync(nameof(EnrollStudentDialog), cancellationToken: cancellationToken);

                        case "contactSupport":
                            await stepContext.Context.SendActivityAsync("Support contact functionality is not yet implemented.", cancellationToken: cancellationToken);
                            return await stepContext.NextAsync(null, cancellationToken);

                        default:
                            await stepContext.Context.SendActivityAsync("Unrecognized action from the card.", cancellationToken: cancellationToken);
                            return await stepContext.NextAsync(null, cancellationToken);
                    }
                }

                await stepContext.Context.SendActivityAsync("Invalid card action data.", cancellationToken: cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Adaptive Card submit action.");
                await stepContext.Context.SendActivityAsync("The bot encountered an error while processing the card action.", cancellationToken: cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = "What else can I help you with?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }

        private static Random _random = new Random();

        public static string GenerateStudentId(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(chars[_random.Next(chars.Length)]);
            }

            return stringBuilder.ToString();
        }
    }
}
