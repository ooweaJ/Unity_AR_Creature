using System;
using UnityEngine;

namespace Prism.Data
{
    /// <summary>
    /// 생성 및 저장에 사용하는 크리처 인스턴스 데이터입니다.
    /// </summary>
    [Serializable]
    public sealed class CreatureData
    {
        [SerializeField] private int seed;
        [SerializeField] private string referenceImageId;
        [SerializeField] private string archetypeId;
        [SerializeField] private ElementType element;
        [SerializeField] private Color primaryTint;
        [SerializeField] private Color secondaryTint;
        [SerializeField] private CreatureStats stats;
        [SerializeField] private Rarity rarity;
        [SerializeField] private string capturedAt;

        /// <summary>결정적 변주에 사용하는 seed입니다.</summary>
        public int Seed => seed;

        /// <summary>크리처가 생성된 레퍼런스 이미지 식별자입니다.</summary>
        public string ReferenceImageId => referenceImageId;

        /// <summary>복원에 사용할 아키타입 식별자입니다.</summary>
        public string ArchetypeId => archetypeId;

        /// <summary>크리처의 원소 타입입니다.</summary>
        public ElementType Element => element;

        /// <summary>주요 머티리얼 틴트입니다.</summary>
        public Color PrimaryTint => primaryTint;

        /// <summary>보조 머티리얼 틴트입니다.</summary>
        public Color SecondaryTint => secondaryTint;

        /// <summary>전투용 기본 수치입니다.</summary>
        public CreatureStats Stats => stats;

        /// <summary>희귀도입니다.</summary>
        public Rarity Rarity => rarity;

        /// <summary>포획 시각 메타데이터입니다.</summary>
        public string CapturedAt => capturedAt;

        /// <summary>
        /// JsonUtility 역직렬화를 위한 기본 생성자입니다.
        /// </summary>
        public CreatureData()
        {
        }

        /// <summary>
        /// 크리처 저장 데이터를 생성합니다.
        /// </summary>
        public CreatureData(
            int seed,
            string referenceImageId,
            string archetypeId,
            ElementType element,
            Color primaryTint,
            Color secondaryTint,
            CreatureStats stats,
            Rarity rarity,
            string capturedAt)
        {
            this.seed = seed;
            this.referenceImageId = referenceImageId;
            this.archetypeId = archetypeId;
            this.element = element;
            this.primaryTint = primaryTint;
            this.secondaryTint = secondaryTint;
            this.stats = stats;
            this.rarity = rarity;
            this.capturedAt = capturedAt;
        }
    }
}
