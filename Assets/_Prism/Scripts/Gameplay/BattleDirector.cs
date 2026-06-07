using System;
using System.Collections;
using Prism.Data;
using UnityEngine;

namespace Prism.Gameplay
{
    /// <summary>
    /// 두 크리처를 배치하고 자동 턴 대전을 연출합니다.
    /// </summary>
    public sealed class BattleDirector : MonoBehaviour, IBattleDirector
    {
        private const int MaxTurns = 30;
        private static readonly Vector3 LeftOffset = new(-0.35f, 0f, 0f);
        private static readonly Vector3 RightOffset = new(0.35f, 0f, 0f);

        [SerializeField] private CreatureDatabase creatureDatabase;

        private readonly ITurnResolver turnResolver = new TurnResolver();
        private Coroutine battleRoutine;

        /// <inheritdoc />
        public event Action<BattleResult> BattleEnded;

        /// <inheritdoc />
        public void StartBattle(CreatureData a, CreatureData b, Pose arenaPose)
        {
            if (battleRoutine != null)
            {
                StopCoroutine(battleRoutine);
            }

            battleRoutine = StartCoroutine(RunBattle(a, b, arenaPose));
        }

        private IEnumerator RunBattle(CreatureData a, CreatureData b, Pose arenaPose)
        {
            if (creatureDatabase == null)
            {
                throw new InvalidOperationException("BattleDirector requires a CreatureDatabase.");
            }

            Transform arenaRoot = CreateArenaRoot(arenaPose);
            Transform aSlot = CreateSlot(arenaRoot, "Creature A Slot", LeftOffset);
            Transform bSlot = CreateSlot(arenaRoot, "Creature B Slot", RightOffset);

            ICreatureController controllerA = CreatureFactory.Build(creatureDatabase, a, aSlot);
            ICreatureController controllerB = CreatureFactory.Build(creatureDatabase, b, bSlot);

            CombatState stateA = new(a, a.Stats.MaxHp, a.Element);
            CombatState stateB = new(b, b.Stats.MaxHp, b.Element);

            yield return PlayAndWait(controllerA, ActionType.BattleReady);
            yield return PlayAndWait(controllerB, ActionType.BattleReady);

            int turns = 0;
            while (stateA.CurrentHp > 0 && stateB.CurrentHp > 0 && turns < MaxTurns)
            {
                bool aAttacks = turns % 2 == 0;
                if (aAttacks)
                {
                    yield return ResolveAndPlayTurn(controllerA, controllerB, stateA, stateB, outcome => stateB = stateB.ApplyDamage(outcome.Damage));
                }
                else
                {
                    yield return ResolveAndPlayTurn(controllerB, controllerA, stateB, stateA, outcome => stateA = stateA.ApplyDamage(outcome.Damage));
                }

                turns++;
            }

            CreatureData winner = stateA.CurrentHp > 0 ? a : b;
            CreatureData loser = winner == a ? b : a;
            ICreatureController winnerController = winner == a ? controllerA : controllerB;
            ICreatureController loserController = loser == a ? controllerA : controllerB;

            yield return PlayAndWait(loserController, ActionType.Faint);
            yield return PlayAndWait(winnerController, ActionType.Win);

            BattleEnded?.Invoke(new BattleResult(winner, loser, turns));
            battleRoutine = null;
        }

        private IEnumerator ResolveAndPlayTurn(
            ICreatureController attackerController,
            ICreatureController defenderController,
            CombatState attacker,
            CombatState defender,
            Action<TurnOutcome> applyOutcome)
        {
            TurnOutcome outcome = turnResolver.Resolve(attacker, defender);
            yield return PlayAndWait(attackerController, ActionType.Attack);
            yield return PlayAndWait(defenderController, ActionType.Hit);
            applyOutcome(outcome);
        }

        private static IEnumerator PlayAndWait(ICreatureController controller, ActionType action)
        {
            bool finished = false;
            void OnFinished(ActionType finishedAction)
            {
                if (finishedAction == action)
                {
                    finished = true;
                }
            }

            controller.ActionFinished += OnFinished;
            controller.Play(action);

            while (!finished)
            {
                yield return null;
            }

            controller.ActionFinished -= OnFinished;
        }

        private static Transform CreateArenaRoot(Pose pose)
        {
            GameObject root = new("Prism Battle Arena");
            root.transform.SetPositionAndRotation(pose.position, pose.rotation);
            return root.transform;
        }

        private static Transform CreateSlot(Transform parent, string name, Vector3 localPosition)
        {
            GameObject slot = new(name);
            Transform slotTransform = slot.transform;
            slotTransform.SetParent(parent, false);
            slotTransform.localPosition = localPosition;
            return slotTransform;
        }
    }
}
