using Conversa.Runtime;
using Conversa.Runtime.Nodes;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Conversa.Editor
{
	public class MultiMessageNodeInspector : BaseNodeInspector<MultiMessageNode>
	{
		public MultiMessageNodeInspector(MultiMessageNode data, Conversation conversation) : base(data, conversation) { }

		protected override void SetBody()
		{
			var template = Resources.Load<VisualTreeAsset>("Inspectors/MultiMessageNode");
			template.CloneTree(this);

			var actorField = this.Q<ObjectField>("actor");
			actorField.objectType = typeof(Actor);
			actorField.RegisterValueChangedCallback(HandleActorChange);
			actorField.SetValueWithoutNotify(data.Actor);

			// var inputMessage = this.Q<TextField>("body");
			// inputMessage.RegisterValueChangedCallback(HandleMessageChange);
			// inputMessage.SetValueWithoutNotify(data.Message);
			// inputMessage.isDelayed = true;

			// Set up messages
			RedrawMessages();

			// Add message button
			this.Q<Button>(classes: "add-option").clickable.clicked += HandleAddOption;
		}

		private void RedrawMessages()
		{
			var messagesWrapper = this.Q(classes: "messages");
			messagesWrapper.Clear();

			// For each message, call Draw message
			for (var i = 0; i < data.Messages.Count; i++)
			{
				var message = data.Messages[i];
				DrawMessage(message, i);
			}
		}

		private void DrawMessage(string message, int index)
		{
			var messagesWrapper = this.Q(classes: "messages");

			// Container element
			var messageContainer = new VisualElement();
			messageContainer.AddToClassList("message-container");
			messageContainer.AddToClassList("py-10");
			messagesWrapper.Add(messageContainer);

			var messageField = new TextField();
			messageField.RegisterValueChangedCallback(evt => HandleMessageChange(evt.newValue, index));
			messageField.SetValueWithoutNotify(message);
			messageField.AddToClassList("text-wrap");
			messageField.multiline = true;
			messageField.isDelayed = true;
			messageContainer.Add(messageField);

			// Add delete button
			var deleteButton = new Button(() => HandleDeleteMessage(index));
			deleteButton.AddToClassList("delete-option");
			deleteButton.AddToClassList("px-10");
			deleteButton.AddToClassList("py-5");
			deleteButton.text = "Delete";

			var moveUpButton = new Button(() =>
			{
				if (index == 0) return;
				HandleMoveToIndex(index, index - 1);
			});

			moveUpButton.AddToClassList("move-up");
			moveUpButton.AddToClassList("px-10");
			moveUpButton.AddToClassList("py-5");
			moveUpButton.text = "Up";

			// Buttons to move one down
			var moveDownButton = new Button(() =>
			{
				if (index == data.Messages.Count - 1) return;
				HandleMoveToIndex(index, index + 1);
			});
			moveDownButton.AddToClassList("move-down");
			moveDownButton.AddToClassList("px-10");
			moveDownButton.AddToClassList("py-5");
			moveDownButton.text = "Down";

			// Add buttons to container
			var buttonContainer = new VisualElement();
			buttonContainer.AddToClassList("mt-5");
			buttonContainer.style.flexDirection = FlexDirection.Row;
			buttonContainer.Add(deleteButton);
			buttonContainer.Add(moveUpButton);
			buttonContainer.Add(moveDownButton);
			messageContainer.Add(buttonContainer);

		}

		//
		// Handlers
		//

		private void HandleAddOption()
		{
			var newMessage = "";
			data.Messages.Add(newMessage);
			RedrawMessages();
		}

		private void HandleActorChange(ChangeEvent<Object> evt)
		{
			RegisterUndoStep();
			data.Actor = evt.newValue as Actor;
		}

		private void HandleMessageChange(string message, int index)
		{
			RegisterUndoStep();
			data.Messages[index] = message;
		}

		private void HandleDeleteMessage(int index)
		{
			RegisterUndoStep();
			data.Messages.RemoveAt(index);
			RedrawMessages();
		}

		private void HandleMoveToIndex(int index, int newIndex)
		{
			RegisterUndoStep();
			var message = data.Messages[index];
			data.Messages.RemoveAt(index);
			data.Messages.Insert(newIndex, message);
			RedrawMessages();
		}
	}
}