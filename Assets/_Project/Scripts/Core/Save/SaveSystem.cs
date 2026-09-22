using System;
using System.IO;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// JSON 저장/로드. 저장 시점: Day 시작·종료·앱 포커스 아웃.
    /// 손상 파일 감지 시 save.bak 폴백.
    /// </summary>
    public static class SaveSystem
    {
        private static readonly string SAVE_PATH =
            Path.Combine(Application.persistentDataPath, "save.json");
        private static readonly string BACKUP_PATH =
            Path.Combine(Application.persistentDataPath, "save.bak");

        private const int CURRENT_SCHEMA_VERSION = 1;

        public static void Save()
        {
            try
            {
                var data = BuildSaveData();
                string json = JsonUtility.ToJson(data, prettyPrint: false);
                File.WriteAllText(SAVE_PATH, json);
                // 쓰기 성공 → 백업 복사
                File.Copy(SAVE_PATH, BACKUP_PATH, overwrite: true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Save 실패: {e.Message}");
            }
        }

        public static bool Load()
        {
            SaveData data = TryLoad(SAVE_PATH) ?? TryLoad(BACKUP_PATH);
            if (data == null)
            {
                Debug.LogWarning("[SaveSystem] 저장 파일 없음. NewGame 처리.");
                NewGame();
                return false;
            }

            data = Migrate(data);
            ApplySaveData(data);
            return true;
        }

        public static void NewGame()
        {
            GameState.StartNewRun();
            GameState.SetPersistent(new LoopPersistentData());
            NoteSystem.LoadFrom(new System.Collections.Generic.List<string>());
        }

        // ── 내부 ────────────────────────────────────────────
        private static SaveData TryLoad(string path)
        {
            if (!File.Exists(path)) return null;
            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch
            {
                return null;
            }
        }

        private static SaveData Migrate(SaveData data)
        {
            // TODO: W5 — 버전별 마이그레이션 (v1 → v2 등)
            return data;
        }

        private static SaveData BuildSaveData()
        {
            var data = new SaveData
            {
                schemaVersion = CURRENT_SCHEMA_VERSION
            };

            var p = GameState.Persistent;
            data.loop.unlockedClues = new System.Collections.Generic.List<string>(p.UnlockedClues);
            data.loop.deathCount    = p.DeathCount;
            data.loop.seenCutscenes = new System.Collections.Generic.List<string>(p.SeenCutscenes);
            data.loop.maxDayReached = p.MaxDayReached;

            data.run.day        = GameState.CurrentDay;
            data.run.gold       = GameState.Gold;
            data.run.sanity     = GameState.Sanity;
            data.run.faintCount = GameState.FaintCount;
            data.run.tick       = GameState.CurrentTick;
            data.run.ordersDone = GameState.OrdersDone;

            return data;
        }

        private static void ApplySaveData(SaveData data)
        {
            // 루프 퍼시스턴트 복원
            var persistent = new LoopPersistentData
            {
                UnlockedClues = data.loop.unlockedClues,
                DeathCount    = data.loop.deathCount,
                SeenCutscenes = data.loop.seenCutscenes,
                MaxDayReached = data.loop.maxDayReached
            };
            GameState.SetPersistent(persistent);

            // 런 스코프 복원
            GameState.CurrentDay  = data.run.day;
            GameState.Gold        = data.run.gold;
            GameState.Sanity      = data.run.sanity;
            GameState.FaintCount  = data.run.faintCount;
            GameState.CurrentTick = data.run.tick;
            GameState.OrdersDone  = data.run.ordersDone;

            NoteSystem.LoadFrom(data.loop.unlockedClues);
        }
    }
}
