using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core
{
    public class BackgroundMusic : MonoBehaviour
    {
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

        private float cooldown = 0f;
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

        public void SetMusic(Music newMusic, float newTransitionSpeed=1f)
        {
            GetAudioSource(newMusic).time = 0f;
            currentMusic = newMusic;
            transitionSpeed = newTransitionSpeed;
        }

        public void StartCombatMusic()
        {
            cooldown = 25f;

            if (currentMusic != Music.Combat)
            {
                SetMusic(Music.Combat);
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
            if (cooldown > 0f)
            {
                cooldown -= Time.deltaTime;

                if (cooldown <= 0f)
                {
                    SetMusic(Music.Espionage, 0.2f);
                }
            }

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
                source.volume = Mathf.Max(0f, source.volume - transitionSpeed  * Time.deltaTime);
            }
            else if (currentMusic == music && source.volume < 1f)
            {
                source.volume = Mathf.Min(1f, source.volume + transitionSpeed  * Time.deltaTime);
            }
        }
    }
}