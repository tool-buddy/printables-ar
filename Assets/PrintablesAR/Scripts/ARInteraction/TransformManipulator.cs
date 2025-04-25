using System;
using JetBrains.Annotations;
using UnityEngine;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public static class TransformManipulator
    {
        public static void Rotate(
            Vector2 touchDrag,
            Transform target,
            Quaternion initialRotation,
            Vector3 cameraPosition,
            float yawSensitivity = 0.2f,
            float pitchSensitivity = 0.09f)
        {
            float yawAmount = -touchDrag.x * yawSensitivity;
            float pitchAmount = touchDrag.y * pitchSensitivity;

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
            Transform target,
            Vector3 initialScale,
            Vector2 firstTouchPosition,
            Vector2 secondTouchPosition,
            Vector2 firstTouchStartPosition,
            Vector2 secondTouchStartPosition)
        {
            float currentPinchDistance = Vector2.Distance(
                firstTouchPosition,
                secondTouchPosition
            );

            float initialPinchDistance = Vector2.Distance(
                firstTouchStartPosition,
                secondTouchStartPosition
            );

            target.localScale = initialScale * (currentPinchDistance / initialPinchDistance);
        }

        public static bool TryPlace(
            Vector2 touchPosition,
            Transform target,
            Vector3 cameraPosition,
            [NotNull] Raycaster raycaster)
        {
            if (raycaster == null)
                throw new ArgumentNullException(nameof(raycaster));

            if (!raycaster.TryGetHit(
                    touchPosition,
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