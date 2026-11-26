using UnityEngine;

namespace Deadzone.Core
{
    /// <summary>Audio foundation: one music source, one SFX source, volume control. No clips yet.</summary>
    public sealed class AudioService : MonoBehaviour
    {
        private AudioSource music;
        private AudioSource sfx;

        public float MusicVolume { get; private set; } = 0.7f;
        public float SfxVolume { get; private set; } = 1f;

        private void Awake()
        {
            music = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.playOnAwake = false;
            music.volume = MusicVolume;

            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.volume = SfxVolume;
        }

        public void SetVolumes(float musicVolume, float sfxVolume)
        {
            MusicVolume = Mathf.Clamp01(musicVolume);
            SfxVolume = Mathf.Clamp01(sfxVolume);
            music.volume = MusicVolume;
            sfx.volume = SfxVolume;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || music.clip == clip) return;
            music.clip = clip;
            music.Play();
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip != null) sfx.PlayOneShot(clip, SfxVolume);
        }
    }
}
