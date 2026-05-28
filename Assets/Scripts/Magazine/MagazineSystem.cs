using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Cardwin.Magazine
{
    public class MagazineSystem : MonoBehaviour
    {
        public int maxBulletPoolCapacity = 8;
        public int magazineBulletCount = 8;
        public float reloadTime = 1.5f;

        public List<string> BulletPool { get; private set; } = new();
        public List<string> LoadedMagazine { get; private set; } = new();
        public int CurrentBulletIndex { get; private set; }
        public bool IsReloading { get; private set; }

        public UnityEvent OnBulletChanged;
        public UnityEvent OnReloadStarted;
        public UnityEvent OnReloadFinished;

        public void Initialize() { }

        public void BuildShuffledLoadedMagazine() { }

        public bool TryGetCurrentBullet(out string cardId)
        {
            cardId = null;
            return false;
        }

        public void ConsumeCurrent() { }

        public void StartReload() { }

        private void FinishReload() { }

        public List<string> GetUpcomingBullets(int count = 3) { return new List<string>(); }

        public void SetBulletPool(List<string> cardIds) { }

        public void AddBullet(string cardId) { }

        public void RemoveBulletAt(int index) { }

        public void SwapSlots(int indexA, int indexB) { }
    }
}
