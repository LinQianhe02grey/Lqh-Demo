using UnityEngine;
using Cardwin.Combat;
using Cardwin.Characters;
using Cardwin.Lua;

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

            // Lua bullet channel (Stage 57): intercept before the old Projectile path so
            // the legacy projectile/card systems are completely untouched for normal cards.
            if (card.isLuaBullet)
            {
                SpawnLuaBullet(card, context);
                return;
            }

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
                TriggerProjectileVisual(effect, direction.x);
            }
            else
            {
                Debug.LogError("[CardEffect] Projectile component missing.");
                Destroy(projObj);
            }
        }

        private void SpawnLuaBullet(CardData card, PlayerCardContext context)
        {
            LuaBulletDefinition def = LuaBulletDatabase.Instance.GetBullet(card.luaBulletId);
            if (def == null || !def.Enabled)
            {
                // Disabled / unknown Lua bullet: do NOT fall back to a normal projectile.
                Debug.LogWarning($"[CardUse] Lua bullet '{card.luaBulletId}' is disabled or missing; not fired.");
                return;
            }

            Vector2 direction = context.GetShootDirectionToMouse();
            Vector3 spawnBase = context.firePoint != null
                ? context.firePoint.position
                : context.player.transform.position;
            Vector3 spawnPos = spawnBase + (Vector3)(direction * 0.3f);
            spawnPos.z = 0f;

            Debug.Log($"[CardUse] Left card={card.cardName} effect=LuaBullet id={def.Id} target=LuaBulletHost");
            LuaBulletHost.Spawn(def, spawnPos, direction, context);
            TriggerProjectileVisual(CardEffectType.Damage, direction.x);
        }

        public void ExecuteRight(CardData card, PlayerCardContext context)
        {
            if (card == null || context == null)
                return;

            CardEffectType effect = card.rightClickEffect;
            Debug.Log($"[CardUse] Right card={card.cardName} effect={effect} target=Self");

            ApplyEffectToTarget(card, effect, context.player, context);
            TriggerSelfVisual(effect);
        }

        private static void TriggerProjectileVisual(CardEffectType effect, float shotDirectionX)
        {
            if (effect == CardEffectType.Damage)
                CardVisualEventBus.Notify(VisualActionType.FireRed, shotDirectionX);
            else
                CardVisualEventBus.Notify(VisualActionType.FireBlue, shotDirectionX);
        }

        private static void TriggerSelfVisual(CardEffectType effect)
        {
            if (effect == CardEffectType.Damage)
                CardVisualEventBus.Notify(VisualActionType.SelfActionRed, 0f);
            else
                CardVisualEventBus.Notify(VisualActionType.SelfActionBlue, 0f);
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
