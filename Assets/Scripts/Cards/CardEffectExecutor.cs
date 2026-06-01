using UnityEngine;
using Cardwin.Combat;

namespace Cardwin.Cards
{
    public class CardEffectExecutor : MonoBehaviour
    {
        private PlayerCardContext _context;

        public void Initialize(PlayerCardContext context)
        {
            _context = context;
        }

        public void ExecuteLeft(CardData card, PlayerCardContext context)
        {
            if (card == null || context == null)
                return;

            CardEffectType effect = card.leftClickEffect;
            Debug.Log($"[CardUse] Left card={card.cardName} effect={effect} target=Projectile");

            GameObject prefab = card.projectilePrefab != null
                ? card.projectilePrefab
                : context.defaultProjectilePrefab;

            if (prefab == null)
            {
                Debug.LogError("[CardEffect] No projectilePrefab for left-click.");
                return;
            }

            Vector2 direction = context.GetShootDirectionToMouse();
            Vector3 spawnBase = context.firePoint != null
                ? context.firePoint.position
                : context.player.transform.position;
            Vector3 spawnPos = spawnBase + (Vector3)(direction * 0.3f);
            spawnPos.z = 0f;

            GameObject projObj = Instantiate(prefab, spawnPos, Quaternion.identity);
            projObj.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
            projObj.transform.localScale = Vector3.one * 0.8f;

            var proj = projObj.GetComponent<Cardwin.Combat.Projectile>();
            if (proj != null)
            {
                proj.Init(direction, card, effect, context);
            }
            else
            {
                Debug.LogError("[CardEffect] Projectile component missing.");
                Destroy(projObj);
            }
        }

        public void ExecuteRight(CardData card, PlayerCardContext context)
        {
            if (card == null || context == null)
                return;

            CardEffectType effect = card.rightClickEffect;
            Debug.Log($"[CardUse] Right card={card.cardName} effect={effect} target=Self");

            ApplyEffectToTarget(card, effect, context.player, context);
        }

        public void ApplyEffectToTarget(CardData card, CardEffectType effectType, GameObject target, PlayerCardContext context)
        {
            if (card == null || target == null)
                return;

            Health health = target.GetComponent<Health>();

            switch (effectType)
            {
                case CardEffectType.Damage:
                    if (health != null)
                    {
                        float focusMult = context.ConsumeFocusMultiplier();
                        int finalDamage = Mathf.RoundToInt(card.damage * focusMult);
                        health.TakeDamage(finalDamage);
                        Debug.Log($"[CardEffect] Apply Damage card={card.cardName} target={target.name} amount={finalDamage}");
                    }
                    break;

                case CardEffectType.Block:
                    if (health != null)
                    {
                        health.GainBlock(card.block);
                        Debug.Log($"[CardEffect] Apply Block card={card.cardName} target={target.name} amount={card.block}");
                    }
                    break;

                case CardEffectType.Heal:
                    if (health != null)
                    {
                        health.Heal(card.heal);
                        Debug.Log($"[CardEffect] Apply Heal card={card.cardName} target={target.name} amount={card.heal}");
                    }
                    break;

                case CardEffectType.Focus:
                    if (target == context.player)
                    {
                        context.AddFocus(card.focusGain);
                    }
                    else
                    {
                        Debug.LogWarning($"[CardEffect] Focus effect ignored on non-Player target: {target.name}");
                    }
                    break;

                case CardEffectType.None:
                default:
                    Debug.Log($"[CardEffect] No effect or unimplemented: {effectType}");
                    break;
            }
        }
    }
}
