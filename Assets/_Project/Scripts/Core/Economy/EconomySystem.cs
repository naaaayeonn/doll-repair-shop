using System;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>골드 증감 + 정산 계산식(§2.8).</summary>
    public static class EconomySystem
    {
        // 설정 컨테이너 (DayConfig에서 읽힘)
        public class Config
        {
            public int BasePricePerOrder = 40;
            public int ComplainPenalty   = 30;
            public int NoAccidentBonus   = 50;
        }

        public static int Gold { get; private set; } = 0;

        public static event Action<int, int>      OnGoldChanged; // (new, delta)
        public static event Action<DaySettlement> OnSettlement;

        private static int   _todayEarned  = 0;
        private static int   _todayPenalty = 0;
        private static Config _config       = new Config();

        public static void Initialize(Config config) => _config = config;

        public static void BeginDay()
        {
            _todayEarned  = 0;
            _todayPenalty = 0;
        }

        public static void AddGold(int earned, int complainDeduction)
        {
            int delta = earned + complainDeduction;
            Gold = Mathf.Max(0, Gold + delta);
            GameState.Gold = Gold;

            _todayEarned += Mathf.Max(0, earned);
            _todayPenalty += Mathf.Abs(Mathf.Min(0, complainDeduction));

            OnGoldChanged?.Invoke(Gold, delta);
        }

        public static void SettleDay(int day, bool noAccident)
        {
            int bonus = noAccident ? _config.NoAccidentBonus : 0;
            // 무사고 보너스 골드 추가
            if (bonus > 0)
            {
                Gold += bonus;
                GameState.Gold = Gold;
                OnGoldChanged?.Invoke(Gold, bonus);
            }

            var settlement = new DaySettlement(day, _todayEarned, _todayPenalty, bonus);
            OnSettlement?.Invoke(settlement);
        }
    }
}
