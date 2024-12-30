// Generated with CoreBot .NET Template version v4.22.0

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CoreBot
{
    public class AdapterWithErrorHandler : CloudAdapter
    {
        public AdapterWithErrorHandler(BotFrameworkAuthentication auth, ILogger<IBotFrameworkHttpAdapter> logger, ConversationState conversationState = default)
            : base(auth, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error: {exception.Message}\nStackTrace: {exception.StackTrace}\nInnerException: {exception.InnerException?.Message}");

                // Send a message to the user
                var errorMessageText = "The bot encountered an error or bug.";
                var errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.ExpectingInput);
                await turnContext.SendActivityAsync(errorMessage);

                var retryMessageText = "Please try again, or ask something else while we look into this issue.";
                var retryMessage = MessageFactory.Text(retryMessageText, retryMessageText, InputHints.ExpectingInput);
                await turnContext.SendActivityAsync(retryMessage);

                if (conversationState != null)
                {
                    try
                    {
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, $"Exception caught while attempting to delete conversation state: {e.Message}");
                    }
                }
                else
                {
                    logger.LogWarning("ConversationState is null; skipping deletion.");
                }

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace",
                    $"Error: {exception.Message}\nUser Input: {turnContext.Activity.Text}\nConversation ID: {turnContext.Activity.Conversation.Id}",
                    "https://www.botframework.com/schemas/error",
                    "TurnError");
            };
        }
    }
}
