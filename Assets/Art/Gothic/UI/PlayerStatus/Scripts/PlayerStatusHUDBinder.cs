using UnityEngine;
using Cardwin.Combat;
using Cardwin.Cards;

namespace Cardwin.UI
{
    public class PlayerStatusHUDBinder : MonoBehaviour
    {
        [SerializeField] private PlayerStatusHUDView _view;
        [SerializeField] private Sprite _focusIconSprite;

        private Health _health;
        private PlayerCardContext _cardContext;

        private int _lastHp, _lastBlock, _lastFocus;
        private int _maxHp, _maxBlock, _maxMp;

        private void Start()
        {
            if (_view == null) _view = GetComponent<PlayerStatusHUDView>();

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _health = player.GetComponent<Health>();
                if (_health != null)
                {
                    _maxHp = _health.maxHealth;
                    _maxBlock = _health.maxHealth;
                }

                var executor = player.GetComponent<CardEffectExecutor>();
                if (executor != null)
                {
                    var contextField = typeof(CardEffectExecutor).GetField("_context",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (contextField != null)
                        _cardContext = contextField.GetValue(executor) as PlayerCardContext;
                }

                if (_cardContext == null)
                {
                    var pc = player.GetComponent<PlayerController2D>();
                    if (pc != null)
                        _cardContext = pc.cardContext;
                }
            }

            _maxMp = 100;
            _maxBlock = 100;

            if (_cardContext != null)
            {
                _cardContext.OnFocusChanged += OnFocusChanged;
                ApplyFocusIcon(_cardContext.focusStacks);
            }
        }

        private void OnDestroy()
        {
            if (_cardContext != null)
                _cardContext.OnFocusChanged -= OnFocusChanged;
        }

        private void OnFocusChanged(int focusStacks)
        {
            ApplyFocusIcon(focusStacks);
        }

        private void ApplyFocusIcon(int focusStacks)
        {
            if (_view == null || _view.StatusEffectStrip == null)
                return;

            if (focusStacks > 0)
                _view.StatusEffectStrip.ShowStatusIcon("Focus", _focusIconSprite);
            else
                _view.StatusEffectStrip.HideStatusIcon("Focus");
        }

        private void LateUpdate()
        {
            if (_view == null) return;

            if (_health != null)
            {
                int hp = _health.currentHealth;
                int block = _health.currentBlock;
                if (hp != _lastHp)
                {
                    _view.SetHPBar(hp, _maxHp, _maxHp);
                    _lastHp = hp;
                }
                if (block != _lastBlock)
                {
                    _view.SetShieldBar(block, _maxBlock, _maxBlock);
                    _lastBlock = block;
                }
            }

            if (_cardContext != null)
            {
                int focus = _cardContext.focusStacks;
                if (focus != _lastFocus)
                {
                    _view.SetMPBar(focus, _maxMp, _maxMp);
                    _lastFocus = focus;
                }
            }
        }
    }
}
