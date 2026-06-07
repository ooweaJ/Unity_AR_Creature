using Prism.Data;

namespace Prism.Gameplay
{
    /// <summary>
    /// 포획 순간 입력값으로부터 저장 가능한 크리처 데이터를 생성합니다.
    /// </summary>
    public interface IGenerationService
    {
        /// <summary>
        /// 요청 데이터에 대응하는 크리처 데이터를 생성합니다.
        /// </summary>
        CreatureData Generate(in GenerationRequest request);
    }
}
