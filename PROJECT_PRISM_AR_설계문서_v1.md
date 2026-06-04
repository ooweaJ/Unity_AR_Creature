# PROJECT: PRISM — AR 크리처 수집·상호작용 프로젝트 아키텍처 설계 문서 (v1)

> **워킹 타이틀**: *Prism* — 카메라가 인식한 이미지의 빛(색·패턴)을 분해해 고유한 크리처로 "생성"한다는 컨셉. 마음에 안 들면 자유롭게 교체.
> **문서 목적**: 포트폴리오용 모바일 AR 프로젝트의 시스템 설계 청사진. 구현 사다리 각 단계를 모듈·클래스 단위로 정의한다.

---

## 1. 프로젝트 개요

### 1.1 한 줄 정의
> 현실의 이미지를 스캔하면, 그 이미지의 색·패턴을 분석해 **세상에 하나뿐인 크리처**가 절차적으로 생성되고, 그 크리처를 내 책상 위 현실 공간에 배치·교감하고, 수집한 크리처끼리 가볍게 대전시키는 모바일 AR 게임.

### 1.2 포트폴리오 목표 (이 프로젝트가 증명하는 것)
이 프로젝트의 평가 축은 "게임이 재밌나"가 아니라 **AR Foundation을 얼마나 깊고 정확하게 다루는가**다. 따라서 다음 4가지를 명시적으로 증명하는 것을 1순위 목표로 한다.

| 증명 항목 | 구체 기술 | 차별화 포인트 |
|---|---|---|
| **인식 (Recognition)** | `ARTrackedImageManager` 기반 이미지 트래킹 | 단순 스폰을 넘어 카메라 CPU 이미지 접근 |
| **생성 (Generation)** | 픽셀 분석 → 파라미터 매핑 → 절차적 크리처 | "스캔 대상마다 다른 결과" = 텍스처/데이터 가공 능력 |
| **공간배치 (Spatial)** | 평면 인식·앵커·오클루전·광원 추정·트래킹 복구 | 튜토리얼이 안 다루는 리얼리즘 레이어 |
| **상호작용 (Interaction)** | 터치 반응 애니메이션 상태 머신 + 경량 대전 | 정적 데모가 아닌 "살아있는" 연출 |

### 1.3 비목표 (Non-Goals) — 스코프 방어선
- 깊은 RPG 대전(타입 상성 풀셋, 스킬 트리, 밸런싱)은 만들지 **않는다**. 대전은 애니메이션 연출 중심의 **수직 슬라이스**.
- 온라인 멀티플레이는 이번 스코프 제외(추후 확장 후보).
- 방대한 크리처 종 수 확보가 아니라, **소수의 아키타입을 절차적으로 변주**하는 방향.

---

## 2. 기술 스택 & 환경

| 항목 | 선택 | 비고 |
|---|---|---|
| 엔진 | Unity 6 (6000.x LTS) | Beat Saber 클론과 동일 버전대 유지 |
| AR 프레임워크 | AR Foundation 6.x | ARCore / ARKit provider |
| 렌더 파이프라인 | URP | 모바일 성능 + 광원 추정 셰이딩 |
| 입력 | Input System (신규) | 터치/탭/드래그 |
| 타겟 플랫폼 | **Android(ARCore) 우선, iOS(ARKit) 호환** | Beat Saber=Quest, Prism=모바일 → 디바이스 포트폴리오 분산 |
| 데이터 직렬화 | JSON (`JsonUtility` 또는 Newtonsoft) | 수집 데이터 영속성 |
| 형상관리 | Git (기존 GitHub 흐름 유지) | |

> **플랫폼 전략 메모**: 오클루전(환경 깊이)·LiDAR 등 일부 기능은 기기별 지원이 갈린다. 메인 검증 기기 1대를 정하고, 미지원 기기에서는 해당 레이어를 graceful하게 끄는 폴백을 설계에 포함한다.

---

## 3. 핵심 게임 루프

```
[1] 스캔        →  카메라로 이미지(포스터/카드/사물 표면) 인식
       │
[2] 생성        →  인식 영역의 픽셀을 분석 → 고유 파라미터로 크리처 생성
       │
[3] 포획        →  생성된 크리처를 도감/컬렉션에 저장 (seed 기반 재현 가능)
       │
[4] 배치        →  평면 위에 크리처 소환 → 앵커 고정 → 현실 공간에 안착
       │
[5] 교감        →  탭/드래그에 크리처가 애니메이션으로 반응 (idle/기쁨/이동)
       │
[6] 대전        →  수집한 크리처 2마리 배치 → 애니메이션 연출 중심 경량 대전
       └────────────────────────────────────────────────────┐
                                                    다시 [1]로 (새 스캔)
```

---

## 4. 시스템 아키텍처

### 4.1 레이어 구조
모듈을 4개 레이어로 분리해 AR Foundation 의존성을 한 곳에 격리한다. (테스트성·교체성 확보 = 면접 어필 포인트)

