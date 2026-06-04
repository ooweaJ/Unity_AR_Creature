using System;
using System.Collections.Generic;
using Prism.Data;

namespace Prism.Gameplay
{
    /// <summary>
    /// 포획한 크리처 목록과 JSON 영속성을 관리합니다.
    /// </summary>
    public interface ICollectionService
    {
        /// <summary>현재 메모리에 로드된 모든 크리처입니다.</summary>
        IReadOnlyList<CreatureData> All { get; }

        /// <summary>
        /// 크리처를 컬렉션에 추가하고 즉시 저장합니다.
        /// </summary>
        void Add(CreatureData data);

        /// <summary>
        /// 지정 레퍼런스 이미지 ID로 생성된 크리처가 있는지 확인합니다.
        /// </summary>
        bool Contains(string referenceImageId);

        /// <summary>
        /// JSON 파일에서 컬렉션을 로드합니다.
        /// </summary>
        void Load();

        /// <summary>크리처가 추가될 때 발생합니다.</summary>
        event Action<CreatureData> CreatureAdded;
    }
}
