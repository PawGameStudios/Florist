using Conversa.Editor;
using Conversa.Runtime;

namespace Conversa.Demo
{
    public class I18nNodeMenuModifier
    {
        [NodeMenuModifier]
        private static void ModifyMenu(NodeMenuTree tree, Conversation conversation)
        {
            tree.AddMenuEntry<I18nNodeView>("I18n", 1);
        }
    }
}