using Conversa.Runtime;
using Conversa.Runtime.Events;
using Conversa.Runtime.Nodes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Conversa.Editor
{
    public class ChoiceNodeInspector : BaseNodeInspector<ChoiceNode>
    {
        public ChoiceNodeInspector(ChoiceNode data, Conversation conversation) : base(data, conversation) { }

        protected override void SetBody()
        {
            var template = Resources.Load<VisualTreeAsset>("Inspectors/ChoiceNode");
            template.CloneTree(this);

            var actorField = this.Q<ActorField>();
            actorField.OnChange(HandleActorChange);
            actorField.SetValueWithoutNotify(data.Actor, data.UseActorProfile, data.ActorProfile);

            var messageInput = this.Q<TextField>("message");
            messageInput.RegisterValueChangedCallback(HandleUpdateMessage);
            messageInput.SetValueWithoutNotify(data.Message);
            messageInput.isDelayed = false;

            var key = this.Q<TextField>("key");
            key.RegisterValueChangedCallback(HandleUpdateKey);
            key.SetValueWithoutNotify(data.Key);
            key.isDelayed = false;

            var type1 = this.Q<EnumField>("type1");
            type1.RegisterValueChangedCallback(HandleUpdatePaseOption1);
            type1.SetValueWithoutNotify(data.ParseOption1);

            var type2 = this.Q<EnumField>("type2");
            type2.RegisterValueChangedCallback(HandleUpdatePaseOption2);
            type2.SetValueWithoutNotify(data.ParseOption2);

            var type3 = this.Q<EnumField>("type3");
            type3.RegisterValueChangedCallback(HandleUpdatePaseOption3);
            type3.SetValueWithoutNotify(data.ParseOption3);

            data.Options.ForEach(SetOption);

            this.Q<Button>(classes: "add-option").clickable.clicked += HandleAddOption;
        }

        private void HandleActorChange(string staticActor, bool useActorProfile, Actor actorProfile)
        {
            data.Actor = staticActor;
            data.UseActorProfile = useActorProfile;
            data.ActorProfile = actorProfile;
        }

        private void SetOption(PortDefinition<BaseNode> portDefinition)
        {
            var optionsWrapper = this.Q(classes: "option-list");

            var option = new ChoiceOptionForm(portDefinition);
            option.OnDelete.AddListener(() => HandleDeleteOption(option));
            optionsWrapper.Add(option);
        }

        private void HandleAddOption()
        {
            var newOption = new PortDefinition<BaseNode>(General.NewGuid(), "");
            data.Options.Add(newOption);
            SetOption(newOption);
        }

        private void HandleDeleteOption(ChoiceOptionForm form)
        {
            data.Options.Remove(form.portDefinition);
            form.RemoveFromHierarchy();
        }

        private void HandleUpdateMessage(ChangeEvent<string> evt) => data.Message = evt.newValue;
        private void HandleUpdateKey(ChangeEvent<string> evt) => data.Key = evt.newValue;
        private void HandleUpdatePaseOption1(ChangeEvent<System.Enum> evt) => data.ParseOption1 = (StringParseOptions)evt.newValue;
        private void HandleUpdatePaseOption2(ChangeEvent<System.Enum> evt) => data.ParseOption2 = (StringParseOptions)evt.newValue;
        private void HandleUpdatePaseOption3(ChangeEvent<System.Enum> evt) => data.ParseOption3 = (StringParseOptions)evt.newValue;
    }
}