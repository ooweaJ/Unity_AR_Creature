using UnityEngine;

namespace Prism.Data
{
    /// <summary>
    /// AR Foundation CPU 이미지를 RGBA32 버퍼로 변환한 AR-neutral 프레임입니다.
    /// </summary>
    public readonly struct CameraImageFrame
    {
        /// <summary>프레임 너비입니다.</summary>
        public readonly int Width;

        /// <summary>프레임 높이입니다.</summary>
        public readonly int Height;

        /// <summary>RGBA32 순서의 픽셀 바이트입니다.</summary>
        public readonly byte[] Rgba32;

        /// <summary>
        /// 카메라 프레임 데이터를 생성합니다.
        /// </summary>
        public CameraImageFrame(int width, int height, byte[] rgba32)
        {
            Width = Mathf.Max(0, width);
            Height = Mathf.Max(0, height);
            Rgba32 = rgba32;
        }

        /// <summary>프레임이 샘플링 가능한 상태인지 여부입니다.</summary>
        public bool IsValid => Width > 0 && Height > 0 && Rgba32 != null && Rgba32.Length >= Width * Height * 4;
    }
}
