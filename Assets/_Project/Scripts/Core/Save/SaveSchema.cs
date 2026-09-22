using System;
using System.Collections.Generic;

namespace DollShop.Core
{
    [Serializable]
    public class SaveData
    {
        public int          schemaVersion = 1;
        public LoopSave     loop          = new LoopSave();
        public RunSave      run           = new RunSave();
        public SettingsSave settings      = new SettingsSave();
    }

    [Serializable]
    public class LoopSave
    {
        public List<string> unlockedClues    = new List<string>();
        public int          deathCount       = 0;
        public List<string> seenCutscenes    = new List<string>();
        public int          maxDayReached    = 0;
        public List<string> unlockedCounters = new List<string>();
    }

    [Serializable]
    public class RunSave
    {
        public int            day          = 1;
        public int            gold         = 0;
        public float          sanity       = 100f;
        public int            faintCount   = 0;
        public int            tick         = 0;
        public int            ordersDone   = 0;
        public bool           knifeStored  = false;
        public List<string>   heldTools    = new List<string>();
        public List<string>   ownedItems   = new List<string>();
        public List<PlacedItemSave> placedItems = new List<PlacedItemSave>();
        public DollStageSave  dollStages   = new DollStageSave();
    }

    [Serializable]
    public class PlacedItemSave
    {
        public string id;
        public int    cellX;
        public int    cellY;
    }

    [Serializable]
    public class DollStageSave
    {
        public int RABBIT  = 3;
        public int OCTOPUS = 3;
        public int BEAR    = 3;
    }

    [Serializable]
    public class SettingsSave
    {
        public float bgm    = 0.8f;
        public float sfx    = 1.0f;
        public bool  haptic = true;
    }
}
