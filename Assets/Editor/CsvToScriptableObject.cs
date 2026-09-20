using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DollShop.Core;

namespace DollShop.Editor
{
    /// <summary>
    /// Tools/인형수선소/CSV 임포트 메뉴.
    /// Data/*.csv → 동일 이름의 .asset(ScriptableObject) 갱신.
    /// </summary>
    public static class CsvToScriptableObject
    {
        private const string DATA_PATH    = "Assets/_Project/Data";
        private const string SO_OUT_PATH  = "Assets/_Project/Data";

        [MenuItem("Tools/인형수선소/CSV 임포트 (전체)")]
        public static void ImportAll()
        {
            ImportDayConfig();
            ImportDollRules();
            ImportAnomalyTable();
            ImportOrderTemplates();
            ImportShopItems();
            ImportSanityConfig();
            ImportClueTable();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DollShop] CSV 전체 임포트 완료");
        }

        // ── DayConfig ────────────────────────────────────────
        [MenuItem("Tools/인형수선소/CSV 임포트/DayConfig")]
        public static void ImportDayConfig()
        {
            var csv  = LoadCsv("DayConfig.csv");
            if (csv == null) return;

            var so = GetOrCreateSO<DayConfigSO>("DayConfigSO");
            var list = new List<DayConfigSO.Entry>();

            for (int i = 1; i < csv.Count; i++)
            {
                var col = csv[i];
                if (col.Length < 7) continue;
                list.Add(new DayConfigSO.Entry
                {
                    day           = int.Parse(col[0]),
                    orderCount    = int.Parse(col[1]),
                    maxTick       = int.Parse(col[2]),
                    activeDolls   = col[3],
                    anomalyPoolIds = col[4],
                    introEventId  = col[5],
                    outroEventId  = col[6]
                });
            }
            so.entries = list.ToArray();
            EditorUtility.SetDirty(so);
            Debug.Log($"[DollShop] DayConfig 임포트: {list.Count}행");
        }

        // ── DollRules ────────────────────────────────────────
        [MenuItem("Tools/인형수선소/CSV 임포트/DollRules")]
        public static void ImportDollRules()
        {
            var csv = LoadCsv("DollRules.csv");
            if (csv == null) return;

            var so   = GetOrCreateSO<DollRulesSO>("DollRulesSO");
            var list = new List<DollRulesSO.Entry>();

            for (int i = 1; i < csv.Count; i++)
            {
                var col = csv[i];
                if (col.Length < 8) continue;
                list.Add(new DollRulesSO.Entry
                {
                    dollId          = col[0],
                    day             = int.Parse(col[1]),
                    active          = col[2] == "1",
                    startStage      = int.Parse(col[3]),
                    moveInterval    = int.Parse(col[4]),
                    counterType     = col[5],
                    clueStage       = int.Parse(col[6]),
                    attackDelayTick = int.Parse(col[7])
                });
            }
            so.entries = list.ToArray();
            EditorUtility.SetDirty(so);
            Debug.Log($"[DollShop] DollRules 임포트: {list.Count}행");
        }

        // ── AnomalyTable ─────────────────────────────────────
        [MenuItem("Tools/인형수선소/CSV 임포트/AnomalyTable")]
        public static void ImportAnomalyTable()
        {
            var csv = LoadCsv("AnomalyTable.csv");
            if (csv == null) return;

            var so   = GetOrCreateSO<AnomalyTableSO>("AnomalyTableSO");
            var list = new List<AnomalyTableSO.Entry>();

            for (int i = 1; i < csv.Count; i++)
            {
                var col = csv[i];
                if (col.Length < 8) continue;
                list.Add(new AnomalyTableSO.Entry
                {
                    anomalyId            = col[0],
                    ownerDoll            = col[1],
                    room                 = col[2],
                    triggerChancePerTick = float.Parse(col[3]),
                    limitTick            = int.Parse(col[4]),
                    counterAction        = col[5],
                    sanityPenalty        = int.Parse(col[6]),
                    intervalPenalty      = int.Parse(col[7])
                });
            }
            so.entries = list.ToArray();
            EditorUtility.SetDirty(so);
            Debug.Log($"[DollShop] AnomalyTable 임포트: {list.Count}행");
        }

        // ── OrderTemplates ───────────────────────────────────
        [MenuItem("Tools/인형수선소/CSV 임포트/OrderTemplates")]
        public static void ImportOrderTemplates()
        {
            var csv = LoadCsv("OrderTemplates.csv");
            if (csv == null) return;

            var so   = GetOrCreateSO<OrderTemplatesSO>("OrderTemplatesSO");
            var list = new List<OrderTemplatesSO.Entry>();

            for (int i = 1; i < csv.Count; i++)
            {
                var col = csv[i];
                if (col.Length < 6) continue;
                list.Add(new OrderTemplatesSO.Entry
                {
                    templateId       = col[0],
                    dollVariant      = col[1],
                    minigameType     = col[2],
                    requirementCount = int.Parse(col[3]),
                    targetSeconds    = float.Parse(col[4]),
                    basePrice        = int.Parse(col[5])
                });
            }
            so.entries = list.ToArray();
            EditorUtility.SetDirty(so);
            Debug.Log($"[DollShop] OrderTemplates 임포트: {list.Count}행");
        }

