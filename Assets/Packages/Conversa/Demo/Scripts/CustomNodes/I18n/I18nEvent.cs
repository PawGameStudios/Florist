using System;
using Conversa.Runtime;
using Conversa.Runtime.Interfaces;

namespace Conversa.Demo
{
    public class I18nEvent : IConversationEvent
    {
        public DemoActor Actor { get; }
        public string MessageKey { get; }
        public Action Advance { get; }

        public I18nEvent(Actor actor, string messageKey, Action advance)
        {
            Actor = actor as DemoActor;
            MessageKey = messageKey;
            Advance = advance;
        }
    }
}