```
┌─────────────────────────────────────────────────────┐
│  Presentation Layer  (UI / HUD / 도감 화면)           │
├─────────────────────────────────────────────────────┤
│  Gameplay Layer                                       │
│   ├─ GenerationService   (픽셀 → 크리처 파라미터)      │
│   ├─ CreatureController   (애니메이션·상호작용 FSM)    │
│   ├─ BattleDirector       (경량 대전 연출)             │
│   └─ CollectionService    (수집/영속성)                │
├─────────────────────────────────────────────────────┤
│  AR Abstraction Layer  ★핵심                          │
│   ├─ IImageTrackingService                            │
│   ├─ IPlacementService    (raycast·anchor)            │
│   ├─ IEnvironmentService  (occlusion·light·tracking)  │
│   └─ ICameraImageService  (CPU image 접근)            │
├─────────────────────────────────────────────────────┤
│  AR Foundation (ARCore / ARKit)                       │
└─────────────────────────────────────────────────────┘
```

> **설계 의도**: Gameplay 레이어는 AR Foundation 타입(`ARTrackedImage` 등)을 직접 모르게 한다. 인터페이스 뒤로 숨기면 (1) AR 없이도 에디터에서 모킹 테스트 가능, (2) 추후 Quest/다른 provider 교체 용이. 이 "어댑터 레이어" 자체가 시니어스러운 설계로 읽힌다.

### 4.2 통신 방식
- 모듈 간 결합은 **이벤트 버스 / C# event** 기반으로 느슨하게.
- 예: `ImageRecognized` → GenerationService 구독 → `CreatureGenerated` → CollectionService & 배치 흐름이 구독.

### 4.3 핵심 매니저 책임 분리
| 클래스 | 책임 | 의존 |
|---|---|---|
| `ARBootstrap` | AR 세션 초기화·기능 지원 체크·폴백 결정 | AR Foundation |
| `ImageTrackingService` | 트래킹 이벤트를 도메인 이벤트로 변환 | `ARTrackedImageManager` |
| `CameraImageService` | 최신 CPU 이미지 획득·크롭·다운샘플 | `ARCameraManager` |
| `GenerationService` | 픽셀 통계 → `CreatureData` 산출 | CameraImageService |
| `PlacementService` | 스크린 탭 → 평면 레이캐스트 → 앵커 부착 | `ARRaycastManager`, `ARAnchorManager` |
| `EnvironmentService` | 오클루전·광원 추정·트래킹 상태 브로드캐스트 | `AROcclusionManager`, `ARCameraManager` |
| `CreatureController` | 배치된 크리처 1개의 FSM·애니메이션 | — |
| `BattleDirector` | 대전 페어링·턴 진행·연출 큐 | CreatureController |
| `CollectionService` | 수집 목록·저장/로드 | JSON I/O |

---

## 5. 인식 시스템 (Image Tracking)

### 5.1 구성
- `XROrigin` + `ARSession` + `ARTrackedImageManager`.
- `XRReferenceImageLibrary` 에 시드 이미지 N장 등록(런타임 동적 추가도 가능 — 스트레치).
- 트래킹 이벤트(`trackablesChanged` / 6.x API)에서 `added` / `updated` / `removed` 분기.

### 5.2 도메인 이벤트 변환
```csharp
// ImageTrackingService
public event Action<TrackedImageInfo> ImageRecognized;

void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> e) {
    foreach (var img in e.added) {
        if (img.trackingState == TrackingState.Tracking)
            ImageRecognized?.Invoke(ToInfo(img)); // 좌표·이미지명·스크린영역 추출
    }
}
```
> `TrackedImageInfo`에는 (a) 어떤 레퍼런스 이미지였는지, (b) 화면상 bounding 영역(픽셀 샘플링용), (c) 월드 포즈를 담는다.

### 5.3 인터페이스 시그니처
```csharp
public interface IImageTrackingService {
    event Action<TrackedImageInfo> ImageRecognized;  // 새 이미지 추적 시작
    event Action<string> ImageTrackingLost;          // 추적 소실 (referenceImageName)
    void SetEnabled(bool on);                         // 스캔 모드 on/off
}

public readonly struct TrackedImageInfo {
    public readonly string referenceImageName; // 베이스 바인딩 키 (6.7)
    public readonly Pose worldPose;            // 월드 위치/회전
    public readonly RectInt screenBounds;      // 화면상 영역 → 픽셀 샘플링용 (6.6)
    public readonly TrackingState trackingState;
}
```
> 구현체 `ImageTrackingService`는 `ARTrackedImageManager`를 래핑해 AR Foundation 타입을 도메인 이벤트로만 노출. Gameplay 레이어는 `ARTrackedImage`를 직접 모른다.

### 5.4 중복 인식 방지
- 같은 이미지를 짧은 시간 내 재인식 시 쿨다운·이미 생성됨 플래그로 중복 생성 차단.

---

## 6. 절차적 생성 시스템 (Hybrid Generation) ★차별화 핵심

### 6.1 하이브리드 생성 모델 (확정안)
> **설계 결정**: 인식 안정성과 기술 어필을 둘 다 잡기 위해, 생성 입력을 **두 갈래로 분리**한다.
> - **베이스 = 등록 이미지(사전 바인딩)** → 어떤 아키타입·기본 속성이 나올지를 *통제 가능하게* 결정. (`ARTrackedImageManager`는 등록 이미지만 추적하므로 이 부분은 사전 매핑이 자연스럽고 안정적)
> - **변주 = 포획 순간의 카메라 프레임 픽셀** → 색 틴트·스탯 굴림·희귀도 등을 *그 자리에서* 흔들어 "유일성"과 "CPU 이미지 가공 능력"을 확보.

