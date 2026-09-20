# CORE_API.md — 인터페이스 계약 최신판

> **변경 시 DEV-A·DEV-B 양쪽 문서를 같은 커밋에서 갱신한다.**
> 최종 권위: 이 파일 → `CLAUDE.md` 요약 순.

## 방향 규칙
```
Core (DEV-A) ──이벤트──▶ View (DEV-B)   상태 변화 알림
Core (DEV-A) ◀──메서드── View (DEV-B)   플레이어 의도 전달
```

## 이벤트 (Core → View)

```csharp
// 시간
DayTimer.CurrentTick : int
DayTimer.MaxTick     : int
DayTimer.IsPaused    : bool
DayTimer.OnTick      : Action<int>          // 현재 틱
DayTimer.OnDayEnd    : Action<DayEndReason> // COMPLETE/TIMEOUT/DEATH/FAINT_LIMIT

// 정신력
SanitySystem.Current    : float
SanitySystem.Max        : float
SanitySystem.FaintCount : int
SanitySystem.OnChanged     : Action<float,float>  // (current, max)
SanitySystem.OnDrained     : Action<SanityCause>  // 급감 연출 트리거
SanitySystem.OnFaint       : Action<int>          // 누적 기절 횟수
SanitySystem.OnDayRestart  : Action              // 기절 3회

// 의뢰
OrderSystem.Current    : Order?
OrderSystem.Remaining  : int
OrderSystem.TotalToday : int
OrderSystem.OnOrderIssued   : Action<Order>
OrderSystem.OnOrderJudged   : Action<OrderResult>
OrderSystem.OnAllOrdersDone : Action

// 인형
DollRuleSystem.StageOf(DollId)       : DollStage
DollRuleSystem.IsNeutralized(DollId) : bool
DollRuleSystem.OnStageChanged  : Action<DollId,DollStage>
DollRuleSystem.OnClueEmitted   : Action<ClueInfo>    // 공정성 단서 — 3중 표현 필수
DollRuleSystem.OnCounterResult : Action<DollId,bool>
DollRuleSystem.OnAttack        : Action<DollId>       // 사망 컷신 신호

// 이상현상
AnomalySystem.OnRaised   : Action<AnomalyInfo>
AnomalySystem.OnResolved : Action<AnomalyId,bool>
AnomalySystem.OnTicking  : Action<AnomalyId,int>  // 남은 틱

// 경제·상점
EconomySystem.Gold : int
EconomySystem.OnGoldChanged : Action<int,int>        // (new, delta)
EconomySystem.OnSettlement  : Action<DaySettlement>
ShopSystem.OnPurchased : Action<ItemId>
ShopSystem.OnPlaced    : Action<ItemId,Vector2Int>

// 진행·노트
DayFlow.CurrentDay : int
DayFlow.OnDayStarted     : Action<int>
DayFlow.OnScriptedEvent  : Action<string>
NoteSystem.Completion : float
NoteSystem.Unlocked   : IReadOnlyList<ClueId>
NoteSystem.OnClueAdded    : Action<ClueId>
NoteSystem.OnNoteCompleted : Action
LoopSystem.DeathCount : int
LoopSystem.OnRunEnded     : Action<DollId?>
LoopSystem.OnRunRestarted : Action
EndingSystem.IsEscapeUnlocked : bool
EndingSystem.OnEscapeUnlocked : Action
EndingSystem.OnEscaped        : Action
```

## 메서드 (View → Core)

```csharp
OrderSystem.SubmitRepair(OrderId, RepairInput)    -> OrderResult
DollRuleSystem.TryCounter(DollId, CounterType)   -> CounterResult
AnomalySystem.TryResolve(AnomalyId)              -> bool

TimeService.SpendMove(RoomId)        -> bool
TimeService.BeginCctv(RoomId)        -> void
TimeService.EndCctv()                -> void
TimeService.BeginInspect(DollId)     -> void
TimeService.EndInspect()             -> ClueId?
TimeService.SpendRepairTicks(float)  -> void
TimeService.SetPaused(bool)          -> void

ToolState.TryTake(ToolId)    -> bool
ToolState.TryStoreKnife()    -> bool

ShopSystem.TryBuy(ItemId)               -> BuyResult
ShopSystem.TryPlace(ItemId, Vector2Int) -> bool
ShopSystem.TryUse(ItemId)               -> bool

DayFlow.RequestNextDay()  -> void
EndingSystem.TryEscape()  -> bool
SaveSystem.Save()         -> void
SaveSystem.Load()         -> bool
SaveSystem.NewGame()      -> void
```

## 공용 타입
```csharp
enum DollId       { RABBIT, OCTOPUS, BEAR }
enum DollStage    { Attack=0, AtDoor=1, Approaching=2, Idle=3 }
enum CounterType  { IMMEDIATE, TOOL, PREVENT }
enum RoomId       { WORKSHOP, SHOWROOM, FRONT, MYROOM }
enum MinigameType { SEAM, BUTTON, CUT }
enum ClueChannel  { SIGHT, SOUND, TRACE }
enum DayEndReason { COMPLETE, TIMEOUT, DEATH, FAINT_LIMIT }
enum ToolId       { SCISSORS, THREAD, KNIFE }
enum BuyResult    { OK, NO_GOLD, OWNED, NO_SPACE }
enum SanityCause  { TICK, ANOMALY_MISS, DOOR_STAGE, COUNTER_FAIL, ITEM }

struct Order          { OrderId, DollVariantId, MinigameType, RepairRequirement[], TargetSeconds }
struct RepairInput    { Accuracy, Seconds, Aborted }
struct OrderResult    { Success, Reward, Complaint }
struct CounterResult  { Success, FailReason }
struct ClueInfo       { DollId, ClueChannel, HintKey }
struct AnomalyInfo    { AnomalyId, RoomId, LimitTick }
struct DaySettlement  { Day, Earned, Penalty, Bonus, Total }
```
