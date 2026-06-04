namespace Prism.Data
{
    /// <summary>
    /// 한 턴 공격 계산 결과입니다.
    /// </summary>
    public readonly struct TurnOutcome
    {
        /// <summary>적용할 데미지입니다.</summary>
        public readonly int Damage;

        /// <summary>상성 우위 또는 결정적 보너스가 발생했는지 여부입니다.</summary>
        public readonly bool IsCritical;

        /// <summary>방어자가 이 턴으로 쓰러지는지 여부입니다.</summary>
        public readonly bool DefenderDown;

        /// <summary>
        /// 턴 결과를 생성합니다.
        /// </summary>
        public TurnOutcome(int damage, bool isCritical, bool defenderDown)
        {
            Damage = damage;
            IsCritical = isCritical;
            DefenderDown = defenderDown;
        }
    }
}