즉 **같은 이미지를 스캔해도 베이스 종(種)은 같지만, 포획 순간의 조명·각도·실제 인쇄 색에 따라 개체가 달라진다.**

### 6.2 두 입력의 역할 분담
| 입력 | 결정하는 것 | 성격 |
|---|---|---|
| 등록 이미지 바인딩 (`ReferenceImageBinding`) | 베이스 아키타입(메시·애니메이션 세트), 기본 원소 타입 | 사전 정의·통제·안정 |
| 라이브 픽셀 통계 (`PixelStats`) | 틴트(주/보조색), 스탯 ± 변주, 희귀도, 원소 ± 미세 보정 | 즉석 계산·유일성·기술 시그널 |

### 6.3 생성 파이프라인
```
[포획 컨펌 시점]
ARCameraManager.TryAcquireLatestCpuImage()           // 그 순간의 프레임 확보
   → 트래킹 이미지의 화면 bounds 로 크롭 + 32×32 다운샘플
   → PixelStats 산출 (평균 HSV, hue 분산, 주요 팔레트 2색)
   → ReferenceImageBinding 조회 (이미지 이름 → 베이스 아키타입/속성)
   → seed = Hash(이미지이름, 양자화된 PixelStats)
   → TraitMapper: 베이스 + 변주 → CreatureData 1건 산출
   → CreatureData 영속화 (라이브 픽셀은 재현 불가 → 결과를 저장)
```
> **재현 정책**: 변주는 라이브 픽셀에서 *1회* 계산해 `CreatureData`에 박아 저장한다. `seed`는 외형의 랜덤 서브선택(미세 비율, idle 변형 등)을 결정적으로 재현하는 용도. (도감 로드 시 재계산 없이 데이터 그대로 복원)

### 6.4 변주 매핑 규칙 (라이브 픽셀 → 파라미터)
| 픽셀 통계 | → 변주 대상 | 매핑 의도 |
|---|---|---|
| 평균 Hue | 원소 ± 미세 보정 / 주 틴트 | 베이스 속성 위에 색감 가미 |
| 평균 Saturation | 공격력 계열 ± 굴림 | 채도 높을수록 공격적 |
| 평균 Brightness | 방어/체력 계열 ± 굴림 | |
| Hue 분산(다채로움) | 희귀도(Rarity) | 복잡한 이미지일수록 희귀 |
| 주요 팔레트 2색 | 주/보조 틴트·VFX 컬러 | 개체 외형 개성 |

### 6.5 외형 생성 전략 (가성비 중심)
- **베이스 메시 아키타입 소수**(예: 4~6종)를 두고, 변주 파라미터로 **머티리얼 틴트 + 스케일 + 원소 VFX + 파츠 토글**을 조합.
- 풀 프로시저럴 메시 생성은 스코프 폭발 위험 → 1차에선 **변주(variation)** 방식. (포트폴리오상 충분히 "절차적"으로 읽힘)
- `MaterialPropertyBlock`으로 틴트 적용(드로우콜·인스턴싱 보호).

### 6.6 GenerationService 인터페이스 (구현 핸드오프용 시그니처)
```csharp
// 공개 진입점: 포획 순간 호출 → CreatureData 1건 반환
public interface IGenerationService {
    CreatureData Generate(in GenerationRequest request);
}

public readonly struct GenerationRequest {
    public readonly string  referenceImageName; // XRReferenceImage 이름 (베이스 바인딩 키)
    public readonly CameraImageFrame cameraFrame;// AR-neutral RGBA 프레임 (변주 소스)
    public readonly RectInt sampleRegion;        // 샘플링할 화면 영역(트래킹 bounds)
}

// AR Foundation 타입을 Gameplay로 넘기지 않기 위한 중간 DTO.
// ICameraImageService 구현체가 XRCpuImage를 RGBA 바이트 버퍼로 변환하고 Dispose까지 책임진다.
public readonly struct CameraImageFrame {
    public readonly int width, height;
    public readonly byte[] rgba32;
}

// 내부 파이프라인 (테스트·교체 용이하게 분리)
internal interface IPixelSampler {
    PixelStats Sample(in CameraImageFrame frame, RectInt region, int downsample = 32);
}
internal interface ITraitMapper {
    CreatureData Map(ReferenceImageBinding baseBinding, in PixelStats stats, int seed);
}

public readonly struct PixelStats {
    public readonly float avgHue, avgSat, avgVal, hueVariance;
    public readonly Color paletteA, paletteB;
}
```
> `GenerationService`는 (1) 이름으로 `ReferenceImageBinding` 조회 → (2) `IPixelSampler.Sample` 로 통계 → (3) `seed` 계산 → (4) `ITraitMapper.Map` 호출, 4단계만 오케스트레이션한다. AR Foundation 픽셀 접근(`XRCpuImage`)은 `ICameraImageService` 구현체 내부에 격리되고, Gameplay에는 `CameraImageFrame` 또는 `PixelStats` 같은 AR-neutral 데이터만 전달한다. 매핑 로직은 `ITraitMapper`에 격리 → 에디터에서 더미 `PixelStats`로 매핑 단위 테스트 가능.

