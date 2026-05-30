using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cardwin.Cards;
using Cardwin.Magazine;

namespace Cardwin.UI
{
    public class MagazinePreviewUI : MonoBehaviour
    {
        public MagazineSystem magazineSystem;
        public int previewCount = 3;

        public CardSlotUI[] previewSlots;

        private bool _isSubscribed;

        private void Start()
        {
            if (magazineSystem != null)
                return;

            magazineSystem = FindObjectOfType<MagazineSystem>();
            if (magazineSystem != null)
                Bind(magazineSystem);
        }

        public void Bind(MagazineSystem system)
        {
            if (_isSubscribed && magazineSystem != null)
            {
                magazineSystem.OnMagazineChanged -= RefreshPreview;
                magazineSystem.OnReloadStarted -= OnReloadStarted;
                magazineSystem.OnReloadFinished -= OnReloadFinished;
            }

            magazineSystem = system;
            magazineSystem.OnMagazineChanged += RefreshPreview;
            magazineSystem.OnReloadStarted += OnReloadStarted;
            magazineSystem.OnReloadFinished += OnReloadFinished;
            _isSubscribed = true;

            RefreshPreview();
        }

        public void RefreshPreview()
        {
            if (magazineSystem == null)
                return;

            List<CardData> cards = magazineSystem.GetPreviewCards(previewCount);
            int currentIdx = magazineSystem.CurrentIndex;

            if (previewSlots == null || previewSlots.Length == 0)
            {
                EnsureSlotsExist();
                if (previewSlots == null)
                    return;
            }

            string previewNames = "";
            for (int i = 0; i < previewSlots.Length; i++)
            {
                if (i < cards.Count)
                {
                    previewSlots[i].SetCard(cards[i], i == 0 && currentIdx < magazineSystem.LoadedCards.Count);
                    previewNames += cards[i].cardName + (i < cards.Count - 1 ? ", " : "");
                }
                else
                {
                    previewSlots[i].SetEmpty();
                    previewNames += "---" + (i < previewSlots.Length - 1 ? ", " : "");
                }
            }

            Debug.Log($"[MagazinePreviewUI] Refresh: {previewNames}");
        }

        private void OnReloadStarted()
        {
            if (previewSlots != null)
            {
                foreach (CardSlotUI slot in previewSlots)
                    if (slot != null)
                        slot.SetReloading();
            }
        }

        private void OnReloadFinished()
        {
            RefreshPreview();
        }

        private void EnsureSlotsExist()
        {
            List<CardSlotUI> slots = new List<CardSlotUI>();

            for (int i = 0; i < previewCount; i++)
            {
                string slotName = $"PreviewSlot_{i}";
                Transform existingSlot = transform.Find(slotName);
                GameObject slotObj;
                if (existingSlot != null)
                {
                    slotObj = existingSlot.gameObject;
                }
                else
                {
                    slotObj = CreateSlotObject(slotName, transform);
                }

                CardSlotUI slot = slotObj.GetComponent<CardSlotUI>();
                if (slot == null)
                    slot = slotObj.AddComponent<CardSlotUI>();

                slot.backgroundImage = slotObj.GetComponent<Image>();
                if (slot.backgroundImage == null)
                {
                    slot.backgroundImage = slotObj.AddComponent<Image>();
                    slot.backgroundImage.color = new Color(0.5f, 0.5f, 0.5f, 0.15f);
                }
                slot.backgroundImage.raycastTarget = false;

                slot.nameText = slotObj.transform.Find("NameText")?.GetComponent<Text>();
                if (slot.nameText == null)
                {
                    GameObject nameGo = new GameObject("NameText");
                    nameGo.transform.SetParent(slotObj.transform, false);
                    Text txt = nameGo.AddComponent<Text>();
                    txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    txt.fontSize = 16;
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.color = Color.white;
                    txt.raycastTarget = false;
                    var nameRt = nameGo.GetComponent<RectTransform>();
                    nameRt.anchorMin = new Vector2(0f, 0.45f);
                    nameRt.anchorMax = new Vector2(1f, 1f);
                    nameRt.offsetMin = new Vector2(4f, 0f);
                    nameRt.offsetMax = new Vector2(-4f, -4f);
                    slot.nameText = txt;
                }

                slot.effectText = slotObj.transform.Find("EffectText")?.GetComponent<Text>();
                if (slot.effectText == null)
                {
                    GameObject effGo = new GameObject("EffectText");
                    effGo.transform.SetParent(slotObj.transform, false);
                    Text txt = effGo.AddComponent<Text>();
                    txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    txt.fontSize = 12;
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.color = new Color(0.6f, 0.6f, 0.6f);
                    txt.raycastTarget = false;
                    var effRt = effGo.GetComponent<RectTransform>();
                    effRt.anchorMin = new Vector2(0f, 0f);
                    effRt.anchorMax = new Vector2(1f, 0.45f);
                    effRt.offsetMin = new Vector2(4f, 4f);
                    effRt.offsetMax = new Vector2(-4f, 0f);
                    slot.effectText = txt;
                }

                RectTransform slotRt = slotObj.GetComponent<RectTransform>();
                if (slotRt == null)
                    slotRt = slotObj.AddComponent<RectTransform>();
                slotRt.sizeDelta = new Vector2(150, 60);

                slots.Add(slot);
            }

            previewSlots = slots.ToArray();
        }

        private static GameObject CreateSlotObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.5f, 0.5f, 0.5f, 0.15f);
            img.raycastTarget = false;

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 60);

            return obj;
        }

        private void OnDestroy()
        {
            if (magazineSystem != null)
            {
                magazineSystem.OnMagazineChanged -= RefreshPreview;
                magazineSystem.OnReloadStarted -= OnReloadStarted;
                magazineSystem.OnReloadFinished -= OnReloadFinished;
            }
        }
    }
}
