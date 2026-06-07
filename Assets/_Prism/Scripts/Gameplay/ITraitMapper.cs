using Prism.Data;

namespace Prism.Gameplay
{
    /// <summary>
    /// 베이스 바인딩과 픽셀 통계를 크리처 형질 데이터로 매핑합니다.
    /// </summary>
    public interface ITraitMapper
    {
        /// <summary>
        /// 결정적 seed와 픽셀 통계를 사용해 크리처 데이터를 생성합니다.
        /// </summary>
        CreatureData Map(ReferenceImageBinding baseBinding, in PixelStats stats, int seed);
    }
}
