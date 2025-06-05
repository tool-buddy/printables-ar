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
        private readonly GameObject _interactibleGameObject;

        [NotNull]
        private AudioClip _placementSound;

        [NotNull]
        private AudioClip _dragUnlockSound;

        [NotNull]
        private readonly ARInteractibleStateMachine _interactibleStateMachine;

        [NotNull]
        private AudioSource _audioSource;

        private const float _volumeMultiplier = 8;

        public ARInteractibleFeedbackProvider(
            [NotNull] GameObject interactibleGameObject,
            [NotNull] ARInteractibleStateMachine interactibleStateMachine)
        {
            _interactibleGameObject = interactibleGameObject ?? throw new ArgumentNullException(nameof(interactibleGameObject));
            _interactibleStateMachine =
                interactibleStateMachine ?? throw new ArgumentNullException(nameof(interactibleStateMachine));

            if (interactibleGameObject.gameObject.TryGetComponent(out _audioSource) == false)
                _audioSource = interactibleGameObject.gameObject.AddComponent<AudioSource>();

            LoadSounds();
            ListenToStateChanges();
            PlaySpawnFeedback();
        }

        private void LoadSounds()
        {
            _placementSound = Resources.Load<AudioClip>("Sounds/impactSoft_heavy_003");
            if (_placementSound == null)
                throw new FileNotFoundException("Placement sound file not found");

            _dragUnlockSound = Resources.Load<AudioClip>("Sounds/switch3");
            if (_dragUnlockSound == null)
                throw new FileNotFoundException("Drag unlocking sound file not found");
        }

        private void ListenToStateChanges()
        {
            _interactibleStateMachine.Configure(ARInteractibleStateMachine.TouchState.Placing)
                .OnExit(
                    transition =>
                    {
                        if (transition.Trigger == ARInteractibleStateMachine.Trigger.PlacementSuccess)
                            PlayPlacementFeedback();
                        //todo add a feedback for placement failure
                    }
                );

            _interactibleStateMachine.Configure(ARInteractibleStateMachine.TouchState.XYDragging)
                .OnEntry(
                    PlayDragUnlockingFeedback
                );
        }

        private void PlayDragUnlockingFeedback() =>
            _audioSource.PlayOneShot(
                _dragUnlockSound,
                _volumeMultiplier * 0.8f
            );

        private void PlaySpawnFeedback() =>
            PlayPlacementFeedback();

        private void PlayPlacementFeedback()
        {
            DOTween.Kill(
                _interactibleGameObject,
                true
            );

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
                () =>
                {
                    _audioSource.PlayOneShot(
                        _placementSound,
                        _volumeMultiplier * 3f
                    );
                }
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
    }
}