### 6.7 데이터 모델
```csharp
[CreateAssetMenu] // ★카탈로그: 모든 바인딩/아키타입을 모아 코드가 조회하는 단일 진입점
public class CreatureDatabase : ScriptableObject {
    public ReferenceImageBinding[] bindings;
    public CreatureArchetype[] archetypes;

    // 이름으로 베이스 바인딩 조회 (GenerationService가 사용)
    public ReferenceImageBinding FindBinding(string referenceImageName);
    public CreatureArchetype FindArchetype(string archetypeId); // 로드 시 복원용
}

[CreateAssetMenu] // 등록 이미지 ↔ 베이스 사전 바인딩 (하이브리드의 "베이스" 축)
public class ReferenceImageBinding : ScriptableObject {
    public string referenceImageName;   // XRReferenceImage 이름과 일치
    public CreatureArchetype archetype;  // 이 이미지의 베이스 아키타입
    public ElementType baseElement;      // 베이스 원소 (픽셀로 ± 보정)
}

[CreateAssetMenu] // 아키타입 정의
public class CreatureArchetype : ScriptableObject {
    public string archetypeId;
    public GameObject basePrefab;             // 베이스 메시+Animator(베이스 컨트롤러)
    public CreatureAnimationSet animationSet; // 행동→클립 매핑 (8.3 참조)
}

[Serializable] // 런타임 생성·영속 인스턴스 (저장의 단일 진실)
public class CreatureData {
    public int seed;
    public string referenceImageId;        // 어느 등록 이미지에서 나왔나
    public string archetypeId;
    public ElementType element;
    public Color primaryTint, secondaryTint;
    public CreatureStats stats;            // atk/def/hp
    public Rarity rarity;
    public string capturedAt;              // 메타(스캔 시각 등)
}
```
> **콘텐츠 작성 워크플로(코드 무수정)**: 메쉬+클립 제작 → `AnimatorOverrideController` → `CreatureAnimationSet` → `CreatureArchetype` → `ReferenceImageBinding` 생성 후 → **`CreatureDatabase`에 등록(배열에 추가)**. 이 마지막 등록이 있어야 코드가 SO를 조회한다. 인스턴스화(`CreatureData` → GameObject)는 생성/로드 공용 `CreatureFactory.Build(data)`가 담당하며, `CreatureDatabase.FindArchetype`으로 prefab을 찾는다.

---

## 7. 공간 배치 시스템 (Spatial Placement) ★실력 변별 구간

### 7.1 평면 인식 + 배치
- `ARPlaneManager`로 수평면(바닥·책상) 검출.
- 스크린 탭 → `ARRaycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon)`.
- 히트 포즈에 크리처 인스턴스화.

### 7.2 앵커 고정
- 배치 즉시 `ARAnchorManager`로 앵커 생성하고 크리처를 앵커의 자식으로 부착 → 드리프트 최소화.
- 앵커별 `trackingState` 구독.

### 7.3 오클루전 (Occlusion) — 리얼리즘 1
- `AROcclusionManager` + URP 오클루전 셰이더로 환경 깊이 적용 → 크리처가 실제 물체 뒤로 가려짐.
- **폴백**: 깊이 미지원 기기에서는 오클루전 비활성 + 안내 토스트.

### 7.4 광원 추정 (Light Estimation) — 리얼리즘 2
- `ARCameraManager.frameReceived` → `ARLightEstimationData`(밝기·색온도·구면 조화/주광 방향) 획득.
- 씬의 디렉셔널 라이트/앰비언트에 반영 → 크리처가 방 조명과 어우러짐.

### 7.5 트래킹 복구 (Tracking Recovery) — 견고함
- `ARSession.stateChanged` + 앵커 `trackingState` 감시.
- `Limited`/`None` 상태: 크리처를 페이드아웃·HUD 경고, 재추적 시 복귀.
- 이 "깨지지 않게 만드는" 처리가 실제 AR 경험 유무를 가른다.

### 7.6 (스트레치) 앵커 영속성
- iOS `ARWorldMap` / Cloud Anchors로 세션 간 위치 보존 — 1차 스코프 밖, 여력 시.

### 7.7 인터페이스 시그니처

**(a) 카메라 CPU 이미지 — 생성 입력 공급 (4.3 `CameraImageService`)**
```csharp
public interface ICameraImageService {
    // 최신 CPU 이미지를 RGBA 프레임으로 변환해 제공한다. XRCpuImage 수명 관리는 구현체 책임.
    bool TryAcquireLatest(out CameraImageFrame frame);
}
```
> 포획 컨펌 시 이 서비스로 AR-neutral 프레임을 받아 `GenerationRequest`(6.6)를 조립한다. AR Foundation의 `ARCameraManager.TryAcquireLatestCpuImage`와 `XRCpuImage.Dispose`는 구현체 내부에 격리한다.

**(b) 배치 — 레이캐스트 + 앵커 (7.1~7.2)**
```csharp
public interface IPlacementService {
    bool TryRaycastPlane(Vector2 screenPos, out Pose pose);   // 평면 히트 포즈
    IAnchorHandle CreateAnchor(Pose pose);                     // ARAnchor 래핑
    event Action<bool> PlaneAvailabilityChanged;               // 배치 가능 평면 존재 여부
}

public interface IAnchorHandle {
    Transform Transform { get; }                  // 크리처를 이 자식으로 부착
    TrackingState TrackingState { get; }
    event Action<TrackingState> TrackingStateChanged;
    void Dispose();                               // 앵커 해제
}
```
> 배치 흐름: `TryRaycastPlane` → `CreateAnchor` → `CreatureFactory.Build(data, handle.Transform)`. 배치 서비스는 크리처를 직접 만들지 않고 앵커만 책임(관심사 분리).

