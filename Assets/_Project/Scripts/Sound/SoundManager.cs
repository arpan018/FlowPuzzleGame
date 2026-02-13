using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Game.Sounds
{
    public class SoundManager : MonoBehaviour
    {
        public enum SoundType
        {
            Button, Win,
            Tap, TileConnect, BulbOn
        }

        private static SoundManager scr;
        private AudioSource[] audioSource;
        public Sounds[] sounds;

        int soundCounts;

        void Start()
        {
            scr = this;
            soundCounts = sounds.Length;
            t = new float[soundCounts];
            canPlay = new bool[soundCounts];
            CreateAudioScource();
        }

        void Update()
        {
            SetTime();
            CheckCooldowns();
        }

        private void CheckCooldowns()
        {
            foreach (var s in sounds)
            {
                if (s.play_ordered && s.deltaCooldown > 0)
                {
                    s.deltaCooldown -= Time.deltaTime;
                    if (s.deltaCooldown <= 0)
                    {
                        s.deltaCooldown = 0f;
                        s.played_index = 0;
                    }
                }
            }
        }

        private void CreateAudioScource()
        {
            audioSource = new AudioSource[soundCounts];
            for (int i = 0; i < soundCounts; i++)
            {
                GameObject temp = new GameObject();
                temp.transform.SetParent(transform);
                audioSource[i] = temp.AddComponent<AudioSource>();
            }
        }

        private void OnValidate()
        {
            soundCounts = sounds.Length;
            for (int i = 0; i < soundCounts; i++)
            {
                sounds[i].soundName = sounds[i].sound.ToString();
            }
        }

        public static void PlaySound(SoundType sound, float volume = 1, float pitch = 1, float delay = 0)
        {
            // todo: return if music/sound on
            //if (!hasSound) return;

            if (scr.canPlay[sound.GetHashCode()])
            {
                if (delay == 0)
                {
                    scr._PlaySound(sound, volume, pitch);
                }
                else
                {
                    scr.StartCoroutine(scr.SoundDelay(sound, volume, pitch, delay));
                }
            }

        }

        private void _PlaySound(SoundType sound, float volume, float pitch)
        {
            AudioClip[] ac;
            ac = scr.GetClip(sound);
            scr.audioSource[sound.GetHashCode()].pitch = pitch;

            AudioClip clip = null;
            Sounds s = sounds.FirstOrDefault(x => x.sound == sound);
            if (s == null)
            {
                return;
            }
            if (s.audioClip.Length == 0)
            {
                return;
            }

            if (s.play_ordered)
            {
                clip = ac[Math.Min(s.played_index, ac.Length - 1)];
                s.played_index++;
                s.deltaCooldown = s.cooldown;
            }
            else
            {
                clip = ac[UnityEngine.Random.Range(0, ac.Length)];
            }

            scr.audioSource[sound.GetHashCode()].PlayOneShot(clip, s.volume);
            scr.canPlay[sound.GetHashCode()] = false;
        }

        public static void PlaySoundLoop(SoundType sound, float volume = 1f, float pitch = 1f)
        {
            // todo: return if music/sound on
            //if (!hasSound) return;

            var s = scr.sounds.FirstOrDefault(x => x.sound == sound);
            if (s == null || s.audioClip == null || s.audioClip.Length == 0)
                return;

            var src = scr.audioSource[sound.GetHashCode()];
            src.clip = s.audioClip[0];
            src.volume = s.volume * volume;
            src.pitch = pitch;
            src.loop = true;
            src.Play();
        }

        public static void StopSoundLoop(SoundType sound)
        {
            var src = scr.audioSource[sound.GetHashCode()];
            if (src.isPlaying)
            {
                src.loop = false;
                src.Stop();
            }
        }

        public static void StopSound(SoundType sound)
        {
            if (scr == null) return;

            var src = scr.audioSource[sound.GetHashCode()];

            if (src.isPlaying)
            {
                src.loop = false;
                src.Stop();
            }
        }

        IEnumerator SoundDelay(SoundType sound, float volume, float delay, float pitch)
        {
            yield return new WaitForSeconds(delay);
            _PlaySound(sound, volume, pitch);
        }

        private AudioClip[] GetClip(SoundType type)
        {
            for (int i = 0; i < soundCounts; i++)
            {
                if (sounds[i].sound == type)
                {
                    return sounds[i].audioClip;
                }
            }
            return null;
        }

        private float[] t;
        private bool[] canPlay;

        private void SetTime()
        {
            for (int i = 0; i < soundCounts; i++)
            {
                if (!canPlay[i])
                {
                    t[i] += Time.deltaTime;
                    if (t[i] >= 0.08f)
                    {
                        canPlay[i] = true;
                        t[i] = 0;
                    }
                }
            }
        }

        [System.Serializable]
        public class Sounds
        {
            [HideInInspector]
            public string soundName;
            public SoundType sound;
            public AudioClip[] audioClip;

            public bool play_ordered;
            public float cooldown;
            [Range(0, 1)]
            public float volume = 1;
            [HideInInspector] public int played_index;
            [HideInInspector] public float deltaCooldown;
        }
    }
}