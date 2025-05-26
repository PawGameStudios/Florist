using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ProfileConfig", menuName = "Paw/Configs/Profile")]
public class ProfileConfig : SerializedScriptableObject
{
    public List<Sprite> Avatars;
    public List<Sprite> Frames;
}