        // ── ShopItems ────────────────────────────────────────
        [MenuItem("Tools/인형수선소/CSV 임포트/ShopItems")]
        public static void ImportShopItems()
        {
            var csv = LoadCsv("ShopItems.csv");
            if (csv == null) return;

            var so   = GetOrCreateSO<ShopItemsSO>("ShopItemsSO");
            var list = new List<ShopItemsSO.Entry>();

            for (int i = 1; i < csv.Count; i++)
            {
                var col = csv[i];
                if (col.Length < 8) continue;
                list.Add(new ShopItemsSO.Entry
                {
                    itemId      = col[0],
                    nameKey     = col[1],
                    price       = int.Parse(col[2]),
                    effectType  = col[3],
                    effectValue = float.Parse(col[4]),
                    gridW       = int.Parse(col[5]),
                    gridH       = int.Parse(col[6]),
                    room        = col[7]
                });
            }
            so.entries = list.ToArray();
            EditorUtility.SetDirty(so);
            Debug.Log($"[DollShop] ShopItems 임포트: {list.Count}행");
        }

        // ── SanityConfig ─────────────────────────────────────
        [MenuItem("Tools/인형수선소/CSV 임포트/SanityConfig")]
        public static void ImportSanityConfig()
        {
            var csv = LoadCsv("SanityConfig.csv");
            if (csv == null) return;

            var so = GetOrCreateSO<SanityConfigSO>("SanityConfigSO");
            var dict = new Dictionary<string, float>();

            for (int i = 1; i < csv.Count; i++)
            {
                var col = csv[i];
                if (col.Length < 2) continue;
                if (float.TryParse(col[1], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float val))
                    dict[col[0]] = val;
            }

            if (dict.TryGetValue("maxSanity",          out float v)) so.maxSanity          = v;
            if (dict.TryGetValue("drainPerTick",       out v))       so.drainPerTick       = v;
            if (dict.TryGetValue("anomalyMissPenalty", out v))       so.anomalyMissPenalty = v;
            if (dict.TryGetValue("doorStagePenalty",   out v))       so.doorStagePenalty   = v;
            if (dict.TryGetValue("counterFailPenalty", out v))       so.counterFailPenalty = v;
            if (dict.TryGetValue("repairBonus",        out v))       so.repairBonus        = v;
            if (dict.TryGetValue("faintJumpTick",      out v))       so.faintJumpTick      = (int)v;
            if (dict.TryGetValue("faintRestore",       out v))       so.faintRestore       = v;
            if (dict.TryGetValue("faintLimit",         out v))       so.faintLimit         = (int)v;

            EditorUtility.SetDirty(so);
            Debug.Log("[DollShop] SanityConfig 임포트 완료");
        }

        // ── ClueTable ────────────────────────────────────────
        [MenuItem("Tools/인형수선소/CSV 임포트/ClueTable")]
        public static void ImportClueTable()
        {
            var csv = LoadCsv("ClueTable.csv");
            if (csv == null) return;

            var so   = GetOrCreateSO<ClueTableSO>("ClueTableSO");
            var list = new List<ClueTableSO.Entry>();

            for (int i = 1; i < csv.Count; i++)
            {
                var col = csv[i];
                if (col.Length < 5) continue;
                list.Add(new ClueTableSO.Entry
                {
                    clueId           = col[0],
                    dollId           = col[1],
                    channel          = col[2],
                    unlockCondition  = col[3],
                    noteText         = col[4]
                });
            }
            so.entries = list.ToArray();
            EditorUtility.SetDirty(so);
            Debug.Log($"[DollShop] ClueTable 임포트: {list.Count}행");
        }

        // ── 유틸리티 ────────────────────────────────────────
        private static List<string[]> LoadCsv(string filename)
        {
            string path = Path.Combine(Application.dataPath, "_Project", "Data", filename);
            if (!File.Exists(path))
            {
                Debug.LogError($"[CsvImporter] 파일 없음: {path}");
                return null;
            }

            var rows = new List<string[]>();
            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                rows.Add(ParseCsvLine(line));
            }
            return rows;
        }

        private static string[] ParseCsvLine(string line)
        {
            // 간단한 CSV 파서 (쉼표 분리, 따옴표 미지원 — 데이터에 쉼표 없으므로 충분)
            return line.Split(',');
        }

        private static T GetOrCreateSO<T>(string assetName) where T : ScriptableObject
        {
            string assetPath = $"{SO_OUT_PATH}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existing != null) return existing;

            var newSO = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(newSO, assetPath);
            return newSO;
        }
    }
}
