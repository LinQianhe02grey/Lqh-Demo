using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cardwin.Cards;
using Cardwin.Magazine;

namespace Cardwin.UI
{
    public class MagazineFullBarUI : MonoBehaviour
    {
        public MagazineSystem magazineSystem;
        public int slotCount = 8;

        public CardSlotUI[] slots;
        public Text titleText;
        public Text indexText;

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
                magazineSystem.OnMagazineChanged -= RefreshFullBar;
                magazineSystem.OnReloadStarted -= HandleReloadStarted;
                magazineSystem.OnReloadFinished -= HandleReloadFinished;
            }

            magazineSystem = system;
            magazineSystem.OnMagazineChanged += RefreshFullBar;
            magazineSystem.OnReloadStarted += HandleReloadStarted;
            magazineSystem.OnReloadFinished += HandleReloadFinished;
            _isSubscribed = true;

            RefreshFullBar();
        }

        public void RefreshFullBar()
        {
            if (magazineSystem == null)
                return;

            EnsureSlotsExist();
            if (slots == null)
                return;

            int currentIdx = magazineSystem.CurrentIndex;
            List<CardData> cards = magazineSystem.LoadedCards;

            if (titleText != null)
            {
                string cardName = currentIdx < cards.Count ? cards[currentIdx].cardName : "---";
                titleText.text = $"Index {currentIdx}/{cards.Count}  Card: {cardName}";
            }

            for (int i = 0; i < slots.Length; i++)
            {
                if (i < cards.Count)
                {
                    bool current = i == currentIdx && !magazineSystem.IsReloading;
                    bool used = i < currentIdx;
                    slots[i].SetCard(cards[i], current, used);
                }
                else
                {
                    slots[i].SetEmpty();
                }
            }

            Debug.Log($"[MagazineFullBarUI] Refresh full bar. Count={cards.Count} Current={currentIdx}");
        }

        private void HandleReloadStarted()
        {
            if (titleText != null)
                titleText.text = "Reloading";

            if (indexText != null)
                indexText.text = "Reloading";

            if (slots != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i] != null)
                        slots[i].SetReloading();
                }
            }
        }

        private void HandleReloadFinished()
        {
            RefreshFullBar();
        }

        private void EnsureSlotsExist()
        {
            if (slots != null && slots.Length == slotCount)
                return;

            List<CardSlotUI> list = new List<CardSlotUI>();

            for (int i = 0; i < slotCount; i++)
            {
                string slotName = $"FullSlot_{i}";
                Transform existingSlot = transform.Find(slotName);
                GameObject slotObj;
                if (existingSlot != null)
                {
                    slotObj = existingSlot.gameObject;
                }
                else
                {
                    slotObj = new GameObject(slotName);
                    slotObj.transform.SetParent(transform, false);
                    var rt = slotObj.AddComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(95, 50);
                    var img = slotObj.AddComponent<Image>();
                    img.color = new Color(0.5f, 0.5f, 0.5f, 0.12f);
                    img.raycastTarget = false;
                }

                CardSlotUI slot = slotObj.GetComponent<CardSlotUI>();
                if (slot == null)
                    slot = slotObj.AddComponent<CardSlotUI>();

                slot.backgroundImage = slotObj.GetComponent<Image>();
                if (slot.backgroundImage == null)
                {
                    slot.backgroundImage = slotObj.AddComponent<Image>();
                    slot.backgroundImage.color = new Color(0.5f, 0.5f, 0.5f, 0.12f);
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
                    var nrt = nameGo.GetComponent<RectTransform>();
                    nrt.anchorMin = new Vector2(0f, 0.45f);
                    nrt.anchorMax = new Vector2(1f, 1f);
                    nrt.offsetMin = new Vector2(4f, 0f);
                    nrt.offsetMax = new Vector2(-4f, -4f);
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
                    txt.color = new Color(0.5f, 0.5f, 0.5f);
                    txt.raycastTarget = false;
                    var ert = effGo.GetComponent<RectTransform>();
                    ert.anchorMin = new Vector2(0f, 0f);
                    ert.anchorMax = new Vector2(1f, 0.45f);
                    ert.offsetMin = new Vector2(4f, 4f);
                    ert.offsetMax = new Vector2(-4f, 0f);
                    slot.effectText = txt;
                }

                list.Add(slot);
            }

            slots = list.ToArray();
        }

        private void OnDestroy()
        {
            if (magazineSystem != null)
            {
                magazineSystem.OnMagazineChanged -= RefreshFullBar;
                magazineSystem.OnReloadStarted -= HandleReloadStarted;
                magazineSystem.OnReloadFinished -= HandleReloadFinished;
            }
        }
    }
}
