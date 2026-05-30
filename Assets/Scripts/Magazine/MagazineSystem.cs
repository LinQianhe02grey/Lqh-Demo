using System;
using System.Collections.Generic;
using UnityEngine;
using Cardwin.Cards;

namespace Cardwin.Magazine
{
    public class MagazineSystem : MonoBehaviour
    {
        [Header("Magazine Config")]
        public int capacity = 8;
        public float reloadTime = 1.2f;

        [Header("Source Pool (random draw pool)")]
        public List<CardData> initialCards = new();

        [Header("Optional CardDatabase Source")]
        public CardDatabase cardDatabase;
        public bool useDatabaseAsSource = false;

        [Header("Shuffle Rules")]
        public bool shuffleOnReload = true;
        public bool allowRepeatWhenNotEnoughCards = true;

        [Header("Context")]
        public PlayerCardContext context;
        public CardEffectExecutor cardExecutor;

        public List<CardData> LoadedCards { get; private set; } = new();
        public List<CardData> LoadoutCards { get; private set; } = new();
        public int CurrentIndex { get; private set; }
        public bool IsReloading { get; private set; }
        public int Capacity => capacity;
        public float ReloadProgress => IsReloading ? 1f - (_reloadTimer / reloadTime) : 1f;

        public event Action OnMagazineChanged;
        public event Action OnReloadStarted;
        public event Action OnReloadFinished;

        private float _reloadTimer;
        private bool _hasUserLoadoutInit;

        private void Start()
        {
            if (!_hasUserLoadoutInit && LoadoutCards.Count == 0 && initialCards.Count > 0)
            {
                int count = Mathf.Min(capacity, initialCards.Count);
                for (int i = 0; i < count; i++)
                    if (initialCards[i] != null)
                        LoadoutCards.Add(initialCards[i]);
                Debug.Log($"[Magazine] Initialized loadout from initialCards. Count={LoadoutCards.Count}");
            }

            if (LoadedCards.Count == 0)
            {
                if (LoadoutCards.Count > 0)
                {
                    BuildRandomMagazine();
                    CurrentIndex = 0;
                    IsReloading = false;
                    OnMagazineChanged?.Invoke();
                    Debug.Log($"[Magazine] Initialized random magazine. Count={LoadedCards.Count}");
                }
                else if (!_hasUserLoadoutInit && initialCards.Count > 0)
                {
                    BuildRandomMagazineFallback();
                    CurrentIndex = 0;
                    IsReloading = false;
                    OnMagazineChanged?.Invoke();
                    Debug.Log($"[Magazine] Initialized magazine from initialCards fallback. Count={LoadedCards.Count}");
                }
            }

            if (LoadoutCards.Count == 0 && initialCards.Count == 0)
                Debug.LogWarning("[Magazine] No cards available. HUD will show Empty.");
        }

        private void Update()
        {
            if (IsReloading)
            {
                _reloadTimer -= Time.deltaTime;
                if (_reloadTimer <= 0f)
                    FinishReload();
            }
        }

        public void SetLoadoutCards(List<CardData> cards)
        {
            LoadoutCards.Clear();
            if (cards != null)
            {
                int count = Mathf.Min(capacity, cards.Count);
                for (int i = 0; i < count; i++)
                {
                    if (cards[i] != null)
                        LoadoutCards.Add(cards[i]);
                }
            }

            _hasUserLoadoutInit = true;

            string names = "";
            for (int i = 0; i < LoadoutCards.Count; i++)
                names += LoadoutCards[i].cardName + (i < LoadoutCards.Count - 1 ? ", " : "");
            Debug.Log($"[Magazine] Loadout updated: {names}");

            BuildRandomMagazine();
            CurrentIndex = 0;
            IsReloading = false;
            OnMagazineChanged?.Invoke();
        }

        public List<CardData> GetLoadoutCards()
        {
            return LoadoutCards;
        }

        public List<CardData> GetLoadedCards()
        {
            return LoadedCards;
        }

