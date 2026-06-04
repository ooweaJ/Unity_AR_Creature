using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Prism.AR
{
    /// <summary>
    /// AR 평면 레이캐스트와 앵커 생성을 담당합니다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ARRaycastManager))]
    public sealed class PlacementService : MonoBehaviour, IPlacementService
    {
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private Transform anchorParent;

        private readonly List<ARRaycastHit> hits = new();
        private readonly List<AnchorHandle> anchorHandles = new();
        private bool lastPlaneAvailability;

        /// <inheritdoc />
        public event Action<bool> PlaneAvailabilityChanged;

        private void Awake()
        {
            if (raycastManager == null)
            {
                raycastManager = GetComponent<ARRaycastManager>();
            }

            if (planeManager == null)
            {
                planeManager = GetComponent<ARPlaneManager>();
            }
        }

        private void OnEnable()
        {
            if (planeManager != null)
            {
                planeManager.trackablesChanged.AddListener(OnPlanesChanged);
                PublishPlaneAvailabilityIfChanged();
            }
        }

        private void OnDisable()
        {
            if (planeManager != null)
            {
                planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
            }
        }

        private void Update()
        {
            for (int i = anchorHandles.Count - 1; i >= 0; i--)
            {
                AnchorHandle handle = anchorHandles[i];
                if (handle.Transform == null)
                {
                    anchorHandles.RemoveAt(i);
                    continue;
                }

                handle.PollTrackingState();
            }
        }

        /// <inheritdoc />
        public bool TryRaycastPlane(Vector2 screenPos, out Pose pose)
        {
            pose = default;
            hits.Clear();

            if (raycastManager == null ||
                !raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon) ||
                hits.Count == 0)
            {
                return false;
            }

            pose = hits[0].pose;
            return true;
        }

        /// <inheritdoc />
        public IAnchorHandle CreateAnchor(Pose pose)
        {
            GameObject anchorObject = new("Prism AR Anchor");
            Transform anchorTransform = anchorObject.transform;
            anchorTransform.SetPositionAndRotation(pose.position, pose.rotation);

            if (anchorParent != null)
            {
                anchorTransform.SetParent(anchorParent, true);
            }

            ARAnchor anchor = anchorObject.AddComponent<ARAnchor>();
            AnchorHandle handle = new(anchor);
            anchorHandles.Add(handle);
            return handle;
        }

        private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> _)
        {
            PublishPlaneAvailabilityIfChanged();
        }

        private void PublishPlaneAvailabilityIfChanged()
        {
            bool available = HasTrackingPlane();
            if (available == lastPlaneAvailability)
            {
                return;
            }

            lastPlaneAvailability = available;
            PlaneAvailabilityChanged?.Invoke(available);
        }

        private bool HasTrackingPlane()
        {
            if (planeManager == null)
            {
                return false;
            }

            foreach (ARPlane plane in planeManager.trackables)
            {
                if (plane != null && plane.trackingState == TrackingState.Tracking)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
