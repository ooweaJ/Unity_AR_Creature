using System;
using Prism.Data;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Prism.AR
{
    /// <summary>
    /// AR 환경 서비스 구현체입니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnvironmentService : MonoBehaviour, IEnvironmentService
    {
        [SerializeField] private AROcclusionManager occlusionManager;
        [SerializeField] private ARCameraManager cameraManager;

        private SessionTracking lastSessionTracking = SessionTracking.None;

        /// <inheritdoc />
        public bool OcclusionSupported => occlusionManager != null && occlusionManager.descriptor != null;

        /// <inheritdoc />
        public event Action<LightReading> LightUpdated;

        /// <inheritdoc />
        public event Action<SessionTracking> SessionTrackingChanged;

        private void Awake()
        {
            if (cameraManager == null)
            {
                cameraManager = FindFirstObjectByType<ARCameraManager>();
            }

            if (occlusionManager == null)
            {
                occlusionManager = FindFirstObjectByType<AROcclusionManager>();
            }
        }

        private void OnEnable()
        {
            if (cameraManager != null)
            {
                cameraManager.frameReceived += OnFrameReceived;
            }

            ARSession.stateChanged += OnSessionStateChanged;
            PublishSessionTracking(SessionTrackingStateMapper.Map(ARSession.state));
        }

        private void OnDisable()
        {
            if (cameraManager != null)
            {
                cameraManager.frameReceived -= OnFrameReceived;
            }

            ARSession.stateChanged -= OnSessionStateChanged;
        }

        /// <inheritdoc />
        public void SetOcclusionEnabled(bool on)
        {
            if (!OcclusionSupported)
            {
                return;
            }

            occlusionManager.enabled = on;
        }

        private void OnFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            LightUpdated?.Invoke(LightEstimationMapper.Map(eventArgs.lightEstimation));
        }

        private void OnSessionStateChanged(ARSessionStateChangedEventArgs eventArgs)
        {
            PublishSessionTracking(SessionTrackingStateMapper.Map(eventArgs.state));
        }

        private void PublishSessionTracking(SessionTracking tracking)
        {
            if (tracking == lastSessionTracking)
            {
                return;
            }

            lastSessionTracking = tracking;
            SessionTrackingChanged?.Invoke(tracking);
        }
    }
}
