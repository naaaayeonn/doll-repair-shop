# 인형수선소 야간 알바 — AI 페어 프로그래밍 컨텍스트

## 프로젝트
- 모바일 2.5D 규칙 기반 생존 호러, 싱글플레이
- Unity 6 LTS / URP / 세로 1080×1920

## 아키텍처 원칙

### Core (DEV-A 소유)
- 경로: `Assets/_Project/Scripts/Core/`
- asmdef: `DollShop.Core`
- **MonoBehaviour 금지** (ScriptableObject 제외)
- UnityEngine.UI, GameObject.Find, Camera, Transform 금지
- 수치 하드코딩 금지 → 반드시 CSV → ScriptableObject에서 읽기

### View (DEV-B 소유)
- 경로: `Assets/_Project/Scripts/View/`
- asmdef: `DollShop.View`
- Core의 필드 직접 접근 금지 → 읽기 전용 프로퍼티만 조회
- 규칙 판단 금지 → Core에 위임

## 인터페이스 계약 요약
```
Core ──이벤트(On~)──▶ View   (상태 변화 알림)
Core ◀──메서드호출──── View   (플레이어 의도 전달 → Result 반환)
```

### 핵심 이벤트 (Core → View)
- `DayTimer.OnTick(int)` — 매 틱
- `SanitySystem.OnChanged(float,float)` — 정신력 변화
- `DollRuleSystem.OnClueEmitted(ClueInfo)` — 공정성 단서 (3중 표현 필수)
- `DollRuleSystem.OnAttack(DollId)` — 사망 컷신 트리거
- `OrderSystem.OnOrderIssued(Order)` — 주문서 발행

### 핵심 메서드 (View → Core)
- `OrderSystem.SubmitRepair(OrderId, RepairInput)` → `OrderResult`
- `DollRuleSystem.TryCounter(DollId, CounterType)` → `CounterResult`
- `TimeService.SpendMove(RoomId)` / `BeginCctv(RoomId)` / `EndCctv()`

## 네이밍 규칙
| 대상 | 규칙 | 예 |
|---|---|---|
| 클래스 | PascalCase, 시스템은 `~System` | `SanitySystem` |
| 이벤트 | `On~` | `OnStageChanged` |
| 시도 메서드 | `Try~` | `TryCounter` |
| enum(데이터 ID) | UPPER_SNAKE | `RABBIT` |
| enum(상태) | PascalCase | `AtDoor` |
| CSV 컬럼 | camelCase | `moveInterval` |
| 데이터 ID | `분류_대상_번호` | `ANO_RABBIT_1` |

## 절대 금지
1. Core에서 View 참조
2. 수치 하드코딩 (CSV → SO를 통해)
3. 씬 파일 공동 수정 (DEV-B 단독 소유)
4. 인터페이스 시그니처 무단 변경 (DEV-B에게 먼저 알릴 것)

## 공정성 규칙 (코드로 강제됨)
- Stage 1 진입 시 `OnClueEmitted` 반드시 발행
- Stage 1 → Stage 0 최소 2틱 보장 (`attackGuard = 2`)
- TOOL 대응은 가위 미보유 시 `FailReason = "NO_TOOL"`

## 최신 API 문서
`docs/CORE_API.md` 참조
