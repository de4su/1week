using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Roguelike
{
    public class EnemyXP : MonoBehaviour
    {
        public int XPValue = 25;

        void Start()
        {
            Health health = GetComponent<Health>();
            if (health)
            {
                health.OnDie += RewardXP;
            }
        }

        void RewardXP()
        {
            if (XPManager.Instance)
            {
                XPManager.Instance.AddXP(XPValue);
            }
        }
    }
}
