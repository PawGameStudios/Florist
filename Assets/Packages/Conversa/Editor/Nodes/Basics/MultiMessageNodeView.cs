using Conversa.Runtime;
using Conversa.Runtime.Nodes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Conversa.Editor
{
	public class MultiMessageNodeView : BaseNodeView<MultiMessageNode>
	{
		protected override string Title => "Multi Message";

		// Constructors

		public MultiMessageNodeView(Conversation conversation)
			: base(new MultiMessageNode(), conversation) { }

		public MultiMessageNodeView(MultiMessageNode data, Conversation conversation) : base(data, conversation) { }

		private Label actorLabel;
		private VisualElement messageContainer;

		// Methods

		protected override void SetBody()
		{
			var template = Resources.Load<VisualTreeAsset>("NodeViews/MultiMessageNode");
			template.CloneTree(bodyContainer);

			actorLabel = bodyContainer.Q<Label>("actor");
			messageContainer = bodyContainer.Q<VisualElement>("messages");

			schedule.Execute(UpdateValues).Every(100);
		}

		private void RedrawMessages()
		{
			try
			{
				messageContainer.Clear();

				// If no messages, show warning of no messages
				if (Data.Messages.Count == 0)
				{
					var messageLabel = new Label("No messages");
					messageLabel.AddToClassList("message");
					messageContainer.Add(messageLabel);
					return;
				}

				for (var i = 0; i < Data.Messages.Count; i++)
				{
					var message = Data.Messages[i];
					var messageLabel = new Label(message);
					messageLabel.AddToClassList("message");
					messageLabel.AddToClassList("text-wrap");
					messageLabel.AddToClassList("mb-10");
					messageContainer.Add(messageLabel);
				}
			}

			catch (System.Exception)
			{
				Debug.Log("Error");
				throw;
			}
		}

		private void UpdateValues()
		{
			if (actorLabel.text != Data.ActorName)
				actorLabel.text = Data.ActorName;

			RedrawMessages();
		}
	}
}