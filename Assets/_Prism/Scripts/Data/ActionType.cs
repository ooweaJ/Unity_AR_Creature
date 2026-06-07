namespace Prism.Data
{
    /// <summary>
    /// 크리처가 수행할 수 있는 데이터 기반 행동 목록입니다.
    /// </summary>
    public enum ActionType
    {
        Spawn,
        Idle,
        Capture,
        Tap,
        Drag,
        Curious,
        BattleReady,
        Attack,
        Hit,
        Win,
        Faint
    }
}
