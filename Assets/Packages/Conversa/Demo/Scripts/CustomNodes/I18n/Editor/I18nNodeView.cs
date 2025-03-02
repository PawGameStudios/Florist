using Conversa.Editor;
using Conversa.Runtime;

namespace Conversa.Demo
{
    public class I18nNodeView : BaseNodeView<I18nNode>
    {
        protected override string Title => "I18n";

        // Constructors

        public I18nNodeView(Conversation conversation) : base(new I18nNode(), conversation) { }

        public I18nNodeView(I18nNode data, Conversation conversation) : base(data, conversation) { }

        protected override void SetBody()
        {
            // TEMPLATE: Write your view body here
        }

    }
}