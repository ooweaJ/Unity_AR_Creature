# PRISM 작업 지시서 (Work Orders) v1

> 각 WO(Work Order)는 **1 PR 단위**의 독립 작업. Claude Code에 하나씩 던진다.
> 공통 규칙은 `CLAUDE.md`, 시그니처·맥락은 `PROJECT_PRISM_AR_설계문서_v1.md`(이하 "설계문서") 참조.
> 순서는 설계문서 부록 C "권장 구현 순서"를 따른다.

---

## WO-0 · 기반: 데이터 타입 + ScriptableObject + 프로젝트 골격

**목표**: 모든 후속 WO가 의존하는 공용 타입과 SO, 폴더 구조를 만든다.
**선행조건**: 없음.

**산출물 (`Scripts/Data/`)**
- enum: `ElementType`(Fire/Water/Grass/Electric/Neutral 등 5종), `Rarity`, `ActionType`(설계 8.3), `SessionTracking`(설계 7.7c).
- `CreatureStats`(atk/def/hp), `CreatureData`(설계 6.7, `[Serializable]`).
- SO: `CreatureArchetype`, `CreatureAnimationSet`(설계 8.3), `ReferenceImageBinding`, `CreatureDatabase`(`FindBinding`/`FindArchetype` 구현; 6.7).
- 빈 자리표시자 prefab 1개(큐브+Animator)와 샘플 `CreatureDatabase.asset` 1개(자리표시자 연결).
- 폴더 구조(`CLAUDE.md` 참조), 네임스페이스 `Prism.Data`.

**테스트**: `CreatureDatabase.FindBinding/ FindArchetype`의 조회·미스 케이스 EditMode 테스트.
**범위 밖**: 실제 크리처 에셋, 생성 로직, AR 코드.

---

## WO-1 · 인식: IImageTrackingService

**목표**: 등록 이미지 인식을 도메인 이벤트로 노출한다.
**선행조건**: WO-0.

**산출물**
- `Scripts/Data/`: `ImageTrackingState`(enum), `TrackedImageInfo`(struct, 설계 5.3).
- `Scripts/AR/`: `IImageTrackingService`(설계 5.3) + 구현체 `ImageTrackingService`(`ARTrackedImageManager` 래핑).
- 씬 세팅 가이드: `XROrigin`+`ARSession`+`ARTrackedImageManager` 구성 메모(README 또는 주석).
- 중복 인식 방지(쿨다운/플래그, 설계 5.4).

**동작 명세**
- `trackablesChanged`의 `added`에서 `TrackingState.Tracking`인 것만 `ImageRecognized` 발행.
- 추적 소실 시 `ImageTrackingLost(referenceImageName)`.
- `TrackedImageInfo`에 worldPose + screenBounds(픽셀 샘플링용) + referenceImageName 채움.

**테스트**: screenBounds 산출 헬퍼 등 순수 함수만 EditMode. (AR 이벤트 자체는 제외)
**범위 밖**: 생성/배치. Gameplay에서 AR 타입 노출 금지.

---

## WO-2 · 생성: ICameraImageService + IGenerationService

**목표**: 포획 순간 프레임을 받아 하이브리드 규칙으로 `CreatureData`를 만든다. (설계 6장)
**선행조건**: WO-0, WO-1.

**산출물**
- `Scripts/AR/`: `ICameraImageService` + 구현체(`ARCameraManager.TryAcquireLatestCpuImage`를 RGBA `CameraImageFrame`으로 변환, 설계 7.7a).
- `Scripts/Gameplay/`: `IGenerationService`+`GenerationService`(오케스트레이션 4단계, 설계 6.6), 내부 `IPixelSampler`+구현, `ITraitMapper`+구현.
- `Scripts/Data/`: `CameraImageFrame`(struct), `PixelStats`(struct), `GenerationRequest`(struct).

