using System;
using System.Collections.Generic;
using Prism.AR;
using Prism.Data;
using Prism.Gameplay;
using UnityEngine;

namespace Prism.Presentation
{
    /// <summary>
    /// Wires the Prism capture flow for a scene without exposing AR Foundation types above the AR layer.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrismSceneBootstrap : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CreatureDatabase creatureDatabase;
        [SerializeField, Min(1)] private int generationDownsample = 32;

        [Header("Services")]
        [SerializeField] private MonoBehaviour imageTrackingServiceSource;
        [SerializeField] private MonoBehaviour cameraImageServiceSource;
        [SerializeField] private MonoBehaviour placementServiceSource;
        [SerializeField] private MonoBehaviour environmentServiceSource;

        [Header("Spawn")]
        [SerializeField] private Transform creatureRoot;
        [SerializeField] private Vector3 spawnLocalOffset;
        [SerializeField] private bool skipAlreadyCollected = true;

        private IImageTrackingService imageTrackingService;
        private ICameraImageService cameraImageService;
        private IPlacementService placementService;
        private IEnvironmentService environmentService;
        private IGenerationService generationService;
        private ICollectionService collectionService;
        private readonly Dictionary<string, ICreatureController> spawnedByReference = new(StringComparer.Ordinal);
        private bool isHandlingRecognition;

        private void Awake()
        {
            imageTrackingService = CastService<IImageTrackingService>(imageTrackingServiceSource);
            cameraImageService = CastService<ICameraImageService>(cameraImageServiceSource);
            placementService = CastService<IPlacementService>(placementServiceSource);
            environmentService = CastService<IEnvironmentService>(environmentServiceSource);

            if (creatureRoot == null)
            {
                creatureRoot = transform;
            }

            if (creatureDatabase != null)
            {
                CreatureFactory.Configure(creatureDatabase);
                generationService = new GenerationService(
                    creatureDatabase,
                    new PixelSampler(),
                    new TraitMapper(),
                    generationDownsample);
            }

            collectionService = new CollectionService();
            collectionService.Load();
        }

        private void OnEnable()
        {
            if (imageTrackingService != null)
            {
                imageTrackingService.ImageRecognized += OnImageRecognized;
                imageTrackingService.ImageTrackingLost += OnImageTrackingLost;
                imageTrackingService.SetEnabled(true);
            }

            if (environmentService != null)
            {
                environmentService.SessionTrackingChanged += OnSessionTrackingChanged;
                environmentService.SetOcclusionEnabled(true);
            }
        }

        private void OnDisable()
        {
            if (imageTrackingService != null)
            {
                imageTrackingService.ImageRecognized -= OnImageRecognized;
                imageTrackingService.ImageTrackingLost -= OnImageTrackingLost;
            }

            if (environmentService != null)
            {
                environmentService.SessionTrackingChanged -= OnSessionTrackingChanged;
            }
        }

        private void OnImageRecognized(TrackedImageInfo info)
        {
            if (isHandlingRecognition)
            {
                return;
            }

            isHandlingRecognition = true;
            try
            {
                HandleImageRecognized(info);
            }
            finally
            {
                isHandlingRecognition = false;
            }
        }

        private void HandleImageRecognized(TrackedImageInfo info)
        {
            if (!ValidateReady(info.ReferenceImageName))
            {
                return;
            }

            if (spawnedByReference.ContainsKey(info.ReferenceImageName))
            {
                return;
            }

            CreatureData data = FindCollected(info.ReferenceImageName);
            if (data != null)
            {
                Spawn(data, info.WorldPose);
                return;
            }

            if (skipAlreadyCollected && collectionService.Contains(info.ReferenceImageName))
            {
                return;
            }

            if (!cameraImageService.TryAcquireLatest(out CameraImageFrame frame))
            {
                Debug.LogWarning($"Prism capture skipped because no camera frame was available for '{info.ReferenceImageName}'.", this);
                return;
            }

            RectInt sampleRegion = MapScreenBoundsToFrame(info.ScreenBounds, frame);
            CreatureData generated = generationService.Generate(new GenerationRequest(info.ReferenceImageName, frame, sampleRegion));
            collectionService.Add(generated);
            Spawn(generated, info.WorldPose);
        }

        private bool ValidateReady(string referenceImageName)
        {
            if (string.IsNullOrEmpty(referenceImageName))
            {
                return false;
            }

            if (creatureDatabase == null || generationService == null)
            {
                Debug.LogError("PrismSceneBootstrap requires a CreatureDatabase.", this);
                return false;
            }

            if (imageTrackingService == null || cameraImageService == null)
            {
                Debug.LogError("PrismSceneBootstrap requires image tracking and camera image services.", this);
                return false;
            }

            return true;
        }

        private CreatureData FindCollected(string referenceImageName)
        {
            IReadOnlyList<CreatureData> collected = collectionService.All;
            for (int i = 0; i < collected.Count; i++)
            {
                CreatureData data = collected[i];
                if (data != null && string.Equals(data.ReferenceImageId, referenceImageName, StringComparison.Ordinal))
                {
                    return data;
                }
            }

            return null;
        }

        private void Spawn(CreatureData data, Pose pose)
        {
            Transform parent = CreateSpawnParent(data.ReferenceImageId, pose);
            ICreatureController controller = CreatureFactory.Build(creatureDatabase, data, parent);
            spawnedByReference[data.ReferenceImageId] = controller;

            Transform controllerTransform = (controller as Component)?.transform;
            if (controllerTransform != null)
            {
                controllerTransform.localPosition = spawnLocalOffset;
                controllerTransform.localRotation = Quaternion.identity;
            }

            controller.Play(ActionType.Spawn);
        }

        private Transform CreateSpawnParent(string referenceImageName, Pose pose)
        {
            IAnchorHandle anchor = placementService?.CreateAnchor(pose);
            if (anchor?.Transform != null)
            {
                return anchor.Transform;
            }

            GameObject spawnPoint = new($"Prism Spawn - {referenceImageName}");
            Transform spawnTransform = spawnPoint.transform;
            spawnTransform.SetParent(creatureRoot, true);
            spawnTransform.SetPositionAndRotation(pose.position, pose.rotation);
            return spawnTransform;
        }

        private static RectInt MapScreenBoundsToFrame(RectInt screenBounds, in CameraImageFrame frame)
        {
            if (screenBounds.width <= 0 || screenBounds.height <= 0 || !frame.IsValid)
            {
                return new RectInt(0, 0, frame.Width, frame.Height);
            }

            float scaleX = frame.Width / Mathf.Max(1f, Screen.width);
            float scaleY = frame.Height / Mathf.Max(1f, Screen.height);
            int x = Mathf.FloorToInt(screenBounds.x * scaleX);
            int y = Mathf.FloorToInt(screenBounds.y * scaleY);
            int width = Mathf.CeilToInt(screenBounds.width * scaleX);
            int height = Mathf.CeilToInt(screenBounds.height * scaleY);
            return new RectInt(x, y, width, height);
        }

        private void OnImageTrackingLost(string referenceImageName)
        {
            spawnedByReference.Remove(referenceImageName);
        }

        private void OnSessionTrackingChanged(SessionTracking tracking)
        {
            if (tracking == SessionTracking.Limited)
            {
                Debug.Log("Prism AR session tracking is limited.", this);
            }
        }

        private static T CastService<T>(MonoBehaviour source) where T : class
        {
            return source as T;
        }
    }
}
