using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Profile : MonoBehaviour
{
    [SerializeField] private Image _frameImage;
    [SerializeField] private Image _avatarImage;

    void OnEnable()
    {
        SetProfile();
        GeneralData.AvatarChanged += SetProfile;
    }

    void OnDisable()
    {
        GeneralData.AvatarChanged -= SetProfile;
    }

    public void SetProfile()
    {
        int selectedAvatar = SaveSystem.Inst.GeneralData.SelectedAvatarIndexInConfig;
        int selectedFrame = SaveSystem.Inst.GeneralData.SelectedFrameIndexInConfig;

        _avatarImage.sprite = Configs.ProfileConfig.Avatars[selectedAvatar];
        _frameImage.sprite = Configs.ProfileConfig.Frames[selectedFrame];
    }

    public void OnProfileClicked()
    {
        References.ProfileMenu.Open();
        HapticsController.PlayButtonHaptic();
    }
}
