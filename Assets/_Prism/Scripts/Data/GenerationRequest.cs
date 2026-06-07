using UnityEngine;

namespace Prism.Data
{
    /// <summary>
    /// 크리처 생성에 필요한 포획 순간 입력값입니다.
    /// </summary>
    public readonly struct GenerationRequest
    {
        /// <summary>XRReferenceImage 이름과 일치하는 바인딩 키입니다.</summary>
        public readonly string ReferenceImageName;

        /// <summary>AR-neutral 카메라 프레임입니다.</summary>
        public readonly CameraImageFrame CameraFrame;

        /// <summary>샘플링할 화면/프레임 영역입니다.</summary>
        public readonly RectInt SampleRegion;

        /// <summary>
        /// 생성 요청을 구성합니다.
        /// </summary>
        public GenerationRequest(string referenceImageName, CameraImageFrame cameraFrame, RectInt sampleRegion)
        {
            ReferenceImageName = referenceImageName;
            CameraFrame = cameraFrame;
            SampleRegion = sampleRegion;
        }
    }
}
