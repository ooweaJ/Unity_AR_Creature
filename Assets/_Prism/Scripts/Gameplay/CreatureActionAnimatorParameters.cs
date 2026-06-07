using Prism.Data;

namespace Prism.Gameplay
{
    /// <summary>
    /// ActionType과 공유 Animator 파라미터 이름을 매핑합니다.
    /// </summary>
    public static class CreatureActionAnimatorParameters
    {
        /// <summary>
        /// 행동에 대응하는 Animator trigger 이름을 반환합니다.
        /// </summary>
        public static string GetTrigger(ActionType action)
        {
            return action switch
            {
                ActionType.Spawn => "spawn",
                ActionType.Capture => "capture",
                ActionType.Tap => "tap",
                ActionType.Drag => "drag",
                ActionType.Curious => "curious",
                ActionType.BattleReady => "battleStart",
                ActionType.Attack => "attack",
                ActionType.Hit => "hit",
                ActionType.Win => "win",
                ActionType.Faint => "faint",
                _ => "idle"
            };
        }

        /// <summary>
        /// 행동이 이동 상태 bool을 사용해야 하는지 여부입니다.
        /// </summary>
        public static bool UsesMovingBool(ActionType action)
        {
            return action == ActionType.Drag;
        }
    }
}
