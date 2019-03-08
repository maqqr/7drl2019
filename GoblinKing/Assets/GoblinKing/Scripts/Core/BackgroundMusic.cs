using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core
{
    public class BackgroundMusic : MonoBehaviour
    {
        [System.Serializable]
        public struct SoundEffectClip
        {
            public string soundname;
            public AudioClip clip;
        }

        public SoundEffectClip[] soundEffects;

        public void PlaySoundEffectAt(string sound, Vector3 position)
        {
            for (int i = 0; i < soundEffects.Length; i++)
            {
                if (soundEffects[i].soundname == sound)
                {
                    AudioSource.PlayClipAtPoint(soundEffects[i].clip, position, 0.3f);
                    return;
                }
            }
            Debug.LogError("No sound effect \"" + sound + "\"");
        }

        public enum Music
        {
            Menu, Espionage, Combat
        }

        [SerializeField]
        private AudioSource MenuMusic = null;
        [SerializeField]
        private AudioSource EspionageMusic = null;
        [SerializeField]
        private AudioSource CombatMusic = null;

        private float transitionSpeed = 1f;

        [SerializeField]
        private Music currentMusic = Music.Menu;

        private static BackgroundMusic instance;

        public static BackgroundMusic Instance
        {
            get
            {
                return instance;
            }
        }

        public void SetMusic(Music newMusic, float newTransitionSpeed = 1f, bool forceChange = false)
        {
            if (currentMusic != newMusic || forceChange)
            {
                GetAudioSource(newMusic).time = 0f;
                currentMusic = newMusic;
                transitionSpeed = newTransitionSpeed;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                instance = this;
            }
            DontDestroyOnLoad(this.gameObject);

            MenuMusic.volume = 0f;
            EspionageMusic.volume = 0f;
            CombatMusic.volume = 0f;
        }

        void Update()
        {
            UpdateVolume(MenuMusic, Music.Menu);
            UpdateVolume(EspionageMusic, Music.Espionage);
            UpdateVolume(CombatMusic, Music.Combat);
        }

        private AudioSource GetAudioSource(Music music)
        {
            if (music == Music.Menu)
            {
                return MenuMusic;
            }
            if (music == Music.Espionage)
            {
                return EspionageMusic;
            }
            if (music == Music.Combat)
            {
                return CombatMusic;
            }
            return null;
        }

        private void UpdateVolume(AudioSource source, Music music)
        {
            if (currentMusic != music && source.volume > 0f)
            {
                source.volume = Mathf.Max(0f, source.volume - transitionSpeed * Time.deltaTime);
            }
            else if (currentMusic == music && source.volume < 1f)
            {
                source.volume = Mathf.Min(1f, source.volume + transitionSpeed * Time.deltaTime);
            }
        }
    }
}