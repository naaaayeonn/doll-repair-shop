using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DollShop.Core
{
    /// <summary>단서 9개, 중복 방지, 완성도 계산. 루프를 넘어 유지된다.</summary>
    public static class NoteSystem
    {
        public static float                 Completion { get; private set; } = 0f;
        public static IReadOnlyList<ClueId> Unlocked   => _unlocked.AsReadOnly();

        public static event Action<ClueId> OnClueAdded;
        public static event Action         OnNoteCompleted;

        private static readonly List<ClueId> _unlocked = new List<ClueId>();
        private static ClueTableSO _tableSO;
        private const int TOTAL_CLUES = 9; // 인형 3종 × 채널 3종

        public static void Initialize(ClueTableSO tableSO)
        {
            _tableSO = tableSO;
        }

        public static void LoadFrom(List<string> savedClueIds)
        {
            _unlocked.Clear();
            foreach (var id in savedClueIds)
                _unlocked.Add(new ClueId(id));
            RecalcCompletion();
        }

        public static bool TryAddClue(ClueId id)
        {
            if (id.IsEmpty) return false;
            foreach (var existing in _unlocked)
                if (existing == id) return false; // 중복 방지

            _unlocked.Add(id);
            RecalcCompletion();
            OnClueAdded?.Invoke(id);

            if (Completion >= 1f)
                OnNoteCompleted?.Invoke();

            return true;
        }

        private static void RecalcCompletion()
        {
            Completion = (float)_unlocked.Count / TOTAL_CLUES;
        }
    }
}
