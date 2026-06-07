using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prism.Data
{
    /// <summary>
    /// 크리처 아키타입별 애니메이션 오버라이드와 행동 피드백 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "CreatureAnimationSet", menuName = "Prism/Creature Animation Set")]
    public sealed class CreatureAnimationSet : ScriptableObject
    {
        [SerializeField] private AnimatorOverrideController overrideController;
        [SerializeField] private FeedbackEntry[] feedback = Array.Empty<FeedbackEntry>();

        /// <summary>공유 FSM 위에 적용할 오버라이드 컨트롤러입니다.</summary>
        public AnimatorOverrideController OverrideController => overrideController;

        /// <summary>행동별 사운드 피드백 목록입니다.</summary>
        public IReadOnlyList<FeedbackEntry> Feedback => feedback;

        /// <summary>
        /// 특정 행동에 연결된 사운드 피드백을 찾습니다.
        /// </summary>
        public AudioClip FindSfx(ActionType action)
        {
            if (feedback == null)
            {
                return null;
            }

            for (int i = 0; i < feedback.Length; i++)
            {
                if (feedback[i].Action == action)
                {
                    return feedback[i].Sfx;
                }
            }

            return null;
        }

        /// <summary>
        /// 행동별 피드백 항목입니다.
        /// </summary>
        [Serializable]
        public struct FeedbackEntry
        {
            [SerializeField] private ActionType action;
            [SerializeField] private AudioClip sfx;

            /// <summary>대상 행동입니다.</summary>
            public ActionType Action => action;

            /// <summary>행동 시작 시 재생할 사운드입니다.</summary>
            public AudioClip Sfx => sfx;

            /// <summary>
            /// 피드백 항목을 생성합니다.
            /// </summary>
            public FeedbackEntry(ActionType action, AudioClip sfx)
            {
                this.action = action;
                this.sfx = sfx;
            }
        }
    }
}
