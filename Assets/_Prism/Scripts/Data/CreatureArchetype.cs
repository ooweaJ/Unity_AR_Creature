using UnityEngine;

namespace Prism.Data
{
    /// <summary>
    /// 크리처의 베이스 프리팹과 애니메이션 세트를 묶는 아키타입 정의입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "CreatureArchetype", menuName = "Prism/Creature Archetype")]
    public sealed class CreatureArchetype : ScriptableObject
    {
        [SerializeField] private string archetypeId;
        [SerializeField] private GameObject basePrefab;
        [SerializeField] private CreatureAnimationSet animationSet;

        /// <summary>저장 데이터에서 아키타입을 복원할 때 쓰는 고유 ID입니다.</summary>
        public string ArchetypeId => archetypeId;

        /// <summary>인스턴스화할 베이스 프리팹입니다.</summary>
        public GameObject BasePrefab => basePrefab;

        /// <summary>아키타입별 애니메이션/피드백 세트입니다.</summary>
        public CreatureAnimationSet AnimationSet => animationSet;
    }
}
