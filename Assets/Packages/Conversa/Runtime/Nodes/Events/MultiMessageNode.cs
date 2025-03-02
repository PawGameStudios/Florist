using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Conversa.Runtime.Events;
using Conversa.Runtime.Interfaces;

namespace Conversa.Runtime.Nodes
{
	[Serializable]
	[Port("Previous", "previous", typeof(BaseNode), Flow.In, Capacity.Many)]
	[Port("Next", "next", typeof(BaseNode), Flow.Out, Capacity.One)]
	public class MultiMessageNode : BaseNode, IEventNode
	{
		public const string DefaultMessage = "Enter your message here";

		[SerializeField] private Actor actor;
		[SerializeField] private List<string> messages = new List<string>();

		public Actor Actor
		{
			get => actor;
			set => actor = value;
		}

		public List<string> Messages
		{
			get => messages;
			set => messages = value;
		}

		public string ActorName => Actor?.DisplayName;

		public MultiMessageNode() { }

		public MultiMessageNode(Actor presetActor)
		{
			if (presetActor == null) return;
			actor = presetActor;
		}

		// Processing node

		void AdvanceNextNode(Conversation conversation, ConversationEvents conversationEvents)
		{
			var nodePort = GetNodePort("next");
			var oppositeNodes = conversation.GetOppositeNodes(nodePort);
			var nextNode = oppositeNodes.FirstOrDefault();
			conversation.Process(nextNode, conversationEvents);
		}

		Action ProcessIndex(Conversation conversation, ConversationEvents conversationEvents, int index) => () =>
		{
			if (index >= messages.Count)
			{
				AdvanceNextNode(conversation, conversationEvents);
				return;
			}

			var e = new ActorMessageEvent(actor, messages[index], ProcessIndex(conversation, conversationEvents, index + 1));
			conversationEvents.OnConversationEvent.Invoke(e);
		};



		public void Process(Conversation conversation, ConversationEvents conversationEvents)
		{

			// If there are no messages, advance
			if (messages.Count == 0)
			{
				AdvanceNextNode(conversation, conversationEvents);
				return;
			}

			ProcessIndex(conversation, conversationEvents, 0)();
		}
	}
}