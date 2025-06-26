using System;
using JetBrains.Annotations;
using ToolBuddy.PrintablesAR.Sound;

namespace ToolBuddy.PrintablesAR.UI
{
    /// <summary>
    /// Provides audio feedback for UI interactions.
    /// </summary>
    public class UIFeedbackProvider
    {
        [NotNull]
        private readonly AudioPlayer _audioPlayer;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIFeedbackProvider"/> class.
        /// </summary>
        /// <param name="ui">The main UI.</param>
        /// <param name="audioPlayer">The audio player for sound feedback.</param>
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