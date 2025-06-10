using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.PrintablesAR.Scripts.Sound
{
    /// <summary>
    /// Provides functionality for managing and playing audio clips associated with specific sound events.
    /// </summary>
    public class AudioPlayer
    {
        private const float _volumeMultiplier = 8;
        private Dictionary<Sounds, AudioClip[]> _soundClips;
        private AudioSource _audioSource;
        private Dictionary<Sounds, float> _soundVolumes;
        [CanBeNull]
        private AudioClip _lastPlayedClip;

        public AudioPlayer(
            [NotNull] AudioSource audioSource)
        {
            _audioSource = audioSource ?? throw new ArgumentNullException(nameof(audioSource));
            _soundClips = new Dictionary<Sounds, AudioClip[]>();
        }

        public void LoadSounds()
        {
            _soundClips[Sounds.SuccessfulPlacement] = new[] { Resources.Load<AudioClip>("Sounds/impactSoft_heavy_003") };
            _soundClips[Sounds.FailedPlacement] = new[] { Resources.Load<AudioClip>("Sounds/impactBell_heavy_003") };
            _soundClips[Sounds.DraggingUnlocked] = new[] { Resources.Load<AudioClip>("Sounds/switch3") };
            _soundClips[Sounds.Dragging] = new[]
            {
                Resources.Load<AudioClip>("Sounds/broom-sweep-106601 A"), Resources.Load<AudioClip>("Sounds/broom-sweep-106601 B")
            };
            _soundClips[Sounds.ButtonClicked] = new[] { Resources.Load<AudioClip>("Sounds/mouseclick1") };

            if (_soundClips.ContainsValue(null))
                throw new FileNotFoundException(
                    "One or more sound files could not be loaded. Please check the Resources/Sounds directory."
                );

            _soundVolumes = new Dictionary<Sounds, float>
            {
                { Sounds.SuccessfulPlacement, 3f },
                { Sounds.FailedPlacement, 0.6f },
                { Sounds.DraggingUnlocked, 0.8f },
                { Sounds.Dragging, 0.5f },
                { Sounds.ButtonClicked, 0.6f }
            };
        }

        /// <summary>
        /// Plays a single instance of the specified sound effect.
        /// </summary>
        /// <param name="sound">The sound effect to play.</param>
        /// <param name="allowSoundReplay">A value indicating whether the same sound can be played again if it is already playing due to the last call to this method.</param>
        public void PlayOneShot(
            Sounds sound,
            bool allowSoundReplay)
        {
            AudioClip[] soundClips = _soundClips[sound];

            // todo bug _lastPlayedClip stores only the last played clip. Having _lastPlayedClip being equal to ClipA does NOT mean that clipA is no currently playing. It can be playing because of the call before last one.
            // With the current and expected usage of sound clips, this leads to Dragging being played again after DraggingUnlocked is played. This is no big deal, but needs to be addressed if expected usage of sound clips changes.
            if (allowSoundReplay == false
                && _audioSource.isPlaying
                && soundClips.Contains(_lastPlayedClip))
                return;

            AudioClip audioClip = soundClips[Random.Range(
                0,
                soundClips.Length
            )];

            _audioSource.PlayOneShot(
                audioClip,
                _soundVolumes[sound] * _volumeMultiplier
            );

            _lastPlayedClip = audioClip;
        }
    }
}