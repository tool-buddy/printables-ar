using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public static class TransformManipulator
    {
        public static void Rotate(
            Touch firstTouch,
            Transform target,
            Quaternion initialRotation,
            Vector3 cameraPosition,
            float yawSensitivity = 0.2f,
            float pitchSensitivity = 0.09f)
        {
            Vector2 dragDelta = firstTouch.screenPosition - firstTouch.startScreenPosition;

            float yawAmount = -dragDelta.x * yawSensitivity;
            float pitchAmount = dragDelta.y * pitchSensitivity;

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
            Vector3 cameraPosition,
            [NotNull] Raycaster raycaster)
        {
            if (raycaster == null)
                throw new ArgumentNullException(nameof(raycaster));

            if (!raycaster.TryGetHit(
                    finger,
                    out Pose hitPose
                ))
                return false;

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