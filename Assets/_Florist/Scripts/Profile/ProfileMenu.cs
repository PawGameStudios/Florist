using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ProfileMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private List<Image> _avatars;

    void Start()
    {
        SetAvatars();
    }

    public void Open()
    {
        SetProfile();
        gameObject.SetActive(true);
    }

    public void OnCloseClicked()
    {
        gameObject.SetActive(false);
    }

    public void OnNameChanged()
    {
        SaveSystem.Inst.GeneralData.ChangePlayerName(_inputField.text);
    }

    public void OnAvatarClicked(int index)
    {
        SaveSystem.Inst.GeneralData.ChangeAvatar(index);
        SetProfile();
    }

    private void SetAvatars()
    {
        foreach (var avatar in _avatars)
        {
            avatar.gameObject.SetActive(false);
        }

        for (int i = 0; i < Configs.ProfileConfig.Avatars.Count; i++)
        {
            _avatars[i].sprite = Configs.ProfileConfig.Avatars[i];
            _avatars[i].gameObject.SetActive(true);
        }
    }

    private void SetProfile()
    {
        _inputField.text = SaveSystem.Inst.GeneralData.PlayerName;
        _levelText.text = $"{LocalizationManager.GetLocalizedText("level")}: {SaveSystem.Inst.GeneralData.CurrentDayIndex + 1}";
    }
}
