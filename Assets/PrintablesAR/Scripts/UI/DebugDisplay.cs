using System;
using System.Collections.Generic;
using System.Linq;
using ToolBuddy.PrintablesAR.ARInteraction;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ToolBuddy.PrintablesAR.UI
{
    public class DebugDisplay : MonoBehaviour
    {
        private Application.Application _application;
        private ARSession _arSession;
        private ARPlaneManager _arPlaneManager;

        private readonly Lazy<GUIStyle> _buttonStyle = new Lazy<GUIStyle>(
            () => new GUIStyle(GUI.skin.button)
            {
                fontSize = 36
            }
        );

        private readonly Lazy<GUIStyle> _labelStyle = new Lazy<GUIStyle>(
            () => new GUIStyle
            {
                fontSize = 36,
                normal = { textColor = Color.white }
            }
        );

        private readonly Lazy<GUIStyle> _listStyle = new Lazy<GUIStyle>(
            () => new GUIStyle
            {
                fontSize = 28,
                normal = { textColor = Color.white }
            }
        );

        private static readonly Lazy<List<PlaneDetectionMode>> _planeDetectionModes = new Lazy<List<PlaneDetectionMode>>(
            () =>
                Enum.GetValues(typeof(PlaneDetectionMode)).Cast<PlaneDetectionMode>().ToList()
        );

        private void Awake()
        {
            _application = FindAnyObjectByType<Application.Application>();
            _arSession = FindAnyObjectByType<ARSession>();
            _arPlaneManager = FindAnyObjectByType<ARPlaneManager>();
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                DrawStates();
                DrawFPS();
                DrawSelectors();
                DrawTrackedPlanes();
            }
            GUILayout.EndVertical();
        }

        private void DrawStates()
        {
            ARInteractable arInteractable = FindAnyObjectByType<ARInteractable>();
            GUILayout.Label(
                $"States: {_application.State} - {arInteractable?.State}",
                _labelStyle.Value
            );
        }

        private void DrawFPS() =>
            GUILayout.Label(
                $"FPS: {1f / Time.smoothDeltaTime}/{UnityEngine.Application.targetFrameRate}",
                _labelStyle.Value
            );

        private void DrawSelectors()
        {
            if (GUILayout.Button(
                    $"FPS Match (current: {_arSession.matchFrameRateEnabled} / request {_arSession.matchFrameRateRequested})",
                    _buttonStyle.Value
                ))
                _arSession.matchFrameRateRequested = !_arSession.matchFrameRateRequested;

            if (GUILayout.Button(
                    $"Detection (current: {_arPlaneManager.currentDetectionMode} / request: {_arPlaneManager.requestedDetectionMode})",
                    _buttonStyle.Value
                ))
            {
                List<PlaneDetectionMode> detectionModes = _planeDetectionModes.Value;
                int nextIndex = (detectionModes.IndexOf(_arPlaneManager.currentDetectionMode) + 1) % detectionModes.Count;
                _arPlaneManager.requestedDetectionMode = detectionModes[nextIndex];
            }
        }

        private void DrawTrackedPlanes()
        {
            GUILayout.Label(
                "Tracked Planes:",
                _labelStyle.Value
            );
            foreach (ARPlane plane in _arPlaneManager.trackables)
                GUILayout.Label(
                    $"  -{plane.name.Substring(9, 7)}: {plane.trackingState} - {plane.pending} - {plane.isActiveAndEnabled} - {plane.subsumedBy}",
                    _listStyle.Value
                );
        }
    }
}