using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DollShop.Core
{
    /// <summary>
    /// 단서 9개 관리. 루프를 넘어 유지.
    /// 획득 3경로: 살펴보기(SIGHT) / CCTV Stage2 목격(SOUND) / 이상현상 대응(TRACE).
    /// </summary>
    public static class NoteSystem
    {
        public static float                 Completion { get; private set; } = 0f;
        public static IReadOnlyList<ClueId> Unlocked   => _unlocked.AsReadOnly();

        public static event Action<ClueId> OnClueAdded;
        public static event Action         OnNoteCompleted;

        private static readonly List<ClueId> _unlocked = new List<ClueId>();
        private const int TOTAL_CLUES = 9;

        public static void Initialize(ClueTableSO tableSO)
        {
            // tableSO 참조 보관 (추후 텍스트 로드용)
        }

        public static void LoadFrom(List<string> savedClueIds)
        {
            _unlocked.Clear();
            if (savedClueIds != null)
                foreach (var id in savedClueIds)
                    _unlocked.Add(new ClueId(id));
            RecalcCompletion();
        }

        public static bool TryAddClue(ClueId id)
        {
            if (id.IsEmpty) return false;
            foreach (var e in _unlocked)
                if (e == id) return false; // 중복 방지

            _unlocked.Add(id);
            RecalcCompletion();
            OnClueAdded?.Invoke(id);

            if (Completion >= 1f)
                OnNoteCompleted?.Invoke();

            // Loop Persistent 업데이트
            var p = GameState.Persistent;
            if (!p.UnlockedClues.Contains(id.ToString()))
                p.UnlockedClues.Add(id.ToString());

            return true;
        }

        /// <summary>CCTV로 인형 Stage 2 이하를 목격했을 때 SOUND 단서 획득 시도.</summary>
        public static void TryCctvClue(DollId doll, DollStage stage)
        {
            if ((int)stage <= (int)DollStage.Approaching) // Stage 2 이하
            {
                string clueId = $"CLUE_{doll}_SOUND";
                TryAddClue(new ClueId(clueId));
            }
        }

        private static void RecalcCompletion()
        {
            Completion = (float)_unlocked.Count / TOTAL_CLUES;
        }
    }
}
