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

namespace UniversityBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly ILogger _logger;
        private readonly UserState _userState;

        public MainDialog(
            GetCoursesDialog getCoursesDialog,
            EnrollStudentDialog enrollStudentDialog,
            UserState userState,
            ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _logger = logger;
            _userState = userState;

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
                    await userProfileAccessor.SetAsync(stepContext.Context, userProfile); // Corrected this line
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating or sending WelcomeCard.");
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("An error occurred while displaying the welcome card."),
                    cancellationToken
                );
            }


            return await stepContext.NextAsync(null, cancellationToken);
        }



        private async Task<DialogTurnResult> ActionActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var activity = stepContext.Context.Activity;

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
                        // Ask what else the user needs help with
                        await stepContext.Context.SendActivityAsync("What else can I help you with?");
                        return await stepContext.EndDialogAsync(null, cancellationToken);

                    case "enrollCourse":
                        return await stepContext.BeginDialogAsync(nameof(EnrollStudentDialog), cancellationToken);

<<<<<<< HEAD
                switch (result.GetTopIntent().intent)
                {
                    case UniversityBotModel.Intent.GetCourses:
                        var courseCategorie = result.Entities.GetCourseCategory(); // Method to extract course category
                        return await stepContext.BeginDialogAsync(nameof(GetCoursesDialog), courseCategorie, cancellationToken: cancellationToken);

                    case UniversityBotModel.Intent.EnrollStudent:
                        var enrollDetails = new EnrollStudentDetails
                        {
                            StudentID = GenerateStudentId(8),
                            CourseTitle = result.Entities.GetCourseName()
                        };
                        return await stepContext.BeginDialogAsync(nameof(EnrollStudentDialog), enrollDetails, cancellationToken: cancellationToken);
=======
           
>>>>>>> 9efc54406d5af5cf8b7ee852da59262d23675404

                    default:
                        await stepContext.Context.SendActivityAsync("Unrecognized action.");
                        return await stepContext.NextAsync(null, cancellationToken);
                }
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }


        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("What can I help you with?", cancellationToken: cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

<<<<<<< HEAD
        private static Random _random = new Random();

        public static int GenerateStudentId(int length)
=======
        public static string GenerateStudentId(int length)
>>>>>>> 9efc54406d5af5cf8b7ee852da59262d23675404
        {
            const string chars = "0123456789"; // Use only numeric characters to ensure valid integer conversion
            var stringBuilder = new StringBuilder();
            var random = new Random();

            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(chars[random.Next(chars.Length)]);
            }

            // Convert the generated string to an integer
            // Use int.TryParse for safer conversion
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
