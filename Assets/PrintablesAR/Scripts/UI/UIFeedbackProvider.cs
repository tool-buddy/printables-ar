using System;
using JetBrains.Annotations;
using ToolBuddy.PrintablesAR.Sound;

namespace ToolBuddy.PrintablesAR.UI
{
    public class UIFeedbackProvider
    {
        [NotNull]
        private readonly AudioPlayer _audioPlayer;

        public UIFeedbackProvider(
            [NotNull] MainUI ui,
            [NotNull] AudioPlayer audioPlayer)
        {
            _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
            ui.ButtonClicked += OnButtonClicked;
        }

        private void OnButtonClicked() =>
            _audioPlayer.PlayOneShot(
                Sounds.ButtonClicked,
                true
            );
    }
}