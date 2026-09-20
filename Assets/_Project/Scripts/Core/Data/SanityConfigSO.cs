using UnityEngine;

namespace DollShop.Core
{
    [CreateAssetMenu(fileName = "SanityConfigSO", menuName = "DollShop/Data/SanityConfig")]
    public class SanityConfigSO : ScriptableObject
    {
        public float maxSanity          = 100f;
        public float drainPerTick       = 1f;
        public float anomalyMissPenalty = 15f;
        public float doorStagePenalty   = 8f;
        public float counterFailPenalty = 20f;
        public float repairBonus        = 3f;
        public int   faintJumpTick      = 4;
        public float faintRestore       = 50f;
        public int   faintLimit         = 3;
    }
}
