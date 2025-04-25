using System.Collections.Generic;
using JetBrains.Annotations;
using ToolBuddy.PrintablesAR.UI;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    /// <summary>
    ///     A wrapper to ARRaycastManager but takes into account UI obstruction.
    /// </summary>
    public class Raycaster
    {
        [NotNull]
        private readonly ARRaycastManager _arRaycastManager;

        [NotNull]
        private readonly List<ARRaycastHit> _hits = new List<ARRaycastHit>();

        [NotNull]
        private readonly MainUI _ui;

        public Raycaster(
            [NotNull] ARRaycastManager arRaycastManager,
            [NotNull] MainUI ui)
        {
            _arRaycastManager = arRaycastManager;
            _ui = ui;
        }

        public bool TryGetHit(
            Vector2 touchPosition,
            out Pose hitPose)
        {
            if (_ui.IsFingerOnUI(touchPosition))
            {
                hitPose = default;
                return false;
            }

            if (!_arRaycastManager.Raycast(
                    touchPosition,
                    _hits,
                    TrackableType.PlaneWithinPolygon
                ))
            {
                hitPose = default;
                return false;
            }

            hitPose = _hits[0].pose;
            return true;
        }

        public bool IsFingerOnUI(
            Vector2 fingerScreenPosition) =>
            _ui.IsFingerOnUI(fingerScreenPosition);
    }
}