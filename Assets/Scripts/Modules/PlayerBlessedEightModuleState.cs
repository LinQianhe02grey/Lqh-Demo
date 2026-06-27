using System;
using System.Collections;
using UnityEngine;
using Cardwin.Combat;
using Cardwin.Magazine;
using Cardwin.Cards;

namespace Cardwin.Modules
{
    public sealed class PlayerBlessedEightModuleState : MonoBehaviour
    {
        [Header("Module State")]
        [SerializeField] private bool isActive;

        [Header("Morality")]
        [SerializeField] private int goodValue = 8;
        [SerializeField] private int evilValue = 0;

        [Header("Move Speed")]
        [SerializeField] private float moveSpeedMultiplier = 0.5f;

        [Header("Fire Rate")]
        [SerializeField] private float fireRateMultiplier = 0.5f;

        private PlayerAlignment _alignment;
        private MagazineSystem _magazine;
        private PlayerController2D _controller;
        private Coroutine _enemyAuraRoutine;

        public bool IsActive => isActive;
        public float FireRateMultiplier => fireRateMultiplier;

        private void Awake()
        {
            _alignment = GetComponent<PlayerAlignment>();
            _magazine = GetComponent<MagazineSystem>();
            _controller = GetComponent<PlayerController2D>();
        }

        [ContextMenu("Debug/Activate Blessed Eight Module")]
        public void Activate()
        {
            if (isActive) return;
            isActive = true;

            var cursed = GetComponent<PlayerCursedEightModuleState>();
            if (cursed != null && cursed.IsActive)
            {
                cursed.Deactivate();
                Debug.Log("[BlessedEightModule] Deactivated existing Cursed module.");
            }

            if (_alignment != null)
                _alignment.SetValues(goodValue, evilValue);

            CardData buffCard = FindBuffCard();
            Debug.Log($"[BlessedEightModule] Selected buff card: {(buffCard != null ? buffCard.name : "NULL")}");
            if (_magazine != null && buffCard != null)
                _magazine.ForceLoadEightAttackCards(buffCard);
            else if (_magazine != null)
                _magazine.InfiniteEightLoopEnabled = true;

            if (_controller != null)
            {
                _controller.SetExternalMoveSpeedMultiplier(moveSpeedMultiplier);
                _controller.SetExternalFireRateMultiplier(fireRateMultiplier);
            }

            if (_enemyAuraRoutine != null) StopCoroutine(_enemyAuraRoutine);
            _enemyAuraRoutine = StartCoroutine(EnemyAuraRoutine());

            Debug.Log($"[BlessedEightModule] ACTIVATED");
            Debug.Log($"[ModuleState] Good={(_alignment != null ? _alignment.Good : -1)}, Evil={(_alignment != null ? _alignment.Evil : -1)}, Sum={(_alignment != null ? _alignment.Good + _alignment.Evil : -1)}");
            Debug.Log($"[ModuleState] CursedActive={cursed != null && cursed.IsActive}, BlessedActive=true");
            Debug.Log($"[ModuleState] MoveSpeedMult={moveSpeedMultiplier}, FireRateMult={fireRateMultiplier}");
            if (_magazine != null)
                Debug.Log($"[ModuleState] LoadedCards={string.Join(",", _magazine.LoadedCards.ConvertAll(c => c != null ? c.name : "null"))}");
        }

        private CardData FindBuffCard()
        {
            if (_magazine == null) return null;
            if (_magazine.cardDatabase != null && _magazine.cardDatabase.allCards != null)
            {
                foreach (var card in _magazine.cardDatabase.allCards)
                {
                    if (card == null) continue;
                    if (card.cardType == CardType.Defense || card.cardType == CardType.Heal ||
                        card.leftClickEffect == CardEffectType.Block || card.leftClickEffect == CardEffectType.Heal ||
                        card.rightClickEffect == CardEffectType.Block || card.rightClickEffect == CardEffectType.Heal ||
                        card.leftClickEffect == CardEffectType.Focus || card.rightClickEffect == CardEffectType.Focus)
                        return card;
                }
            }
            if (_magazine.initialCards != null)
            {
                foreach (var card in _magazine.initialCards)
                {
                    if (card == null) continue;
                    if (card.cardType != CardType.Attack) return card;
                }
            }
            Debug.LogError("[BlessedEightModule] No buff card found.");
            return null;
        }

        private IEnumerator EnemyAuraRoutine()
        {
            while (isActive)
            {
                var enemies = FindObjectsByType<Health>(FindObjectsSortMode.None);
                foreach (var enemy in enemies)
                {
                    if (enemy == null || enemy.IsDead() || enemy.gameObject == gameObject) continue;
                    var dist = Vector2.Distance(transform.position, enemy.transform.position);
                    if (dist < 8f)
                    {
                        int drain = Mathf.Max(1, Mathf.RoundToInt(enemy.maxHealth * 0.02f));
                        enemy.currentHealth = Mathf.Max(1, enemy.currentHealth - drain);
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }

        public void Deactivate()
        {
            if (!isActive) return;
            isActive = false;
            if (_controller != null)
            {
                _controller.SetExternalMoveSpeedMultiplier(1f);
                _controller.SetExternalFireRateMultiplier(1f);
            }
            if (_enemyAuraRoutine != null) { StopCoroutine(_enemyAuraRoutine); _enemyAuraRoutine = null; }
            if (_magazine != null) _magazine.InfiniteEightLoopEnabled = false;
        }
    }
}
