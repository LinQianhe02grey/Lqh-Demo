using System.Collections.Generic;
using UnityEngine;
using Cardwin.Cards;
using Cardwin.Inventory;
using Cardwin.Enemies;
using Cardwin.UI;

namespace Cardwin.Combat
{
    public class RewardManager : MonoBehaviour
    {
        private CardDatabase _cardDatabase;
        private InventorySystem _inventory;
        private PlayerController2D _playerController;
        private List<CardData> _rewardChoices = new();
        private bool _showingReward;
        private int _processedDeaths;

        private void Start()
        {
            _cardDatabase = FindObjectOfType<CardDatabase>();
            if (_cardDatabase == null)
            {
                CardDatabase[] all = Resources.FindObjectsOfTypeAll<CardDatabase>();
                if (all.Length > 0)
                    _cardDatabase = all[0];
            }

            _inventory = GetComponent<InventorySystem>();
            _playerController = GetComponent<PlayerController2D>();

            if (_cardDatabase == null)
                Debug.LogError("[RewardManager] CardDatabase not found in scene.");
            else
                Debug.Log($"[RewardManager] CardDatabase found. Cards={_cardDatabase.allCards.Count}");

            SubscribeToEnemies();
        }

        private void SubscribeToEnemies()
        {
            Health[] allHealth = FindObjectsOfType<Health>();
            int subscribed = 0;
            foreach (Health h in allHealth)
            {
                if (h.GetComponent<MeleeEnemyController>() != null
                    || h.GetComponent<RangedEnemyController>() != null)
                {
                    h.OnDeath.AddListener(() => OnEnemyKilled());
                    subscribed++;
                }
            }
            Debug.Log($"[RewardManager] Subscribed to {subscribed} enemy Health.OnDeath events.");
        }

        private void OnEnemyKilled()
        {
            if (_showingReward)
                return;

            if (_cardDatabase == null)
                return;

            _rewardChoices = _cardDatabase.GetRandomCards(3, false);
            if (_rewardChoices.Count == 0)
                return;

            _showingReward = true;
            Time.timeScale = 0f;
            if (_playerController != null)
                _playerController.SetInputLocked(true);

            string names = string.Join(", ", _rewardChoices.ConvertAll(c => c.cardName));
            Debug.Log($"[Reward] Enemy killed. Choices: {names}");
        }

        private void OnGUI()
        {
            if (!_showingReward || _rewardChoices.Count == 0)
                return;

            float w = 520f;
            float h = 320f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.Box(new Rect(x, y, w, h), "Pick a Reward Card");

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 16;
            btnStyle.alignment = TextAnchor.MiddleLeft;

            float btnW = 460f;
            float btnH = 55f;
            float startX = x + (w - btnW) * 0.5f;
            float startY = y + 50f;

            for (int i = 0; i < _rewardChoices.Count; i++)
            {
                CardData card = _rewardChoices[i];
                if (card == null) continue;

                string atkTag = card.IsOffensive ? " [ATK]" : "";
                string label = $"  {card.cardName}{atkTag}  —  L:{CardSlotUI.EffectToShortPublic(card.leftClickEffect)}  R:{CardSlotUI.EffectToShortPublic(card.rightClickEffect)}";

                if (GUI.Button(new Rect(startX, startY + i * (btnH + 8f), btnW, btnH), label, btnStyle))
                {
                    SelectCard(card);
                }
            }
        }

        private void SelectCard(CardData card)
        {
            if (_inventory != null)
                _inventory.AddCard(card);

            Debug.Log($"[Reward] Selected card={card.cardName} added to inventory.");

            _rewardChoices.Clear();
            _showingReward = false;
            Time.timeScale = 1f;
            if (_playerController != null)
                _playerController.SetInputLocked(false);
        }
    }
}
