using System;
using System.IO;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public class ARInteractibleFeedbackProvider
    {
        [NotNull]
        private readonly Transform _targetTransform;

        [NotNull]
        private readonly AudioClip _placementSound;

        [NotNull]
        private readonly ARInteractibleStateMachine _interactibleStateMachine;

        public ARInteractibleFeedbackProvider(
            [NotNull] Transform targetTransform,
            [NotNull] ARInteractibleStateMachine interactibleStateMachine)
        {
            _targetTransform = targetTransform ?? throw new ArgumentNullException(nameof(targetTransform));
            _interactibleStateMachine =
                interactibleStateMachine ?? throw new ArgumentNullException(nameof(interactibleStateMachine));

            _placementSound = Resources.Load<AudioClip>("Sounds/impactSoft_heavy_003");
            if (_placementSound == null)
                throw new FileNotFoundException("Placement sound file not found");

            _interactibleStateMachine.Configure(ARInteractibleStateMachine.TouchState.Placing)
                .OnEntry(
                    PlayPlacementFeedback
                );

            PlaySpawnFeedback();
        }

        private void PlaySpawnFeedback() =>
            PlayPlacementFeedback();

        private void PlayPlacementFeedback()
        {
            DOTween.Kill(
                _targetTransform,
                true
            );

            Vector3 originalScale = _targetTransform.localScale;

            const float duration = 0.25f;
            const float squashFactorY = 0.7f;
            const float stretchFactorXz = 1.2f;
            const float reboundFactorY = 1.15f;
            const float reboundFactorXz = 0.9f;

            Sequence sequence = DOTween.Sequence();
            sequence.SetTarget(_targetTransform);
            // Play sound
            sequence.OnStart(
                () =>
                {
                    AudioSource.PlayClipAtPoint(
                        _placementSound,
                        _targetTransform.position
                    );
                }
            );
            // Squash: Y down, XZ up
            sequence.Append(
                _targetTransform.DOScale(
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
                _targetTransform.DOScale(
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
                _targetTransform.DOScale(
                    originalScale,
                    duration * 0.3f
                ).SetEase(Ease.InExpo)
            );

            sequence.Play();
        }
    }
}