using System;
using Assets.PrintablesAR.Scripts.Sound;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public class ARInteractibleFeedbackProvider
    {
        [NotNull]
        private readonly GameObject _interactibleGameObject;

        [NotNull]
        private readonly ARInteractibleStateMachine _interactibleStateMachine;

        private readonly AudioPlayer _audioPlayer;


        public ARInteractibleFeedbackProvider(
            [NotNull] GameObject interactibleGameObject,
            [NotNull] ARInteractibleStateMachine interactibleStateMachine,
            [NotNull] AudioPlayer audioPlayer)
        {
            _interactibleGameObject = interactibleGameObject ?? throw new ArgumentNullException(nameof(interactibleGameObject));
            _interactibleStateMachine =
                interactibleStateMachine ?? throw new ArgumentNullException(nameof(interactibleStateMachine));

            _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));

            ListenToStateChanges();
            Touch.onFingerMove += OnFingerMove;

            PlaySpawnFeedback();
        }


        private void ListenToStateChanges()
        {
            _interactibleStateMachine.Configure(ARInteractibleStateMachine.TouchState.Placing)
                .OnExit(
                    transition => PlayPlacementFeedback(transition.Trigger)
                );

            _interactibleStateMachine.Configure(ARInteractibleStateMachine.TouchState.XYDragging)
                .OnEntry(
                    PlayDragUnlockingFeedback
                );
        }

        private void PlayDragUnlockingFeedback() =>
            _audioPlayer.PlayOneShot(
                Sounds.DraggingUnlocked,
                true
            );

        private void PlaySpawnFeedback() =>
            PlaySuccessfulPlacementFeedback();

        private void PlayPlacementFeedback(
            ARInteractibleStateMachine.Trigger transitionTrigger)
        {
            DOTween.Kill(
                _interactibleGameObject,
                true
            );

            switch (transitionTrigger)
            {
                case ARInteractibleStateMachine.Trigger.PlacementSuccess:
                    PlaySuccessfulPlacementFeedback();
                    break;
                case ARInteractibleStateMachine.Trigger.PlacementFail:
                    PlayFailedPlacementFeedback();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(transitionTrigger),
                        transitionTrigger,
                        null
                    );
            }
        }

        private void PlaySuccessfulPlacementFeedback()
        {
            Transform transform = _interactibleGameObject.transform;
            Vector3 originalScale = transform.localScale;

            const float duration = 0.25f;
            const float squashFactorY = 0.7f;
            const float stretchFactorXz = 1.2f;
            const float reboundFactorY = 1.15f;
            const float reboundFactorXz = 0.9f;

            Sequence sequence = DOTween.Sequence();
            sequence.SetTarget(_interactibleGameObject);
            // Play sound
            sequence.OnStart(
                () => _audioPlayer.PlayOneShot(
                    Sounds.SuccessfulPlacement,
                    true
                )
            );
            // Squash: Y down, XZ up
            sequence.Append(
                transform.DOScale(
                    new Vector3(
                        originalScale.x * stretchFactorXz,
                        originalScale.y * squashFactorY,
                        originalScale.z * stretchFactorXz
                    ),
                    duration * 0.4f
                ).SetEase(Ease.OutExpo)
            );
            // Stretch (rebound): Y up (beyond original), XZ down (slightly below original)
            sequence.Append(
                transform.DOScale(
                    new Vector3(
                        originalScale.x * reboundFactorXz,
                        originalScale.y * reboundFactorY,
                        originalScale.z * reboundFactorXz
                    ),
                    duration * 0.3f
                ).SetEase(Ease.InOutSine)
            );
            // Settle: Return to original scale
            sequence.Append(
                transform.DOScale(
                    originalScale,
                    duration * 0.3f
                ).SetEase(Ease.InExpo)
            );

            sequence.Play();
        }

        private void PlayFailedPlacementFeedback() =>
            _audioPlayer.PlayOneShot(
                Sounds.FailedPlacement,
                true
            );

        private void OnFingerMove(
            Finger obj)
        {
            if (_audioPlayer == null)
                return;

            if (_interactibleStateMachine.IsInState(ARInteractibleStateMachine.TouchState.Dragging)
                || _interactibleStateMachine.IsInState(ARInteractibleStateMachine.TouchState.Pinching))
                _audioPlayer.PlayOneShot(
                    Sounds.Dragging,
                    false
                );
        }
    }
}