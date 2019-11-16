using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace docBot.Accessors
{
    public class ImageProcessingBotAccessors
    {

        public ImageProcessingBotAccessors(Microsoft.Bot.Builder.ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(ConversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(UserState));
        }

        public static readonly string CommandStateName = $"{nameof(ImageProcessingBotAccessors)}.CommandState";

        public static readonly string DialogStateName = $"{nameof(ImageProcessingBotAccessors)}.DialogState";

        public IStatePropertyAccessor<string> CommandState { get; set; }

        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }
        public ConversationState ConversationState { get; }

        public UserState UserState { get; }
    }
}