        public void InitializeDefaultLoadoutIfEmpty(CardDatabase db)
        {
            if (LoadoutCards.Count > 0)
                return;

            if (db == null)
            {
                Debug.LogWarning("[Magazine] CardDatabase is null. Cannot initialize default loadout.");
                return;
            }

            CardData strike = db.GetByName("Strike");
            CardData guard = db.GetByName("Guard");
            CardData heal = db.GetByName("Heal");
            CardData focus = db.GetByName("Focus");

            CardData[] order = { strike, guard, heal, focus, strike, strike, guard, strike };
            LoadoutCards.Clear();
            foreach (CardData c in order)
            {
                if (c != null)
                    LoadoutCards.Add(c);
            }

            if (LoadoutCards.Count > 0)
            {
                _hasUserLoadoutInit = true;
                Debug.Log($"[Magazine] Default loadout initialized. Count={LoadoutCards.Count}");
                BuildRandomMagazine();
                CurrentIndex = 0;
                IsReloading = false;
                OnMagazineChanged?.Invoke();
            }
            else
            {
                Debug.LogWarning("[Magazine] Default loadout is empty. CardDatabase cards not found.");
            }
        }

        public void SetMagazineCards(List<CardData> cards)
        {
            LoadedCards.Clear();
            int count = Mathf.Min(capacity, cards.Count);
            for (int i = 0; i < count; i++)
                LoadedCards.Add(cards[i]);

            CurrentIndex = 0;
            IsReloading = false;
            OnMagazineChanged?.Invoke();

            Debug.Log($"[Magazine] SetMagazineCards count={LoadedCards.Count}");
        }

        public void BuildRandomMagazine()
        {
            List<CardData> source = ResolveSourcePool();
            if (source.Count == 0)
            {
                Debug.LogWarning("[Magazine] No source cards to build magazine from.");
                LoadedCards.Clear();
                return;
            }

            LoadedCards.Clear();

            if (source.Count >= capacity)
            {
                List<CardData> pool = new List<CardData>(source);
                for (int i = pool.Count - 1; i > 0; i--)
                {
                    int j = UnityEngine.Random.Range(0, i + 1);
                    CardData temp = pool[i];
                    pool[i] = pool[j];
                    pool[j] = temp;
                }
                for (int i = 0; i < capacity; i++)
                    LoadedCards.Add(pool[i]);
            }
            else if (allowRepeatWhenNotEnoughCards)
            {
                for (int i = 0; i < capacity; i++)
                    LoadedCards.Add(source[UnityEngine.Random.Range(0, source.Count)]);
            }
            else
            {
                for (int i = 0; i < source.Count; i++)
                    LoadedCards.Add(source[i]);
                Debug.LogWarning($"[Magazine] Only {LoadedCards.Count} cards loaded (source pool smaller than capacity).");
            }

            string names = "";
            for (int i = 0; i < LoadedCards.Count; i++)
                names += LoadedCards[i].cardName + (i < LoadedCards.Count - 1 ? ", " : "");
            Debug.Log($"[Magazine] Random loaded from loadout: {names}");
        }

        private List<CardData> ResolveSourcePool()
        {
            if (_hasUserLoadoutInit)
            {
                if (LoadoutCards.Count > 0)
                {
                    List<CardData> pool = new List<CardData>(LoadoutCards);
                    Debug.Log($"[Magazine] ResolveSourcePool: using loadoutCards. Count={pool.Count}");
                    return pool;
                }

                Debug.LogWarning("[Magazine] Loadout is empty. No cards can be loaded.");
                return new List<CardData>();
            }

            if (LoadoutCards.Count > 0)
            {
                List<CardData> pool = new List<CardData>(LoadoutCards);
                Debug.Log($"[Magazine] ResolveSourcePool: using loadoutCards. Count={pool.Count}");
                return pool;
            }

            if (useDatabaseAsSource && cardDatabase != null)
            {
                List<CardData> pool = new List<CardData>();
                foreach (CardData cd in cardDatabase.allCards)
                    if (cd != null)
                        pool.Add(cd);
                if (pool.Count > 0)
                {
                    Debug.Log($"[Magazine] ResolveSourcePool: using cardDatabase. Count={pool.Count}");
                    return pool;
                }

                Debug.LogWarning("[Magazine] useDatabaseAsSource=true but CardDatabase has no cards. Falling back to initialCards.");
            }

            Debug.Log($"[Magazine] ResolveSourcePool: using initialCards. Count={initialCards.Count}");
            return initialCards;
        }

