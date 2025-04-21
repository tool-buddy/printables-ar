using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Object = UnityEngine.Object;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public static class TransformManipulator
    {
        private static Lazy<ARRaycastManager> ArRaycastManager =>
            new Lazy<ARRaycastManager>(
                Object.FindFirstObjectByType<ARRaycastManager>
            );

        private static readonly List<ARRaycastHit> _hits = new List<ARRaycastHit>();

        public static void Rotate(
            Touch firstTouch,
            Transform target,
            Quaternion initialRotation,
            float rotationSensitivity,
            Vector3 cameraPosition)
        {
            Vector2 dragDelta = firstTouch.screenPosition - firstTouch.startScreenPosition;

            float yawAmount = -dragDelta.x * rotationSensitivity;
            float pitchAmount = dragDelta.y * rotationSensitivity;

            Vector3 initialUp = initialRotation * Vector3.up;

            Quaternion yawRotation = Quaternion.AngleAxis(
                yawAmount,
                initialUp
            );

            //todo better handling for objects which up is aiming towards the camera
            Quaternion pitchRotation = Quaternion.AngleAxis(
                pitchAmount,
                Vector3.Cross(
                    cameraPosition - target.position,
                    initialUp
                )
            );

            target.rotation = pitchRotation * yawRotation * initialRotation;
        }

        public static void Scale(
            Touch firstTouch,
            Touch secondTouch,
            Transform target,
            Vector3 initialScale)
        {
            float currentPinchDistance = Vector2.Distance(
                firstTouch.screenPosition,
                secondTouch.screenPosition
            );

            float initialPinchDistance = Vector2.Distance(
                firstTouch.startScreenPosition,
                secondTouch.startScreenPosition
            );

            target.localScale = initialScale * (currentPinchDistance / initialPinchDistance);
        }

        public static bool TryPlace(
            Finger finger,
            Transform target,
            Vector3 cameraPosition)
        {
            if (!ArRaycastManager.Value.Raycast(
                    finger.screenPosition,
                    _hits,
                    TrackableType.PlaneWithinPolygon
                ))
                return false;

            Pose hitPose = _hits[0].pose;
            target.position = hitPose.position;
            target.rotation = GetPlacementOrientation(
                hitPose,
                cameraPosition
            );

            return true;
        }

        private static Quaternion GetPlacementOrientation(
            Pose hitPose,
            Vector3 cameraPosition)
        {
            Vector3 cameraLookingDirection = Vector3.ProjectOnPlane(
                cameraPosition - hitPose.position,
                hitPose.up
            );

            Quaternion result;
            if (cameraLookingDirection.sqrMagnitude > 0.001f)
                result = Quaternion.LookRotation(
                    cameraLookingDirection.normalized,
                    hitPose.up
                );
            else
                //Camera directly above placement point
                result = hitPose.rotation;
            return result;
        }
    }
}