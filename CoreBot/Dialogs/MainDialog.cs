using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.Cards;
using CoreBot.DialogDetails;
using CoreBot.Models;
using UniversityBot.CognitiveModels;
using CoreBot.Dialogs;
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
            GetEventsDialog getEventsDialog,
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
            AddDialog(getEventsDialog);

            var waterfallSteps = new WaterfallStep[]
            {
                FirstActionStepAsync,
                ActionActStepAsync,
                FinalStepAsync
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

                if (!userProfile.HasShownWelcomeCard)
                {
                    var welcomeCard = WelcomeCard.CreateCardAttachment();
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(welcomeCard), cancellationToken);

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

            // Check for Adaptive Card actions
            if (activity.Value is JObject actionData)
            {
                var action = actionData["action"]?.ToString();

                switch (action)
                {
                    case "viewCourses":
                        var courses = await CourseDataService.GetCoursesAsync();
                        var coursesCard = await GetCoursesCard.CreateCardAttachmentAsync(courses);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(coursesCard), cancellationToken);
                        await stepContext.Context.SendActivityAsync("What else can I help you with?");
                        return await stepContext.EndDialogAsync(null, cancellationToken);

                    case "enrollCourse":
                        return await stepContext.BeginDialogAsync(nameof(EnrollStudentDialog), null, cancellationToken);

                    default:
                        return await stepContext.NextAsync(null, cancellationToken);
                }
            }

            // Process natural language input
            if (_recognizer.IsConfigured && activity.Type == ActivityTypes.Message)
            {
                try
                {
                    var result = await _recognizer.RecognizeAsync<UniversityBotModel>(stepContext.Context, cancellationToken);

                    switch (result.GetTopIntent().intent)
                    {
                        case UniversityBotModel.Intent.GetCourses:
                            string category = result.Entities.GetCourseCategory();

                            var coursesList = await CourseDataService.GetCoursesAsync();

                            if (!string.IsNullOrEmpty(category))
                            {
                                // Filter by category if input matches the course category
                                coursesList = coursesList.Where(course => course.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
                            }

                            // Create a card with the filtered list of courses
                            var coursesCard = await GetCoursesCard.CreateCardAttachmentAsync(coursesList);

                            // Send the courses card attachment to the user
                            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(coursesCard), cancellationToken);

                            // Ask if the user needs any further assistance
                            await stepContext.Context.SendActivityAsync("Here are the available courses. What else can I help you with?");
                            break;



                        case UniversityBotModel.Intent.EnrollStudent:
                            var enrollDetails = new EnrollStudentDetails
                            {
                                FirstName = result.Entities.GetFirstName(),
                                LastName = result.Entities.GetLastName(),
                                CourseTitles = result.Entities.GetCourseNames()
                            };
                            return await stepContext.BeginDialogAsync(nameof(EnrollStudentDialog), enrollDetails, cancellationToken);

                        case UniversityBotModel.Intent.GetEvents:
                            await stepContext.BeginDialogAsync(nameof(GetEventsDialog), null, cancellationToken);
                            break;

                        default:
                            await stepContext.Context.SendActivityAsync("I'm sorry, I didn't understand that. Can you try rephrasing?");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing CLU recognizer.");
                    await stepContext.Context.SendActivityAsync("An error occurred while processing your input. Please try again.");
                }
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("What can I help you with?", cancellationToken: cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