**동작 명세**
- `GenerationService.Generate`: (1) `CreatureDatabase.FindBinding(name)` (2) `IPixelSampler.Sample`(32×32 다운샘플, 평균 HSV·hue분산·팔레트2색) (3) `seed = Hash(name, 양자화 stats)` (4) `ITraitMapper.Map(binding, stats, seed)` → `CreatureData`.
- `GenerationService`와 `Scripts/Data/`는 `XRCpuImage` 등 AR Foundation 타입을 직접 참조하지 않는다. `ICameraImageService` 구현체가 `XRCpuImage` 획득·변환·Dispose를 끝내고 AR-neutral `CameraImageFrame`만 넘긴다.
- `ITraitMapper`: 베이스(아키타입/속성)는 binding에서, 변주(틴트·스탯±·희귀도·원소 미세보정)는 stats에서. 매핑 규칙 설계 6.4.
- 라이브 픽셀은 1회 계산 후 결과를 `CreatureData`에 확정(재현 불가 — 설계 6.3).

**테스트 (필수)**: `ITraitMapper.Map`을 더미 `PixelStats`로 — 같은 입력=같은 출력(결정성), hue→element 경계값, 희귀도 분기. `IPixelSampler`는 더미 바이트 배열로 통계 산출 검증.
**범위 밖**: 인스턴스화(CreatureFactory는 WO-5), UI 연출.

---

## WO-3 · 배치: IPlacementService

**목표**: 평면 레이캐스트 + 앵커 부착으로 안정적 배치 지점을 제공한다. (설계 7.1~7.2)
**선행조건**: WO-0.

**산출물**
- `Scripts/AR/`: `IPlacementService`+구현(`ARRaycastManager`/`ARAnchorManager` 래핑), `IAnchorHandle`+구현(`ARAnchor` 래핑, 설계 7.7b).

**동작 명세**
- `TryRaycastPlane(screenPos, out pose)`: `PlaneWithinPolygon` 히트.
- `CreateAnchor(pose)` → `IAnchorHandle`(Transform 노출, `TrackingStateChanged` 이벤트, `Dispose`).
- `PlaneAvailabilityChanged`로 배치 가능 평면 존재 여부 통지.
- 배치 서비스는 크리처를 만들지 않는다(앵커만).

**테스트**: 인터페이스 모킹 가능성 확인 수준. AR 동작은 제외.
**범위 밖**: 크리처 생성·이동 로직.

---

## WO-4 · 환경: IEnvironmentService (오클루전·광원·트래킹)

**목표**: 리얼리즘 레이어와 견고성. (설계 7.3~7.5)
**선행조건**: WO-0.

**산출물**
- `Scripts/Data/`: `LightReading`(struct, 7.7c).
- `Scripts/AR/`: `IEnvironmentService`+구현(`AROcclusionManager`/`ARCameraManager`/`ARSession`).

**동작 명세**
- `OcclusionSupported` 체크 → 미지원 시 `SetOcclusionEnabled` 무효 + 폴백 플래그.
- `frameReceived`의 `ARLightEstimationData` → `LightReading`(brightness/colorCorrection/mainLightDir) 발행.
- `ARSession.state` → `SessionTracking`(None/Limited/Tracking) 매핑 후 `SessionTrackingChanged`.
- URP 오클루전 셰이더 연동은 설정 메모로 남기고, 셰이더 자체 작성은 사람과 협의(범위 밖 가능).

**테스트**: `ARLightEstimationData`→`LightReading` 변환 등 순수 매핑 함수 EditMode.
**범위 밖**: 라이트/크리처에 실제 적용(소비측은 WO-5/통합 단계).

---

## WO-5 · 크리처: ICreatureController + CreatureFactory

**목표**: 배치된 크리처의 인스턴스화와 상호작용·애니메이션 제어. (설계 8장)
**선행조건**: WO-0. (배치 연동은 WO-3, 광원 소비는 WO-4 — 인터페이스만 있으면 진행 가능)

**산출물**
- `Scripts/Gameplay/`: `ICreatureController`+`CreatureController`(MonoBehaviour), `CreatureFactory`(static).
- 베이스 `AnimatorController`(상태/전이, 설계 8.2) — 자리표시자 클립으로.

