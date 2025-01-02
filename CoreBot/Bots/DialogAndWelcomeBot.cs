using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.Cards;
using CoreBot.Models;

namespace CoreBot.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T>
        where T : Dialog
    {
        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base(conversationState, userState, dialog, logger)
        {
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Skip sending the card if it has already been shown
            var userProfileAccessor = UserState.CreateProperty<UserProfile>("UserProfile");
            var userProfile = await userProfileAccessor.GetAsync(turnContext, () => new UserProfile(), cancellationToken);

            if (userProfile.HasShownWelcomeCard)
            {
                return; // Exit early if the welcome card was already shown
            }

            foreach (var member in membersAdded)
            {
                // Skip if the member added is the bot itself
                if (member.Id == turnContext.Activity.Recipient.Id)
                {
                    continue;
                }

                // Dynamically create and send the welcome card
                var welcomeCard = WelcomeCard.CreateCardAttachment();
                var response = MessageFactory.Attachment(welcomeCard);
                await turnContext.SendActivityAsync(response, cancellationToken);

                // Mark the welcome card as shown
                userProfile.HasShownWelcomeCard = true;
                await userProfileAccessor.SetAsync(turnContext, userProfile, cancellationToken);

                // Start the dialog
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            }
        }


    }
}