**(c) 환경 — 오클루전·광원·세션 추적 (7.3~7.5)**
```csharp
public interface IEnvironmentService {
    bool OcclusionSupported { get; }                 // 기기 깊이 지원 여부 → 폴백 판단
    void SetOcclusionEnabled(bool on);
    event Action<LightReading> LightUpdated;          // 광원 추정 (매 프레임/저빈도)
    event Action<SessionTracking> SessionTrackingChanged;
}

public readonly struct LightReading {
    public readonly float brightness;       // 0..1
    public readonly Color colorCorrection;  // 색온도 보정
    public readonly Vector3? mainLightDir;  // 주광 방향(있으면)
}

public enum SessionTracking { None, Limited, Tracking }
```
> `LightUpdated` 구독 → 씬 라이트/앰비언트 갱신. `SessionTrackingChanged`가 `Limited/None`이면 배치된 크리처 페이드아웃 + HUD 경고(7.5). `OcclusionSupported=false`면 오클루전 토글을 끄고 안내.

---

## 8. 크리처 상호작용 & 애니메이션 시스템

> **설계 원칙**: 행동(action)은 enum으로 데이터화하고, FSM(상태 전이 로직)은 모든 크리처가 **공유**한다. "그 행동에 어떤 애니메이션이 재생되는가"만 크리처마다 다르며, 이는 `AnimatorOverrideController`로 클립을 갈아끼워 해결한다. 즉 로직(언제 무엇을)과 에셋(어떤 모션으로)을 분리한다.

### 8.1 인터랙션 카탈로그 (행동 정의)
크리처가 가질 수 있는 모든 행동을 `ActionType` enum으로 정의한다. 이게 "데이터로 관리되는 행동 목록"의 단일 출처(single source of truth)다.

| ActionType | 트리거 (언제) | 분류 | 비고 |
|---|---|---|---|
| `Spawn` | 이미지 최초 인식 / 도감에서 소환 | 등장 | 스폰 VFX 동반, 1회 재생 후 Idle |
| `Idle` | 기본 대기 | 상시 | 루프, 미세 호흡/부유 |
| `Capture` | 등장한 크리처 포획 컨펌 | 수집 | 포획 연출 → 도감 등록 이벤트 발행 |
| `Tap` (React) | 크리처 탭/쓰다듬기 | 교감 | 기쁨 반응, 사운드 1컷 |
| `Drag` (Move) | 드래그로 평면 위 이동 | 교감 | 이동 중 Walk/Follow 루프 |
| `Curious` | 카메라가 임계 거리 이내 접근 | 교감 | 카메라 응시 등 |
| `BattleReady` | `BattleDirector`가 대전 배치 | 대전 | 마주보기/전투 자세 |
| `Attack` | 자기 턴 공격 | 대전 | 데미지 적용 타이밍은 로직이 결정 |
| `Hit` | 피격 | 대전 | 넉백/피격 리액션 |
| `Win` | 대전 승리 | 대전 | 환호 |
| `Faint` | HP 0 | 대전 | 기절, 대전 종료 트리거 |

> 포획(`Capture`)은 "애니메이션"이자 동시에 **상태 전이의 분기점**이다. 포획 연출이 끝나면 `CollectionService`로 도감 등록 이벤트가 발행되고, 크리처는 "수집됨" 데이터로 영속화된다. (10장 연계)

### 8.2 상태 머신 (Animator FSM) — 공유 로직
```
Spawn ─▶ Idle ─┬─▶ Tap(React)   ← 탭/쓰다듬기
               ├─▶ Drag(Move)    ← 드래그 / 평면 이동
               ├─▶ Curious       ← 카메라가 가까워질 때
               ├─▶ Capture       ← 포획 컨펌 → 도감 등록
               └─▶ (Battle 전이) ← BattleDirector 신호
Battle: BattleReady ─▶ Attack ─▶ Hit ─▶ (Win | Faint)
```
- 이 FSM(상태·전이)은 **베이스 `AnimatorController` 1개**에 정의 → 모든 크리처가 공유.
- 파라미터: `Trigger: tap, drag, curious, capture, battleStart, attack, hit / Bool: isMoving / Trigger: faint, win`.
- `CreatureController`가 `Play(ActionType)` 단일 진입점을 제공하고, 상태 진입/이탈을 이벤트로 노출(연출·로직 동기화용).

### 8.3 데이터 주도 애니메이션 매핑 ★핵심
**같은 `ActionType`이라도 크리처마다 다른 클립이 나온다.** 불 속성의 `Attack`과 물 속성의 `Attack`은 모션이 다르다. 로직은 그대로 두고 클립만 교체하는 게 목표.

```csharp
public enum ActionType { Spawn, Idle, Capture, Tap, Drag, Curious,
                         BattleReady, Attack, Hit, Win, Faint }

// 크리처별 "행동 → 클립" 매핑 데이터 (ScriptableObject)
[CreateAssetMenu]
public class CreatureAnimationSet : ScriptableObject {
    public AnimatorOverrideController overrideController; // 베이스 위에 클립 교체
    [Serializable] public struct Entry { public ActionType action; public AudioClip sfx; }
    public Entry[] feedback;   // 행동별 사운드/VFX 키 등 부가 데이터
}
```

