using System;
using System.Linq;
using Conversa.Runtime;
using Conversa.Runtime.Interfaces;
using UnityEngine;

namespace Conversa.Demo
{
    [Serializable]
    [Port("Previous", "previous", typeof(BaseNode), Flow.In, Capacity.Many)]
    [Port("Next", "next", typeof(BaseNode), Flow.Out, Capacity.One)]
    public class I18nNode : BaseNode, IEventNode
    {
        [SerializeField] private Actor actor;
        [SerializeField] private string messageKey;

        [Slot("Actor", "actor", Flow.In, Capacity.One)]
        public Actor Actor
        {
            get => actor;
            set => actor = value;
        }

        [Slot("Message Key", "message", Flow.In, Capacity.One)]
        public string MessageKey
        {
            get => messageKey;
            set => messageKey = value;
        }

        public I18nNode() { }

        public void Process(Conversation conversation, ConversationEvents conversationEvents)
        {
            void Advance()
            {
                var nextNode = conversation.GetOppositeNodes(GetNodePort("next")).FirstOrDefault();
                conversation.Process(nextNode, conversationEvents);
            }

            // TEMPLATE: Replace with your own custom event
            var e = new I18nEvent(actor, messageKey, Advance);
            conversationEvents.OnConversationEvent.Invoke(e);
        }
    }
}