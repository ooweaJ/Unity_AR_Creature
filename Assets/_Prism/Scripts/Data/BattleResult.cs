namespace Prism.Data
{
    /// <summary>
    /// 대전 종료 결과입니다.
    /// </summary>
    public readonly struct BattleResult
    {
        /// <summary>승리한 크리처입니다.</summary>
        public readonly CreatureData Winner;

        /// <summary>패배한 크리처입니다.</summary>
        public readonly CreatureData Loser;

        /// <summary>진행된 턴 수입니다.</summary>
        public readonly int Turns;

        /// <summary>
        /// 대전 결과를 생성합니다.
        /// </summary>
        public BattleResult(CreatureData winner, CreatureData loser, int turns)
        {
            Winner = winner;
            Loser = loser;
            Turns = turns;
        }
    }
}
