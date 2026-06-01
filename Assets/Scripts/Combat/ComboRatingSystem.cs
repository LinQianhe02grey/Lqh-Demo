using UnityEngine;
using Cardwin.Cards;

namespace Cardwin.Combat
{
    public class ComboRatingSystem : MonoBehaviour
    {
        private int comboCount;
        private float comboTimer;
        private const float ComboTimeout = 5f;
        private string currentRank = "-";
        private bool comboActive;

        public int ComboCount => comboCount;
        public float ComboTimer => comboTimer;
        public string CurrentRank => currentRank;
        public bool IsActive => comboActive;

        public void RegisterCardUse(CardData card, bool usedLeftClick, bool useSucceeded)
        {
            if (!useSucceeded || card == null)
                return;

            bool correct = card.useTarget switch
            {
                CardUseTarget.Enemy => usedLeftClick,
                CardUseTarget.Self => !usedLeftClick,
                CardUseTarget.Both => true,
                _ => usedLeftClick
            };

            if (correct)
            {
                comboCount++;
                comboTimer = ComboTimeout;
                comboActive = true;
                currentRank = CalculateRank(comboCount);
                Debug.Log($"[Combo] Correct use card={card.cardName} combo={comboCount} rank={currentRank}");
            }
            else
            {
                Debug.Log($"[Combo] Wrong use card={card.cardName} input={(usedLeftClick ? "Left" : "Right")} reset.");
                ResetCombo("Wrong input");
            }
        }

        public void ResetCombo(string reason = "")
        {
            comboCount = 0;
            comboTimer = 0f;
            comboActive = false;
            currentRank = "-";
            Debug.Log($"[Combo] Reset. reason={reason}");
        }

        private void Update()
        {
            if (!comboActive)
                return;

            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                ResetCombo("Timeout");
            }
        }

        private static string CalculateRank(int combo)
        {
            if (combo >= 10) return "A";
            if (combo >= 6) return "B";
            if (combo >= 3) return "C";
            if (combo >= 1) return "D";
            return "-";
        }
    }
}
