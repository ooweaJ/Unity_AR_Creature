using System;
using Prism.Data;
using UnityEngine;

namespace Prism.Gameplay
{
    /// <summary>
    /// 크리처 prefab 인스턴스의 Animator와 행동 이벤트를 제어합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CreatureController : MonoBehaviour, ICreatureController
    {
        private static readonly int IsMovingId = Animator.StringToHash("isMoving");

        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource audioSource;

        private CreatureAnimationSet animationSet;

        /// <inheritdoc />
        public CreatureData Data { get; private set; }

        /// <inheritdoc />
        public event Action<ActionType> ActionStarted;

        /// <inheritdoc />
        public event Action<ActionType> ActionFinished;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        /// <summary>
        /// 팩토리에서 생성 직후 컨트롤러를 초기화합니다.
        /// </summary>
        public void Initialize(CreatureData data, CreatureAnimationSet animationSet)
        {
            Data = data;
            this.animationSet = animationSet;

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator != null && animationSet != null && animationSet.OverrideController != null)
            {
                animator.runtimeAnimatorController = animationSet.OverrideController;
            }
        }

        /// <inheritdoc />
        public void Play(ActionType action)
        {
            ActionStarted?.Invoke(action);
            ApplyAnimatorParameters(action);
            PlayFeedback(action);

            // Placeholder 컨트롤러는 실제 클립 이벤트가 없으므로 즉시 완료를 발행한다.
            ActionFinished?.Invoke(action);
        }

        private void ApplyAnimatorParameters(ActionType action)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(IsMovingId, CreatureActionAnimatorParameters.UsesMovingBool(action));
            animator.SetTrigger(CreatureActionAnimatorParameters.GetTrigger(action));
        }

        private void PlayFeedback(ActionType action)
        {
            if (audioSource == null || animationSet == null)
            {
                return;
            }

            AudioClip sfx = animationSet.FindSfx(action);
            if (sfx != null)
            {
                audioSource.PlayOneShot(sfx);
            }
        }
    }
}
