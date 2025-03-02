using Conversa.Runtime;
using Conversa.Runtime.Nodes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Conversa.Editor
{
    public class MessageNodeInspector : BaseNodeInspector<MessageNode>
    {
        public MessageNodeInspector(MessageNode data, Conversation conversation) : base(data, conversation) { }

        protected override void SetBody()
        {
            var template = Resources.Load<VisualTreeAsset>("Inspectors/MessageNode");
            template.CloneTree(this);


            var actorField = this.Q<ActorField>();
            actorField.OnChange(HandleActorChange);
            actorField.SetValueWithoutNotify(data.Actor, data.UseActorProfile, data.ActorProfile);

            var inputMessage = this.Q<TextField>("body");
            inputMessage.RegisterValueChangedCallback(UpdateMessage);
            inputMessage.SetValueWithoutNotify(data.Message);
            inputMessage.isDelayed = true;
        }

        private void HandleActorChange(string staticActor, bool useActorProfile, Actor actorProfile)
        {
            data.Actor = staticActor;
            data.UseActorProfile = useActorProfile;
            data.ActorProfile = actorProfile;
        }

        private void UpdateMessage(ChangeEvent<string> evt)
        {
            RegisterUndoStep();
            data.Message = evt.newValue;
        }
    }

    public class LocalizedMessageNodeInspector : BaseNodeInspector<LocalizedMessageNode>
    {
        public LocalizedMessageNodeInspector(LocalizedMessageNode data, Conversation conversation) : base(data, conversation) { }

        protected override void SetBody()
        {
            var template = Resources.Load<VisualTreeAsset>("Inspectors/LocalizedMessageNode");
            template.CloneTree(this);

            var inputMessage = this.Q<TextField>("body");
            inputMessage.RegisterValueChangedCallback(UpdateMessage);
            inputMessage.SetValueWithoutNotify(data.Message);
            inputMessage.isDelayed = false;

            var key = this.Q<TextField>("key");
            key.RegisterValueChangedCallback(UpdateKey);
            key.SetValueWithoutNotify(data.Key);
            key.isDelayed = false;

            // this.Q<Button>(classes: "add-option").clickable.clicked += HandleAddOption;
        }


        private void UpdateMessage(ChangeEvent<string> evt)
        {
            RegisterUndoStep();
            data.Message = evt.newValue;
        }

        private void UpdateKey(ChangeEvent<string> evt)
        {
            RegisterUndoStep();
            data.Key = evt.newValue;
        }

        // private void HandleAddOption()
        // {
        //     var newOption = new PortDefinition<BaseNode>(General.NewGuid(), "");
        //     data.Options.Add(newOption);
        //     SetOption(newOption);
        // }

        // private void SetOption(PortDefinition<BaseNode> portDefinition)
        // {
        //     var optionsWrapper = this.Q(classes: "option-list");

        //     var option = new ChoiceOptionForm(portDefinition);
        //     option.OnDelete.AddListener(() => HandleDeleteOption(option));
        //     optionsWrapper.Add(option);
        // }

        // private void HandleDeleteOption(ChoiceOptionForm form)
        // {
        //     data.Options.Remove(form.portDefinition);
        //     form.RemoveFromHierarchy();
        // }

    }
}