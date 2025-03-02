using System;
using System.Collections.Generic;
using System.Linq;
using Conversa.Runtime;
using Conversa.Runtime.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Conversa.Editor
{
	public class MessageNodeView : BaseNodeView<MessageNode>
	{
		protected override string Title => "Message";

		// Constructors

		public MessageNodeView(Conversation conversation)
			: base(new MessageNode(ConversaSettings.instance.UseActorProfileByDefault), conversation) { }

		public MessageNodeView(MessageNode data, Conversation conversation) : base(data, conversation) { }

		private Label actorLabel;
		private Label messageLabel;

		// Methods

		protected override void SetBody()
		{
			var template = Resources.Load<VisualTreeAsset>("NodeViews/MessageNode");
			template.CloneTree(bodyContainer);

			actorLabel = bodyContainer.Q<Label>("actor");
			messageLabel = bodyContainer.Q<Label>("message");
			UpdateValues();

			schedule.Execute(UpdateValues).Every(100);
		}

		private void UpdateValues()
		{
			if (actorLabel.text != Data.ActorName)
				actorLabel.text = Data.ActorName;

			if (Data.Message != messageLabel.text)
				messageLabel.text = Data.Message;
		}
	}

	public class LocalizedMessageNodeView : BaseNodeView<LocalizedMessageNode>
	{
		protected override string Title => "Localized Message";

		// Constructors

		public LocalizedMessageNodeView(Conversation conversation)
			: base(new LocalizedMessageNode(), conversation) { }

		public LocalizedMessageNodeView(LocalizedMessageNode data, Conversation conversation) : base(data, conversation) { }

		private Label messageKeyLabel;
		private Label messageLabel;
		private VisualElement optionList;


		protected override void SetBody()
		{
			var template = Resources.Load<VisualTreeAsset>("NodeViews/LocalizedMessageNode");
			template.CloneTree(bodyContainer);

			messageKeyLabel = bodyContainer.Q<Label>("key");
			messageLabel = bodyContainer.Q<Label>("message");
			// UpdateValues();

			// optionList = bodyContainer.Q(classes: "option-list");
			// Data.Options.ForEach(AddOption);

			schedule.Execute(UpdateValues).Every(100);
		}

		private void AddOption(PortDefinition<BaseNode> option)
		{
			// var optionElement = new ChoiceOption(option);
			// optionList.Add(optionElement);
			// RegisterPort(optionElement.port, option.Guid);
		}

		private void RemoveOldEntries()
		{
			// var optionElements = bodyContainer.Query<ChoiceOption>().ToList();

			// var optionElementsToRemove = optionElements
			// 	.Where(x => Data.Options.ToList().All(y => y.Guid != x.portDefinition.Guid));

			// var edgesToRemove = new List<GraphElement>();

			// foreach (var element in optionElementsToRemove)
			// {
			// 	edgesToRemove.AddRange(element.port.connections);
			// 	element.RemoveFromHierarchy();
			// }

			// if (edgesToRemove.Count > 0)
			// 	GraphView.DeleteElements(edgesToRemove);
		}

		private void UpdateEntries()
		{
			// var optionElements = bodyContainer.Query<ChoiceOption>().ToList();
			// foreach (var el in optionElements)
			// 	el.Update();
		}

		private void AddNewEntries()
		{
			// var optionElements = bodyContainer.Query<ChoiceOption>().ToList();

			// Data.Options
			// 	.Where(x => optionElements.TrueForAll(y => y.portDefinition.Guid != x.Guid))
			// 	.ToList()
			// 	.ForEach(AddOption);
		}


		private void HandleNodeChange()
		{
			// var actorName = Data.UseActorProfile ? Data.Actor :
			// 	Data.ActorProfile != null ? Data.ActorProfile.DisplayName : "";

			// if (actorLabel.text != actorName)
			// 	actorLabel.text = actorName;

			if (messageLabel.text != Data.Message)
				messageLabel.text = Data.Message;

			RemoveOldEntries();
			UpdateEntries();
			AddNewEntries();
		}

		// We need to extend this method, so that when we delete the node, the edges attached to
		// each option are included in the list of elements to delete, for "graphViewChanged"
		public override void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFunc)
		{
			// base.CollectElements(collectedElementSet, conditionFunc);

			// var choicePorts = bodyContainer.Query<Port>().ToList();
			// collectedElementSet.UnionWith(choicePorts.SelectMany(port => port.connections));
		}

		private void UpdateValues()
		{
			if (Data.Key != messageKeyLabel.text)
				messageKeyLabel.text = Data.Key;

			if (Data.Message != messageLabel.text)
				messageLabel.text = Data.Message;
		}
	}
}