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

        public AudioSource MenuMusic;
        public AudioSource EspionageMusic;
        public AudioSource CombatMusic;

        private Music currentMusic = Music.Menu;

        private static BackgroundMusic instance;

        void Awake()
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

        }
    }
}