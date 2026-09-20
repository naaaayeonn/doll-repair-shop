using System;
using UnityEngine;

namespace DollShop.Core
{
    // ── Enums ───────────────────────────────────────────────
    public enum DollId       { RABBIT, OCTOPUS, BEAR }
    public enum DollStage    { Attack = 0, AtDoor = 1, Approaching = 2, Idle = 3 }
    public enum CounterType  { IMMEDIATE, TOOL, PREVENT }
    public enum RoomId       { WORKSHOP, SHOWROOM, FRONT, MYROOM }
    public enum MinigameType { SEAM, BUTTON, CUT }
    public enum ClueChannel  { SIGHT, SOUND, TRACE }
    public enum DayEndReason { COMPLETE, TIMEOUT, DEATH, FAINT_LIMIT }
    public enum ToolId       { SCISSORS, THREAD, KNIFE }
    public enum BuyResult    { OK, NO_GOLD, OWNED, NO_SPACE }
    public enum SanityCause  { TICK, ANOMALY_MISS, DOOR_STAGE, COUNTER_FAIL, ITEM }
    public enum CounterAction { RESTITCH, REFILL_THREAD, CUT_THREAD, REARRANGE, STORE_KNIFE }

    // ── ID 타입 ─────────────────────────────────────────────
    /// <summary>의뢰 고유 ID</summary>
    public readonly struct OrderId : IEquatable<OrderId>
    {
        private readonly int _value;
        public OrderId(int v) { _value = v; }
        public bool Equals(OrderId other) => _value == other._value;
        public override bool Equals(object obj) => obj is OrderId o && Equals(o);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => $"Order#{_value}";
        public static bool operator ==(OrderId a, OrderId b) => a._value == b._value;
        public static bool operator !=(OrderId a, OrderId b) => !(a == b);
    }

    /// <summary>이상현상 고유 ID 문자열 래퍼</summary>
    public readonly struct AnomalyId : IEquatable<AnomalyId>
    {
        private readonly string _value;
        public AnomalyId(string v) { _value = v; }
        public bool Equals(AnomalyId other) => _value == other._value;
        public override bool Equals(object obj) => obj is AnomalyId o && Equals(o);
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
        public override string ToString() => _value ?? "";
        public static bool operator ==(AnomalyId a, AnomalyId b) => a._value == b._value;
        public static bool operator !=(AnomalyId a, AnomalyId b) => !(a == b);
    }

    /// <summary>단서 고유 ID 문자열 래퍼</summary>
    public readonly struct ClueId : IEquatable<ClueId>
    {
        private readonly string _value;
        public ClueId(string v) { _value = v; }
        public bool Equals(ClueId other) => _value == other._value;
        public override bool Equals(object obj) => obj is ClueId o && Equals(o);
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
        public override string ToString() => _value ?? "";
        public static bool operator ==(ClueId a, ClueId b) => a._value == b._value;
        public static bool operator !=(ClueId a, ClueId b) => !(a == b);
        public static readonly ClueId Empty = new ClueId("");
        public bool IsEmpty => string.IsNullOrEmpty(_value);
    }

    /// <summary>아이템 ID 문자열 래퍼</summary>
    public readonly struct ItemId : IEquatable<ItemId>
    {
        private readonly string _value;
        public ItemId(string v) { _value = v; }
        public bool Equals(ItemId other) => _value == other._value;
        public override bool Equals(object obj) => obj is ItemId o && Equals(o);
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
        public override string ToString() => _value ?? "";
        public static bool operator ==(ItemId a, ItemId b) => a._value == b._value;
        public static bool operator !=(ItemId a, ItemId b) => !(a == b);
    }

    // ── 공용 구조체 ─────────────────────────────────────────
    public readonly struct RepairRequirement
    {
        public readonly string PartKey;
        public readonly string DemandKey;
        public RepairRequirement(string partKey, string demandKey)
        {
            PartKey = partKey;
            DemandKey = demandKey;
        }
    }

    public readonly struct Order
    {
        public readonly OrderId Id;
        public readonly string DollVariantId;          // 아트 에셋 키. 예: "VAR_BEAR_01"
        public readonly MinigameType Minigame;
        public readonly RepairRequirement[] Requirements; // 2~3개
        public readonly float TargetSeconds;

        public Order(OrderId id, string dollVariantId, MinigameType minigame,
                     RepairRequirement[] requirements, float targetSeconds)
        {
            Id = id;
            DollVariantId = dollVariantId;
            Minigame = minigame;
            Requirements = requirements;
            TargetSeconds = targetSeconds;
        }
    }

    public readonly struct RepairInput
    {
        public readonly float Accuracy;  // 0~100
        public readonly float Seconds;
        public readonly bool Aborted;
        public RepairInput(float accuracy, float seconds, bool aborted)
        {
            Accuracy = accuracy;
            Seconds = seconds;
            Aborted = aborted;
        }
    }

    public readonly struct OrderResult
    {
        public readonly bool Success;
        public readonly int Reward;
        public readonly bool Complaint;
        public OrderResult(bool success, int reward, bool complaint)
        {
            Success = success;
            Reward = reward;
            Complaint = complaint;
        }
    }

    public readonly struct CounterResult
    {
        public readonly bool Success;
        public readonly string FailReason; // "NO_TOOL", "WRONG_TYPE", "WRONG_TIMING"
        public CounterResult(bool success, string failReason = "")
        {
            Success = success;
            FailReason = failReason;
        }
    }

    public readonly struct ClueInfo
    {
        public readonly DollId Doll;
        public readonly ClueChannel Channel;
        public readonly string HintKey;
        public ClueInfo(DollId doll, ClueChannel channel, string hintKey)
        {
            Doll = doll;
            Channel = channel;
            HintKey = hintKey;
        }
    }

    public readonly struct AnomalyInfo
    {
        public readonly AnomalyId Id;
        public readonly RoomId Room;
        public readonly int LimitTick;
        public AnomalyInfo(AnomalyId id, RoomId room, int limitTick)
        {
            Id = id;
            Room = room;
            LimitTick = limitTick;
        }
    }

    public readonly struct DaySettlement
    {
        public readonly int Day;
        public readonly int Earned;
        public readonly int Penalty;
        public readonly int Bonus;
        public readonly int Total;
        public DaySettlement(int day, int earned, int penalty, int bonus)
        {
            Day = day;
            Earned = earned;
            Penalty = penalty;
            Bonus = bonus;
            Total = earned - penalty + bonus;
        }
    }
}
