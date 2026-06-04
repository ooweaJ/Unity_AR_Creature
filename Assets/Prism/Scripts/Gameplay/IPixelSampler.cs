using Prism.Data;
using UnityEngine;

namespace Prism.Gameplay
{
    /// <summary>
    /// RGBA32 카메라 프레임에서 생성용 픽셀 통계를 계산합니다.
    /// </summary>
    public interface IPixelSampler
    {
        /// <summary>
        /// 지정 영역을 다운샘플링해 픽셀 통계를 계산합니다.
        /// </summary>
        PixelStats Sample(in CameraImageFrame frame, RectInt region, int downsample = 32);
    }
}