매핑 메커니즘 (2가지 중 택1, **A 권장**):

- **A. AnimatorOverrideController** — 베이스 컨트롤러의 상태(Idle/Attack/…)에 각 크리처의 실제 클립을 오버라이드로 끼운다. FSM 전이 로직은 100% 재사용, 클립만 데이터로 교체. Unity 표준 패턴이라 가장 견고.
- **B. Playables API + 클립 딕셔너리** — `Dictionary<ActionType, AnimationClip>`을 들고 `PlayableGraph`로 직접 재생. 더 유연하지만 직접 관리 부담↑. (스코프상 비권장)

런타임 흐름:
```
CreatureController.Play(ActionType.Attack)
   → animator.SetTrigger("attack")             // 공유 FSM이 Attack 상태로 전이
   → animator.runtimeAnimatorController 가 이미 이 크리처의 OverrideController
   → 결과적으로 "이 크리처의 Attack 클립"이 재생됨
   → feedback[Attack] 의 sfx/VFX 동시 발행
```

### 8.4 아키타입 ↔ 애니메이션 연결
6.4의 `CreatureArchetype`이 `CreatureAnimationSet`을 참조한다. 즉 **아키타입 = 메시 + 애니메이션 세트 묶음**이고, 절차적 생성은 그 위에 색/스탯/VFX 컬러만 변주한다. (모션은 아키타입 단위 공유, 외형·수치는 인스턴스 단위 변주)

### 8.5 상호작용 입력 처리
- 탭: 크리처 콜라이더에 스크린 레이캐스트 → 히트 시 `Play(Tap)`.
- 드래그: 평면 레이캐스트로 목표 위치 산출 → `isMoving` on → `Play(Drag)`.
- 근접 반응: 카메라–크리처 거리 임계값 → `Play(Curious)`.

### 8.6 연출 디테일
- 등장 시 스폰 VFX(원소 컬러), Idle 중 미세 부유/숨쉬기, 반응 시 사운드 1컷.
- 광원 추정 값으로 크리처 그림자/하이라이트 조정.

### 8.7 인터페이스 시그니처
```csharp
// 배치된 크리처 1개의 상호작용·애니메이션 제어
public interface ICreatureController {
    CreatureData Data { get; }
    void Play(ActionType action);                 // 단일 진입점
    event Action<ActionType> ActionStarted;
    event Action<ActionType> ActionFinished;      // 연출 동기화용 (대전 큐가 await)
}

// CreatureData → 실제 GameObject (생성·로드 공용 창구)
public static class CreatureFactory {
    // 아키타입 prefab 인스턴스화 + OverrideController 적용 + 틴트/스케일/VFX(데이터·seed 기반)
    public static ICreatureController Build(CreatureData data, Transform parent);
}
```
> `ActionFinished` 이벤트가 9장 연출 큐의 동기화 신호다. 대전은 `Play(Attack)` 후 `ActionFinished(Attack)`를 기다렸다가 다음 단계로 넘어간다.

---

## 9. 대전 시스템 (애니메이션 기반 경량 대전)

### 9.1 범위 정의
> 핵심은 **"수집한 크리처가 살아 움직이며 맞붙는 연출"**. 깊은 전투 로직이 아니라 **연출 디렉팅**이 보여줄 가치다.

### 9.2 구조
- `BattleDirector`가 크리처 2마리(내 컬렉션에서 선택 or 야생 스폰)를 평면 위 마주보게 배치.
- 진행 모델(택1, 1차 권장 = **자동 턴**):
  - **자동 턴제**: 스탯 + 원소 상성으로 데미지 계산 → 턴마다 `Attack`/`Hit` 애니메이션 큐 재생 → HP 0이면 `Faint`.
  - (대안) 탭 타이밍 액션: 공격 순간 탭으로 보너스 — 스코프 여유 시.
- 상성은 **간단한 3~5 타입 가위바위보 테이블**로 한정(밸런싱 늪 회피).

### 9.3 연출 파이프라인
```
BattleDirector
  → ResolveTurn() : 데미지·결과 계산 (로직)
  → EnqueueChoreography() : Attack→Hit→React 애니/카메라/VFX 시퀀스 (연출)
  → Coroutine/async 로 순차 재생, 끝나면 다음 턴
```
- 로직과 연출을 분리(결과는 즉시 결정, 화면은 순차 재생) → 동기화 버그 예방.

