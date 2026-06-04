using System;
using System.Collections.Generic;
using Prism.Data;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Prism.AR
{
    /// <summary>
    /// ARTrackedImageManager 이벤트를 Prism 이미지 트래킹 이벤트로 변환합니다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ARTrackedImageManager))]
    public sealed class ImageTrackingService : MonoBehaviour, IImageTrackingService
    {
        [SerializeField] private ARTrackedImageManager imageManager;
        [SerializeField] private Camera arCamera;
        [SerializeField, Min(0f)] private float recognitionCooldownSeconds = 1f;

        private readonly Dictionary<TrackableId, string> trackedNamesById = new();
        private readonly HashSet<string> currentlyTrackingNames = new(StringComparer.Ordinal);
        private readonly Dictionary<string, float> lastRecognizedTimes = new(StringComparer.Ordinal);

        /// <inheritdoc />
        public event Action<TrackedImageInfo> ImageRecognized;

        /// <inheritdoc />
        public event Action<string> ImageTrackingLost;

        private void Awake()
        {
            if (imageManager == null)
            {
                imageManager = GetComponent<ARTrackedImageManager>();
            }

            if (arCamera == null)
            {
                arCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            imageManager.trackablesChanged.AddListener(OnTrackablesChanged);
        }

        private void OnDisable()
        {
            imageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        }

        /// <inheritdoc />
        public void SetEnabled(bool on)
        {
            imageManager.enabled = on;
        }

        private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
        {
            foreach (ARTrackedImage image in eventArgs.added)
            {
                ProcessTrackable(image);
            }

            foreach (ARTrackedImage image in eventArgs.updated)
            {
                ProcessTrackable(image);
            }

            foreach (KeyValuePair<TrackableId, ARTrackedImage> removed in eventArgs.removed)
            {
                ProcessRemoved(removed.Key, removed.Value);
            }
        }

        private void ProcessTrackable(ARTrackedImage image)
        {
            if (image == null)
            {
                return;
            }

            string referenceName = image.referenceImage.name;
            if (string.IsNullOrEmpty(referenceName))
            {
                return;
            }

            trackedNamesById[image.trackableId] = referenceName;

            if (image.trackingState == TrackingState.Tracking)
            {
                bool becameTracking = currentlyTrackingNames.Add(referenceName);
                if (becameTracking && CanRecognize(referenceName))
                {
                    lastRecognizedTimes[referenceName] = Time.unscaledTime;
                    ImageRecognized?.Invoke(ToInfo(image));
                }

                return;
            }

            if (currentlyTrackingNames.Remove(referenceName))
            {
                ImageTrackingLost?.Invoke(referenceName);
            }
        }

        private void ProcessRemoved(TrackableId trackableId, ARTrackedImage image)
        {
            string referenceName = image != null ? image.referenceImage.name : null;
            if (string.IsNullOrEmpty(referenceName))
            {
                trackedNamesById.TryGetValue(trackableId, out referenceName);
            }

            trackedNamesById.Remove(trackableId);

            if (!string.IsNullOrEmpty(referenceName) && currentlyTrackingNames.Remove(referenceName))
            {
                ImageTrackingLost?.Invoke(referenceName);
            }
        }

        private bool CanRecognize(string referenceName)
        {
            if (recognitionCooldownSeconds <= 0f)
            {
                return true;
            }

            return !lastRecognizedTimes.TryGetValue(referenceName, out float lastTime)
                || Time.unscaledTime - lastTime >= recognitionCooldownSeconds;
        }

        private TrackedImageInfo ToInfo(ARTrackedImage image)
        {
            Pose pose = new(image.transform.position, image.transform.rotation);
            RectInt screenBounds = TrackedImageScreenBoundsUtility.Calculate(arCamera, pose, image.size);
            return new TrackedImageInfo(
                image.referenceImage.name,
                pose,
                screenBounds,
                MapTrackingState(image.trackingState));
        }

        private static ImageTrackingState MapTrackingState(TrackingState state)
        {
            return state switch
            {
                TrackingState.Tracking => ImageTrackingState.Tracking,
                TrackingState.Limited => ImageTrackingState.Limited,
                _ => ImageTrackingState.None
            };
        }
    }
}
