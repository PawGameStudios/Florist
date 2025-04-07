using System;
using System.Collections.Generic;
using Conversa.Runtime.Interfaces;

namespace Conversa.Runtime.Events
{
	public enum StringParseOptions
	{
		None, Count, FlowerType, FlowerCountAndType, BouquetType, BouquetContent,
		FlowerColor, Ribbon, WrappingPaper
	}

	public class Option
	{
		public string Message { get; }
		public Action Advance { get; }

		public Option(string message, Action advance)
		{
			Message = message;
			Advance = advance;
		}
	}

	public class ChoiceEvent : IConversationEvent
	{
		public string Actor { get; }
		public string Message { get; }
		public string Key { get; }
		public List<StringParseOptions> ParseOptions { get; }
		public List<Option> Options { get; }

		public ChoiceEvent(string actor, string key, string message, List<Option> options, List<StringParseOptions> parseOptions)
		{
			Actor = actor;
			Key = key;
			Message = message;
			Options = options;
			ParseOptions = parseOptions;
		}

		public ChoiceEvent(string actor, string key, string message, List<Option> options)
		{
			Actor = actor;
			Key = key;
			Message = message;
			Options = options;
		}
	}
}