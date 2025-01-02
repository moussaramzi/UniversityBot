using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.Cards;
using CoreBot.DialogDetails;
using CoreBot.Models;
using UniversityBot.CognitiveModels;
using CoreBot;

namespace UniversityBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly ILogger _logger;
        private readonly UserState _userState;
        private readonly UniversityBotCLURecognizer _recognizer;

        public MainDialog(
            GetCoursesDialog getCoursesDialog,
            EnrollStudentDialog enrollStudentDialog,
            UserState userState,
            UniversityBotCLURecognizer recognizer,
            ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _logger = logger;
            _userState = userState;
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
            try
            {
                var userProfileAccessor = _userState.CreateProperty<UserProfile>("UserProfile");
                var userProfile = await userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

                // Check if the welcome card has been shown
                if (!userProfile.HasShownWelcomeCard)
                {
                    var welcomeCard = WelcomeCard.CreateCardAttachment();
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(welcomeCard), cancellationToken);

                    // Mark the welcome card as shown
                    userProfile.HasShownWelcomeCard = true;
                    await userProfileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during FirstActionStepAsync.");
                await stepContext.Context.SendActivityAsync("An error occurred during initialization. Please try again later.");
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }


        private async Task<DialogTurnResult> ActionActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var activity = stepContext.Context.Activity;

            // Skip handling for non-message activities (e.g., conversationUpdate)
            if (activity.Type != ActivityTypes.Message)
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Check for Adaptive Card actions
            if (activity.Value != null)
            {
                var actionData = activity.Value as JObject;
                var action = actionData?["action"]?.ToString();

                switch (action)
                {
                    case "viewCourses":
                        var courses = await CourseDataService.GetCoursesAsync();
                        var card = GetCoursesCard.CreateCardAttachmentAsync(courses);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
                        await stepContext.Context.SendActivityAsync("What else can I help you with?");
                        return await stepContext.EndDialogAsync(null, cancellationToken);

                    case "enrollCourse":
                        return await stepContext.BeginDialogAsync(nameof(EnrollStudentDialog), null, cancellationToken);

                    default:
                        await stepContext.Context.SendActivityAsync("Unrecognized action.");
                        return await stepContext.NextAsync(null, cancellationToken);
                }
            }

            // Handle CLU intents
            var text = activity.Text?.Trim();

            if (string.IsNullOrEmpty(text))
            {
                // Ignore empty or null input, especially for initial turns
                return await stepContext.NextAsync(null, cancellationToken);
            }

            try
            {
                var result = await _recognizer.RecognizeAsync<UniversityBotModel>(stepContext.Context, cancellationToken);

                switch (result.GetTopIntent().intent)
                {
                    case UniversityBotModel.Intent.GetCourses:
                        var coursesList = await CourseDataService.GetCoursesAsync();
                        var coursesCard = GetCoursesCard.CreateCardAttachmentAsync(coursesList);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(coursesCard), cancellationToken);
                        await stepContext.Context.SendActivityAsync("Here are the available courses. What else can I help you with?");
                        return await stepContext.EndDialogAsync(null, cancellationToken);

                    case UniversityBotModel.Intent.EnrollStudent:
                        return await stepContext.BeginDialogAsync(nameof(EnrollStudentDialog), null, cancellationToken);

                    default:
                        await stepContext.Context.SendActivityAsync("I'm sorry, I didn't understand that. Can you try rephrasing?");
                        return await stepContext.NextAsync(null, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CLU recognizer.");
                await stepContext.Context.SendActivityAsync("An error occurred while processing your input. Please try again.");
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }



        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("What can I help you with?", cancellationToken: cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        public static int GenerateStudentId(int length)
        {
            const string chars = "0123456789"; // Use only numeric characters to ensure valid integer conversion
            var stringBuilder = new StringBuilder();
            var random = new Random();

            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(chars[random.Next(chars.Length)]);
            }

            // Convert the generated string to an integer
            if (int.TryParse(stringBuilder.ToString(), out int studentId))
            {
                return studentId;
            }
            else
            {
                throw new InvalidOperationException("Failed to generate a valid numeric student ID.");
            }
        }
    }
}
