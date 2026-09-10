using TMPro;
using UnityEngine;
namespace Florist.Merge
{
    public sealed class MergeLocalizedLabel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private string localizationKey;
        private void OnEnable()
        {
            if (targetText != null) targetText.text = MergeLocalization.Text(localizationKey);
        }
    }
}
