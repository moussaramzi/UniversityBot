﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using UniversityBot.CognitiveModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.DialogDetails;
using CoreBot;

namespace UniversityBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly ILogger _logger;
        private readonly UniversityBotCLURecognizer _recognizer;

        public MainDialog(
            GetCoursesDialog getCoursesDialog,
            EnrollStudentDialog enrollStudentDialog,
            //GetScheduleDialog getScheduleDialog,
            //GetEventsDialog getEventsDialog,
            UniversityBotCLURecognizer recognizer,
            ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _logger = logger;
            _recognizer = recognizer;

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            var waterfallSteps = new WaterfallStep[]
            {
                FirstActionStepAsync,
                ActionActStepAsync,
                FinalStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            AddDialog(getCoursesDialog);
            AddDialog(enrollStudentDialog);
            //AddDialog(getScheduleDialog);
            //AddDialog(getEventsDialog);

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FirstActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                throw new InvalidOperationException("Error: Recognizer not configured properly.");
            }

            var messageText = stepContext.Options?.ToString() ?? "How can I assist you today?";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActionActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = await _recognizer.RecognizeAsync<UniversityBotModel>(stepContext.Context, cancellationToken);

            switch (result.GetTopIntent().intent)
            {
                case UniversityBotModel.Intent.GetCourses:
                    return await stepContext.BeginDialogAsync(nameof(GetCoursesDialog), cancellationToken: cancellationToken);

                case UniversityBotModel.Intent.EnrollStudent:
                    var enrollDetails = new EnrollStudentDetails
                    {
                        StudentID = result.Entities.GetStudentID(),
                        CourseTitle = result.Entities.GetCourseTitle()
                    };
                    return await stepContext.BeginDialogAsync(nameof(EnrollStudentDialog), enrollDetails, cancellationToken);

                //case UniversityBotModel.Intent.GetSchedule:
                //    return await stepContext.BeginDialogAsync(nameof(GetScheduleDialog), cancellationToken: cancellationToken);

                //case UniversityBotModel.Intent.GetEvents:
                //    return await stepContext.BeginDialogAsync(nameof(GetEventsDialog), cancellationToken: cancellationToken);

                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sorry, I didn't understand that."), cancellationToken);
                    return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = "What else can I help you with?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}