### 9.4 인터페이스 시그니처
```csharp
public interface IBattleDirector {
    void StartBattle(CreatureData a, CreatureData b, Pose arenaPose);
    event Action<BattleResult> BattleEnded;
}

public readonly struct BattleResult {
    public readonly CreatureData winner, loser;
    public readonly int turns;
}

// 턴 결과를 "즉시" 계산 (로직). 연출과 분리.
internal interface ITurnResolver {
    TurnOutcome Resolve(in CombatState attacker, in CombatState defender);
}

public struct CombatState {        // 대전 중 런타임 상태 (CreatureData에서 초기화)
    public CreatureData source;
    public int currentHp;
    public ElementType element;
}

public readonly struct TurnOutcome {
    public readonly int damage;
    public readonly bool isCritical;   // 원소 상성 우위
    public readonly bool defenderDown; // HP 0 도달
}
```
> 흐름: `ITurnResolver.Resolve`로 결과를 먼저 확정(즉시) → `BattleDirector`가 `ICreatureController.Play(Attack)`/`Play(Hit)`를 순차 호출하고 각 `ActionFinished`를 await(연출) → 모든 턴 소진/`defenderDown` 시 `Play(Win)`/`Play(Faint)` 후 `BattleEnded` 발행. 상성표는 `ITurnResolver` 구현 내부의 3~5타입 가위바위보 테이블로 한정.

---

## 10. 수집 / 도감 & 영속성

### 10.1 저장
- `CollectionService`가 `List<CreatureData>` 보관 → JSON으로 `Application.persistentDataPath`에 저장.
- **데이터 영속 원칙**: 변주는 라이브 픽셀에서 1회 계산되며 재현 불가하므로, 결과 파라미터(틴트·스탯·희귀도·element)를 `CreatureData`에 그대로 저장한다. `seed`는 외형의 랜덤 서브선택(미세 비율·idle 변형)만 결정적으로 재현. 로드 시 `CreatureFactory.Build(data)`로 재계산 없이 복원 → 용량도 작음(필드 몇 개 + seed).

### 10.2 도감 UI
- 그리드(수집 카드) → 탭 시 상세(원소·스탯·희귀도·획득 정보).
- 상세에서 "현실에 소환" 버튼 → 배치 플로우 진입.

### 10.3 인터페이스 시그니처
```csharp
public interface ICollectionService {
    IReadOnlyList<CreatureData> All { get; }
    void Add(CreatureData data);                 // 포획 등록 + 즉시 저장
    bool Contains(string referenceImageId);       // 이미 잡은 이미지인지(중복 인식 보조)
    void Load();                                  // 앱 시작 시 JSON → 메모리
    event Action<CreatureData> CreatureAdded;     // 도감 UI 갱신 신호
}
```
> 8.1 `Capture` 연출 종료 → `Add(data)` 호출이 "포획 완료"의 단일 지점. `CreatureAdded`를 도감 UI가 구독.

---

## 11. UI / UX 흐름

```
[홈/카메라뷰]
   ├─ (스캔 모드)  타겟 인식 → "생성 연출" 풀스크린 → 포획 컨펌
   ├─ (배치 모드)  도감에서 선택 → 평면 탭 소환 → 교감
   ├─ (대전 모드)  2마리 선택 → 배치 → 대전 연출
   └─ [도감] 탭 → 그리드 → 상세
HUD: AR 상태 인디케이터(트래킹 양호/제한), 모드 토글
```

---

## 12. 구현 로드맵 (단계별 사다리)

> 원칙: **차별화 핵심(생성)을 빠르게 박고**, 리얼리즘 레이어는 뒤에 쌓는다. 각 단계는 "혼자 데모 가능한 완결 상태"로 끊는다.

### Phase 0 — 환경 셋업 (0.5주)
- Unity 6 + AR Foundation 6.x 프로젝트, URP, Input System 구성.
- 기기 빌드 파이프라인 검증(ARCore 기기 1대 기준).

### Phase 1 — 인식 파이프라인 (1주)
- `ARTrackedImageManager` + 레퍼런스 라이브러리.
- 이미지 인식 시 **고정 더미 크리처** 1종 스폰.
- ✅ 마일스톤: "스캔하면 뭔가 나온다".

### Phase 2 — 절차적 생성 (1.5주) ★
- `CameraImageService`로 CPU 이미지 획득·크롭·다운샘플.
- 픽셀 통계 → `CreatureData` 매핑, seed 결정성 확보.
- 아키타입 4종 + 틴트/VFX 변주.
- ✅ 마일스톤: "스캔 대상마다 다른 크리처".

### Phase 3 — 공간 배치 기본 (1주)
- 평면 인식 + 탭 레이캐스트 + 앵커 부착.
- ✅ 마일스톤: "책상 위에 안정적으로 선다".

### Phase 4 — 리얼리즘 레이어 (1.5주)
- 오클루전 + 광원 추정 + 트래킹 복구·폴백.
- ✅ 마일스톤: "현실에 진짜 있는 것처럼 보인다" (포트폴리오 하이라이트 영상 구간).

### Phase 5 — 상호작용 애니메이션 (1주)
- FSM(Idle/React/Move) + 탭·드래그·근접 반응.
- ✅ 마일스톤: "만지면 반응한다".

### Phase 6 — 경량 대전 (1.5주)
- `BattleDirector` 자동 턴 + 연출 큐 + 간단 상성.
- ✅ 마일스톤: "수집한 둘이 맞붙는다".

### Phase 7 — 수집/도감 + 영속성 + 폴리시 (1주)
- JSON 저장/로드, 도감 UI, 사운드/VFX/HUD 마감, 데모 영상.

> 합산 ≈ **9주(여유 포함)**. Beat Saber·LETHE와 병행이면 Phase 2·4를 우선 완성해 "AR 기술 깊이" 데모만 먼저 확보하는 분할 출시도 가능.

---

## 13. 리스크 & 기술적 도전 과제

