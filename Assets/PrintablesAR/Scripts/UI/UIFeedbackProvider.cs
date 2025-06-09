using System;
using System.IO;
using JetBrains.Annotations;
using ToolBuddy.PrintablesAR.UI;
using UnityEngine;

namespace Assets.PrintablesAR.Scripts.UI
{
    public class UIFeedbackProvider
    {
        [NotNull]
        private AudioClip _buttonClickSound;

        [NotNull]
        private AudioSource _audioSource;

        private readonly MainUI _ui;

        private const float _volumeMultiplier = 5;

        public UIFeedbackProvider(
            [NotNull] MainUI ui,
            [NotNull] AudioSource uiAudioSource)
        {
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));
            _audioSource = uiAudioSource ?? throw new ArgumentNullException(nameof(uiAudioSource));

            _ui.ButtonClicked += OnButtonClicked;
            LoadSounds();
        }

        private void OnButtonClicked()
        {
            _audioSource.PlayOneShot(
                _buttonClickSound,
                _volumeMultiplier
            );
        }

        private void LoadSounds()
        {
            _buttonClickSound = Resources.Load<AudioClip>("Sounds/mouseclick1");
            if (_buttonClickSound == null)
                throw new FileNotFoundException("Button click sound file not found");
        }
    }
}