        private void BuildRandomMagazineFallback()
        {
            List<CardData> source = initialCards;
            if (source.Count == 0)
            {
                LoadedCards.Clear();
                return;
            }

            LoadedCards.Clear();

            if (source.Count >= capacity)
            {
                List<CardData> pool = new List<CardData>(source);
                for (int i = pool.Count - 1; i > 0; i--)
                {
                    int j = UnityEngine.Random.Range(0, i + 1);
                    CardData temp = pool[i];
                    pool[i] = pool[j];
                    pool[j] = temp;
                }
                for (int i = 0; i < capacity; i++)
                    LoadedCards.Add(pool[i]);
            }
            else if (allowRepeatWhenNotEnoughCards)
            {
                for (int i = 0; i < capacity; i++)
                    LoadedCards.Add(source[UnityEngine.Random.Range(0, source.Count)]);
            }
            else
            {
                for (int i = 0; i < source.Count; i++)
                    LoadedCards.Add(source[i]);
            }
        }

        public CardData GetCurrentCard()
        {
            if (LoadedCards.Count == 0)
                return null;
            if (IsReloading)
                return null;
            if (CurrentIndex >= LoadedCards.Count)
                return null;

            return LoadedCards[CurrentIndex];
        }

        public List<CardData> GetPreviewCards(int count)
        {
            List<CardData> preview = new List<CardData>();
            for (int i = 0; i < count; i++)
            {
                int idx = CurrentIndex + i;
                if (idx < LoadedCards.Count)
                    preview.Add(LoadedCards[idx]);
            }
            return preview;
        }

        public void UseCurrentCardLeft()
        {
            if (IsReloading)
            {
                Debug.Log("[Magazine] Cannot use card: Reloading");
                return;
            }

            CardData card = GetCurrentCard();
            if (card == null)
            {
                Debug.Log("[Magazine] Cannot use card: No current card");
                return;
            }

            Debug.Log($"[Magazine] UseLeft card={card.cardName} index={CurrentIndex}");

            if (cardExecutor != null && context != null)
                cardExecutor.ExecuteLeft(card, context);

            AdvanceIndex();
        }

        public void UseCurrentCardRight()
        {
            if (IsReloading)
            {
                Debug.Log("[Magazine] Cannot use card: Reloading");
                return;
            }

            CardData card = GetCurrentCard();
            if (card == null)
            {
                Debug.Log("[Magazine] Cannot use card: No current card");
                return;
            }

            Debug.Log($"[Magazine] UseRight card={card.cardName} index={CurrentIndex}");

            if (cardExecutor != null && context != null)
                cardExecutor.ExecuteRight(card, context);

            AdvanceIndex();
        }

        public void ManualReload()
        {
            if (IsReloading)
            {
                Debug.Log("[Magazine] Already reloading");
                return;
            }

            StartReload();
        }

        private void AdvanceIndex()
        {
            CurrentIndex++;

            if (CurrentIndex >= LoadedCards.Count)
            {
                Debug.Log("[Magazine] Magazine empty, starting reload");
                StartReload();
            }
            else
            {
                CardData next = GetCurrentCard();
                Debug.Log($"[Magazine] Advance to index={CurrentIndex} card={next?.cardName ?? "null"}");
                OnMagazineChanged?.Invoke();
            }
        }

        private void StartReload()
        {
            IsReloading = true;
            _reloadTimer = reloadTime;
            OnReloadStarted?.Invoke();
            Debug.Log($"[Magazine] Reload started ({reloadTime}s)");
        }

        private void FinishReload()
        {
            if (shuffleOnReload)
                BuildRandomMagazine();

            CurrentIndex = 0;
            IsReloading = false;
            OnReloadFinished?.Invoke();
            OnMagazineChanged?.Invoke();

            CardData current = GetCurrentCard();
            Debug.Log($"[Magazine] Reload finished. Current card={current?.cardName ?? "null"}");
        }
    }
}