| 리스크 | 영향 | 대응 |
|---|---|---|
| 오클루전/깊이 기기 파편화 | 일부 기기서 리얼리즘 레이어 미동작 | 지원 체크 후 graceful 폴백, 검증 기기 명시 |
| CPU 이미지 접근 성능 | 프레임 드랍 | 저빈도·다운샘플·비동기 처리 |
| 생성 결정성 깨짐 | 같은 이미지 다른 결과 | seed 고정·통계 정규화 |
| 앵커 드리프트 | 크리처가 떠다님 | 앵커 부착·트래킹 상태 가드 |
| 대전 스코프 크리프 | 일정 폭발 | 자동 턴 + 가위바위보 상성으로 동결 |
| 라이팅/오클루전 셰이더 URP 연동 | 시각 품질 | AR Foundation 샘플 셰이더 기반 출발 |

---

## 14. 포트폴리오 어필 포인트 (면접 서술용)

이 프로젝트를 말로 풀 때 강조할 한 줄들:

1. **"AR Foundation의 표면(이미지 스폰)이 아니라 깊이를 다뤘다"** — CPU 이미지 접근, 오클루전, 광원 추정, 트래킹 복구까지.
2. **"인식 결과를 데이터로 가공해 절차적 생성으로 연결"** — 픽셀 통계 → 게임 파라미터 매핑(텍스처/데이터 처리 역량).
3. **"AR 의존성을 추상화 레이어로 격리"** — 인터페이스 어댑터 설계로 테스트성·이식성 확보(아키텍처 역량).
4. **"로직과 연출을 분리한 대전 디렉팅"** — 결과 즉시 결정 + 시퀀스 순차 재생으로 동기화 버그 회피.
5. **디바이스 다변화** — Quest(Beat Saber) ↔ 모바일(Prism)로 XR 포트폴리오 폭 증명.

---

## 부록 A. 권장 패키지/레퍼런스
- AR Foundation, ARCore XR Plugin, ARKit XR Plugin (provider)
- AR Foundation Samples (오클루전·라이트·이미지 트래킹 레퍼런스 코드 출발점)
- URP, Input System
- (선택) Newtonsoft Json for Unity

## 부록 B. 네이밍/테마 후보
- **Prism** (빛 분해 → 크리처 생성, 추상·미니멀 비주얼과 결)
- Spectra / Lumen / Chroma — 색 기반 컨셉 유지 시
- 비주얼 톤: 추상·미니멀 네온(LETHE 방향성과도 통일 가능) ↔ 또는 명확한 카툰. **1개로 동결 권장.**

---

## 부록 C. 인터페이스 인덱스 (구현 핸드오프용)
구현 단위로 쪼개 Claude Code에 넘길 때 이 표를 작업 분배 기준으로 사용.

| 인터페이스 | 레이어 | 책임 | 정의 위치 | 주요 의존 |
|---|---|---|---|---|
| `IImageTrackingService` | AR 추상화 | 이미지 인식 → 도메인 이벤트 | 5.3 | `ARTrackedImageManager` |
| `ICameraImageService` | AR 추상화 | CPU 프레임 획득(생성 입력) | 7.7(a) | `ARCameraManager` |
| `IPlacementService` / `IAnchorHandle` | AR 추상화 | 평면 레이캐스트 + 앵커 | 7.7(b) | `ARRaycastManager`, `ARAnchorManager` |
| `IEnvironmentService` | AR 추상화 | 오클루전·광원·세션 추적 | 7.7(c) | `AROcclusionManager`, `ARCameraManager`, `ARSession` |
| `IGenerationService` | Gameplay | 베이스+변주 → `CreatureData` | 6.6 | `ICameraImageService` |
| `IPixelSampler` / `ITraitMapper` | Gameplay(내부) | 픽셀 통계 / 형질 매핑 | 6.6 | — |
| `ICreatureController` / `CreatureFactory` | Gameplay | 크리처 상호작용·인스턴스화 | 8.7 | `CreatureAnimationSet` |
| `IBattleDirector` / `ITurnResolver` | Gameplay | 대전 진행·연출 / 턴 로직 | 9.4 | `ICreatureController` |
| `ICollectionService` | Gameplay | 수집·영속성 | 10.3 | JSON I/O |

**ScriptableObject 데이터**: `CreatureDatabase`(카탈로그·조회 진입점, 6.7), `ReferenceImageBinding`(6.7), `CreatureArchetype`(6.7), `CreatureAnimationSet`(8.3).
**핵심 데이터 타입**: `CreatureData`(6.7), `TrackedImageInfo`(5.3), `PixelStats`(6.6), `LightReading`(7.7c), `CombatState`/`TurnOutcome`(9.4).

**권장 구현 순서(인터페이스 기준)**: `IImageTrackingService` → `ICameraImageService` + `IGenerationService`(+내부) → `IPlacementService` → `IEnvironmentService` → `ICreatureController`/`CreatureFactory` → `IBattleDirector` → `ICollectionService`. (12장 Phase 로드맵과 일치)

---

*문서 버전 v1.1 — 전 시스템 인터페이스 시그니처화 완료(5·6·7·8·9·10장). 생성은 하이브리드 모델로 확정. 다음 분기: 시스템별 하위 문서(셰이더·데이터 스키마·CLAUDE.md 작업 지시서).*
