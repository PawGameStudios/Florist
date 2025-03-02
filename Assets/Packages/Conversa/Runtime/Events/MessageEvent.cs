using System;
using Conversa.Runtime.Interfaces;

namespace Conversa.Runtime.Events
{
	public class MessageEvent : IConversationEvent
	{
		public string Actor { get; }
		public string Message { get; }
		public Action Advance { get; }

		public MessageEvent(string actor, string message, Action advance)
		{
			Actor = actor;
			Message = message;
			Advance = advance;
		}
	}

	public class LocalizedMessageEvent : IConversationEvent
	{
		public string Key { get; }
		public string Message { get; }
		public Action Advance { get; }

		public LocalizedMessageEvent(string key, string message, Action advance)
		{
			Key = key;
			Message = message;
			Advance = advance;
		}
	}
}