**동작 명세**
- `CreatureFactory.Build(data, parent)`: `CreatureDatabase.FindArchetype`로 prefab 인스턴스화 → `AnimatorOverrideController` 적용 → 틴트(`MaterialPropertyBlock`)·스케일·VFX 컬러를 `data`+`seed`로 설정 → `ICreatureController` 반환.
- `Play(ActionType)`: 베이스 FSM 트리거 set + `feedback` sfx/vfx 발행 + `ActionStarted`/`ActionFinished` 이벤트(연출 동기화).
- 입력 처리(설계 8.5): 탭→`Play(Tap)`, 드래그→이동+`Play(Drag)`, 근접→`Play(Curious)`.

**테스트**: ActionType→트리거명 매핑 등 순수 로직 EditMode. (Animator 재생 자체는 제외)
**범위 밖**: 대전 진행(WO-6), 실제 애니메이션 클립 제작.

---

## WO-6 · 대전: IBattleDirector + ITurnResolver

**목표**: 로직(즉시 계산)과 연출(순차 재생)을 분리한 경량 자동 대전. (설계 9장)
**선행조건**: WO-5.

**산출물**
- `Scripts/Data/`: `CombatState`(struct), `TurnOutcome`(struct), `BattleResult`(struct).
- `Scripts/Gameplay/`: `IBattleDirector`+`BattleDirector`, 내부 `ITurnResolver`+구현(3~5타입 상성표).

**동작 명세**
- `StartBattle(a, b, arenaPose)`: 두 `CreatureData`로 `CombatState` 초기화, `CreatureFactory.Build`로 배치, `Play(BattleReady)`.
- 턴 루프: `ITurnResolver.Resolve`(즉시 결과) → `Play(Attack)` 후 `ActionFinished(Attack)` await → 방어측 `Play(Hit)` → HP 반영 → `defenderDown`이면 `Play(Faint)`/승자 `Play(Win)`.
- 종료 시 `BattleEnded(BattleResult)`.

**테스트 (필수)**: `ITurnResolver.Resolve` — 상성 우위 크리티컬, 데미지 계산, `defenderDown` 경계, 결정성. 더미 `CombatState`.
**범위 밖**: 탭 타이밍 액션(스트레치), 깊은 밸런싱, 멀티플레이.

---

## WO-7 · 수집: ICollectionService + 영속성

**목표**: 포획 등록과 JSON 영속성, 도감 UI 연동 신호. (설계 10장)
**선행조건**: WO-0.

**산출물**
- `Scripts/Gameplay/`: `ICollectionService`+`CollectionService`(JSON ↔ `Application.persistentDataPath`).

**동작 명세**
- `Add(data)`: 목록 추가 + 즉시 저장 + `CreatureAdded` 발행. (8.1 `Capture` 종료 시 호출)
- `Contains(referenceImageId)`: 중복 인식 보조.
- `Load()`: 앱 시작 시 JSON → 메모리. 파일 없음/손상 시 빈 컬렉션으로 graceful.

**테스트 (필수)**: Add→Save→Load 라운드트립, 손상 파일 복구, Contains 정확성. 임시 경로 사용.
**범위 밖**: 도감 UI 비주얼(별도 UI WO), 클라우드 동기화.

---

## 통합 단계 (WO 완료 후, 사람 주도)
- 캡처 플로우 와이어링: `ImageRecognized` → 포획 컨펌 → `ICameraImageService.TryAcquireLatest` → `GenerationService.Generate` → `CollectionService.Add`.
- 광원: `IEnvironmentService.LightUpdated` → 씬 라이트/크리처 머티리얼 적용.
- 실기기 검증·튜닝(트래킹/오클루전/광원), 크리처 에셋 제작, UI 비주얼 — **사람 담당**.

---

## 권장 진행 순서
WO-0 → WO-1 → WO-2 → WO-3 → WO-4 → WO-5 → WO-6 → WO-7 → 통합.
(WO-3/4/7은 WO-0만 있으면 병렬 가능. WO-2는 WO-1, WO-6은 WO-5 선행.)
