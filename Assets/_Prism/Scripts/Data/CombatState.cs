using System;
using UnityEngine;

namespace Prism.Data
{
    /// <summary>
    /// 대전 중 크리처의 현재 전투 상태입니다.
    /// </summary>
    [Serializable]
    public struct CombatState
    {
        [SerializeField] private CreatureData source;
        [SerializeField] private int currentHp;
        [SerializeField] private ElementType element;

        /// <summary>원본 크리처 데이터입니다.</summary>
        public CreatureData Source => source;

        /// <summary>현재 체력입니다.</summary>
        public int CurrentHp => currentHp;

        /// <summary>전투에 사용할 원소 타입입니다.</summary>
        public ElementType Element => element;

        /// <summary>
        /// 전투 상태를 생성합니다.
        /// </summary>
        public CombatState(CreatureData source, int currentHp, ElementType element)
        {
            this.source = source;
            this.currentHp = Mathf.Max(0, currentHp);
            this.element = element;
        }

        /// <summary>
        /// 데미지를 반영한 새 전투 상태를 반환합니다.
        /// </summary>
        public CombatState ApplyDamage(int damage)
        {
            return new CombatState(source, Mathf.Max(0, currentHp - Mathf.Max(0, damage)), element);
        }
    }
}
