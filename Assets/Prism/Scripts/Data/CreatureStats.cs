using System;
using UnityEngine;

namespace Prism.Data
{
    /// <summary>
    /// 크리처의 전투용 기본 수치입니다.
    /// </summary>
    [Serializable]
    public struct CreatureStats
    {
        [SerializeField] private int attack;
        [SerializeField] private int defense;
        [SerializeField] private int maxHp;

        /// <summary>공격력입니다.</summary>
        public int Attack => attack;

        /// <summary>방어력입니다.</summary>
        public int Defense => defense;

        /// <summary>최대 체력입니다.</summary>
        public int MaxHp => maxHp;

        /// <summary>
        /// 전투 수치를 생성합니다.
        /// </summary>
        public CreatureStats(int attack, int defense, int maxHp)
        {
            this.attack = Mathf.Max(0, attack);
            this.defense = Mathf.Max(0, defense);
            this.maxHp = Mathf.Max(1, maxHp);
        }
    }
}
