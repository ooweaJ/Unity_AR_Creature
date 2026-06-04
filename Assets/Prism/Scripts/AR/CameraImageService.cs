using Prism.Data;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Prism.AR
{
    /// <summary>
    /// ARCameraManager의 XRCpuImage를 RGBA32 CameraImageFrame으로 변환합니다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ARCameraManager))]
    public sealed class CameraImageService : MonoBehaviour, ICameraImageService
    {
        [SerializeField] private ARCameraManager cameraManager;

        private void Awake()
        {
            if (cameraManager == null)
            {
                cameraManager = GetComponent<ARCameraManager>();
            }
        }

        /// <inheritdoc />
        public bool TryAcquireLatest(out CameraImageFrame frame)
        {
            frame = default;

            if (cameraManager == null || !cameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage))
            {
                return false;
            }

            try
            {
                XRCpuImage.ConversionParams conversionParams = new(
                    cpuImage,
                    TextureFormat.RGBA32,
                    XRCpuImage.Transformation.None);

                int size = cpuImage.GetConvertedDataSize(conversionParams);
                using NativeArray<byte> buffer = new(size, Allocator.Temp);
                cpuImage.Convert(conversionParams, buffer);

                frame = new CameraImageFrame(cpuImage.width, cpuImage.height, buffer.ToArray());
                return true;
            }
            finally
            {
                cpuImage.Dispose();
            }
        }
    }
}
