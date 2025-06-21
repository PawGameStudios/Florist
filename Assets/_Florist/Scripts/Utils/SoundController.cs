using MelenitasDev.SoundsGood;
using UnityEngine;

public class SoundController : MonoSingleton<SoundController>
{
    public struct SoundParams
    {
        public bool Loop;
        public bool RandomClip;
        public float Volume;
        public bool RandomPitch;
    }

    [SerializeField] private Track _mainMusicTrack;
    private Sound[] _sounds;
    private Music _music;
    private int _currentSoundIndex = 0;
    private bool _enableCooldown = true;

    protected override void VirtualAwake()
    {
        _sounds = new Sound[5];
        _music = new(_mainMusicTrack);
    }

    public static void PlaySound(SFX sfx, float cooldown)
    {
        if (!SaveSystem.Inst.GeneralData.IsSoundOn)
            return;

        if (!Instance._enableCooldown)
            return;

        Instance._enableCooldown = false;
        Instance.Invoke(nameof(EnableCooldown), cooldown);

        Debug.Log($"#sound# Playing sound: {sfx}");

        int soundIndex = Instance._currentSoundIndex;
        Sound sound = Instance._sounds[soundIndex];

        if (sound == null)
        {
            sound = new Sound(sfx);
        }
        else
        {
            if (sound.Paused)
            {
                sound.Resume();
                return;
            }
        }

        sound.SetClip(sfx)
            .SetLoop(false)
            .SetRandomClip(false)
            .SetVolume(1)
            .SetSpatialSound(false)
            .Play(cooldown);

        Instance._currentSoundIndex = (soundIndex + 1) % Instance._sounds.Length;
    }

    public static void PlaySound(SFX sfx)
    {
        if (!SaveSystem.Inst.GeneralData.IsSoundOn)
            return;

        Debug.Log($"#sound# Playing sound: {sfx}");

        int soundIndex = Instance._currentSoundIndex;
        Sound sound = Instance._sounds[soundIndex];

        if (sound == null)
        {
            sound = new Sound(sfx);
        }
        else
        {
            if (sound.Paused)
            {
                sound.Resume();
                return;
            }
        }

        sound.SetClip(sfx)
            .SetLoop(false)
            .SetRandomClip(false)
            .SetVolume(1)
            .SetSpatialSound(false)
            .Play();

        Instance._currentSoundIndex = (soundIndex + 1) % Instance._sounds.Length;
    }

    public static void PlaySound(SFX sfx, SoundParams soundParams)
    {
        if (!SaveSystem.Inst.GeneralData.IsSoundOn)
            return;

        int soundIndex = Instance._currentSoundIndex;
        Sound sound = Instance._sounds[soundIndex];

        if (sound == null)
        {
            sound = new Sound(sfx);
        }
        else
        {
            if (sound.Paused)
            {
                sound.Resume();
                return;
            }
        }

        if (soundParams.RandomPitch)
        {
            sound.SetRandomPitch();
        }
        sound.SetClip(sfx)
            .SetLoop(soundParams.Loop)
            .SetRandomClip(soundParams.RandomClip)
            .SetVolume(soundParams.Volume)
            .SetSpatialSound(false)
            .Play();

        Instance._currentSoundIndex = (soundIndex + 1) % Instance._sounds.Length;
    }

    public static void PauseSounds()
    {
        for (int i = 0; i < Instance._sounds.Length; i++)
        {
            Sound sound = Instance._sounds[i];
            sound.Pause();
        }
    }

    public static void StopSounds()
    {
        for (int i = 0; i < Instance._sounds.Length; i++)
        {
            Sound sound = Instance._sounds[i];
            sound?.Stop();
        }
    }

    public static void PlayMusic(Track track, bool loop = false, bool randomClip = false, float volume = 1, float fadeInTime = 0, float fadeOutTime = 0)
    {
        return;

        if (!SaveSystem.Inst.GeneralData.IsMusicOn)
            return;

        Music music = Instance._music;

        if (music == null)
        {
            music = new Music(track);
        }
        else
        {
            if (music.Paused)
            {
                music.Resume();
                return;
            }

            if (music.Playing)
            {
                StopMusic();
            }
        }

        music.SetLoop(loop)
            .SetRandomClip(randomClip)
            .SetVolume(volume)
            .SetSpatialSound(false)
            .SetFadeOut(fadeOutTime)
            .Play(fadeInTime);
    }

    public static void PauseMusic()
    {
        Instance._music.Pause();
    }

    public static void StopMusic()
    {
        Instance._music.Stop();
    }

    public static void SetMusicEnabled(bool isEnabled)
    {
        if (isEnabled)
        {
            PlayMusic(Instance._mainMusicTrack);
        }
        else
        {
            StopMusic();
        }
    }

    public static void SetSoundEnabled(bool isEnabled)
    {
        if (!isEnabled)
            StopSounds();
    }

    private void EnableCooldown()
    {
        _enableCooldown = true;
    }
}
