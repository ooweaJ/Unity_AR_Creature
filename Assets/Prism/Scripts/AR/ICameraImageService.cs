using Prism.Data;

namespace Prism.AR
{
    /// <summary>
    /// 최신 카메라 CPU 이미지를 AR-neutral 프레임으로 제공합니다.
    /// </summary>
    public interface ICameraImageService
    {
        /// <summary>
        /// 최신 카메라 이미지를 RGBA32 프레임으로 획득합니다.
        /// </summary>
        bool TryAcquireLatest(out CameraImageFrame frame);
    }
}
