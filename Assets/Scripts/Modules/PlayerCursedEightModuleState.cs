using System;
using System.Collections;
using UnityEngine;
using Cardwin.Combat;
using Cardwin.Magazine;
using Cardwin.Cards;

namespace Cardwin.Modules
{
    public sealed class PlayerCursedEightModuleState : MonoBehaviour
    {
        [Header("Module State")]
        [SerializeField] private bool isActive;

        [Header("Morality")]
        [SerializeField] private int evilValue = 8;
        [SerializeField] private int goodValue = 0;

        [Header("Fire Rate")]
        [SerializeField] private float fireRateMultiplier = 1.5f;

        [Header("HP Drain")]
        [SerializeField] private float hpDrainFractionPerSecond = 0.01f;

        [Header("Infinite Magazine")]
        [SerializeField] private bool infiniteEightLoopEnabled = true;

        private Health _health;
        private PlayerAlignment _alignment;
        private MagazineSystem _magazine;
        private PlayerController2D _controller;
        private Coroutine _drainRoutine;
        private float _drainAccumulator;

        public bool IsActive => isActive;
        public float FireRateMultiplier => fireRateMultiplier;
        public bool InfiniteEightLoopEnabled => isActive && infiniteEightLoopEnabled;

        public event Action OnDeactivated;

        private void Awake()
        {
            _alignment = GetComponent<PlayerAlignment>();
            _magazine = GetComponent<MagazineSystem>();
            _controller = GetComponent<PlayerController2D>();
        }

        private void ResolveHealth()
        {
            if (_health != null) return;
            _health = GetComponent<Health>();
            if (_health == null) _health = GetComponentInParent<Health>();
            if (_health == null) _health = GetComponentInChildren<Health>();
            if (_health != null)
                Debug.Log($"[CursedEightModule] Health resolved: obj={_health.gameObject.name}, maxHp={_health.maxHealth}, currentHp={_health.currentHealth}");
        }

        [ContextMenu("Debug/Activate Cursed Eight Module")]
        public void Activate()
        {
            if (isActive) return;
            isActive = true;

            var blessed = GetComponent<PlayerBlessedEightModuleState>();
            if (blessed != null && blessed.IsActive)
            {
                blessed.Deactivate();
                Debug.Log("[CursedEightModule] Deactivated existing Blessed module.");
            }

            ResolveHealth();

            if (_alignment != null)
                _alignment.SetValues(goodValue, evilValue);

            CardData attackCard = FindAttackCard();
            if (_magazine != null && attackCard != null)
                _magazine.ForceLoadEightAttackCards(attackCard);
            else if (_magazine != null)
                _magazine.InfiniteEightLoopEnabled = true;

            if (_drainRoutine != null) StopCoroutine(_drainRoutine);
            _drainAccumulator = 0f;
            _drainRoutine = StartCoroutine(DrainHpRoutine());

            if (_controller != null)
                _controller.SetExternalFireRateMultiplier(fireRateMultiplier);

            Debug.Log($"[CursedEightModule] ACTIVATED");
            Debug.Log($"[ModuleState] Good={(_alignment != null ? _alignment.Good : -1)}, Evil={(_alignment != null ? _alignment.Evil : -1)}, Sum={(_alignment != null ? _alignment.Good + _alignment.Evil : -1)}");
            Debug.Log($"[ModuleState] CursedActive=true, BlessedActive={blessed != null && blessed.IsActive}");
            if (_magazine != null)
                Debug.Log($"[ModuleState] LoadedCards={string.Join(",", _magazine.LoadedCards.ConvertAll(c => c != null ? c.name : "null"))}");
        }

        private IEnumerator DrainHpRoutine()
        {
            while (isActive)
            {
                ResolveHealth();
                if (_health == null || _health.maxHealth <= 0 || _health.currentHealth <= 0)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                int maxHp = _health.maxHealth;
                float minHp = maxHp * 0.01f;
                int oldHp = _health.currentHealth;

                if (oldHp <= Mathf.CeilToInt(minHp))
                {
                    Debug.Log($"[CursedEightModule] HP drain stopped: reached non-lethal min (hp={oldHp}, min={minHp:F1})");
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                _drainAccumulator += maxHp * hpDrainFractionPerSecond;
                int drainAmount = Mathf.FloorToInt(_drainAccumulator);
                if (drainAmount > 0)
                {
                    _drainAccumulator -= drainAmount;
                    int newHp = Mathf.Max(Mathf.CeilToInt(minHp), oldHp - drainAmount);
                    _health.currentHealth = newHp;
                    _health.OnDamaged?.Invoke(drainAmount);
                    Debug.Log($"[CursedEightModule] HP drain: {oldHp} -> {newHp}, drained={drainAmount}, accumulator={_drainAccumulator:F2}, maxHp={maxHp}");
                }

                yield return new WaitForSeconds(1f);
            }
        }

        [ContextMenu("Debug/Force One HP Drain Tick")]
        private void DebugForceOneDrainTick()
        {
            ResolveHealth();
            if (_health == null) { Debug.LogError("[CursedEightModule] Debug: Health missing"); return; }
            int oldHp = _health.currentHealth;
            _drainAccumulator += _health.maxHealth * hpDrainFractionPerSecond;
            int drainAmount = Mathf.FloorToInt(_drainAccumulator);
            if (drainAmount > 0)
            {
                _drainAccumulator -= drainAmount;
                int newHp = Mathf.Max(Mathf.CeilToInt(_health.maxHealth * 0.01f), oldHp - drainAmount);
                _health.currentHealth = newHp;
                _health.OnDamaged?.Invoke(drainAmount);
            }
            Debug.Log($"[CursedEightModule] Debug drain: {oldHp} -> {_health.currentHealth}, accumulator={_drainAccumulator:F2}, drainAmount={drainAmount}");
        }

        private CardData FindAttackCard()
        {
            if (_magazine == null) return null;
            if (_magazine.cardDatabase != null && _magazine.cardDatabase.allCards != null)
            {
                foreach (var card in _magazine.cardDatabase.allCards)
                {
                    if (card == null) continue;
                    if (card.cardType == CardType.Attack || card.leftClickEffect == CardEffectType.Damage || card.rightClickEffect == CardEffectType.Damage)
                        return card;
                }
            }
            if (_magazine.initialCards != null)
            {
                foreach (var card in _magazine.initialCards)
                {
                    if (card == null) continue;
                    if (card.cardType == CardType.Attack || card.leftClickEffect == CardEffectType.Damage)
                        return card;
                }
            }
            if (_magazine.LoadedCards != null)
            {
                foreach (var card in _magazine.LoadedCards)
                {
                    if (card == null) continue;
                    if (card.cardType == CardType.Attack || card.leftClickEffect == CardEffectType.Damage)
                        return card;
                }
            }
            Debug.LogError("[CursedEightModule] No attack card found.");
            return null;
        }

        public void Deactivate()
        {
            if (!isActive) return;
            isActive = false;
            if (_drainRoutine != null) { StopCoroutine(_drainRoutine); _drainRoutine = null; }
            if (_magazine != null) _magazine.InfiniteEightLoopEnabled = false;
            OnDeactivated?.Invoke();
        }
    }
}
