using UnityEngine;

namespace Cardwin.Combat
{
    public class PlayerAlignment : MonoBehaviour
    {
        [SerializeField] private int good = 4;
        [SerializeField] private int evil = 4;

        public int Good => good;
        public int Evil => evil;

        public void SetGood(int value)
        {
            good = Mathf.Max(0, value);
        }

        public void SetEvil(int value)
        {
            evil = Mathf.Max(0, value);
        }

        public void SetValues(int goodValue, int evilValue)
        {
            good = Mathf.Max(0, goodValue);
            evil = Mathf.Max(0, evilValue);
        }
    }
}
