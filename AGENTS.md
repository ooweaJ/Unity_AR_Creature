# CLAUDE.md — PROJECT PRISM (Unity AR) 작업 규칙

> 이 파일은 에이전트가 매 세션 읽는 **프로젝트 상시 규칙**이다. 특정 기능 구현 명세는 별도 `작업 지시서`를 참조한다.

## 프로젝트 개요
- 모바일 AR 크리처 수집·상호작용·경량 대전 게임. 포트폴리오용.
- 핵심: 이미지 인식 → (하이브리드) 절차적 생성 → 공간 배치 → 상호작용 애니메이션 → 경량 대전.
- 설계 단일 진실: `PROJECT_PRISM_AR_설계문서_v1.md`. 코드는 이 문서의 인터페이스 시그니처를 따른다.

## 기술 스택 (버전 고정)
- Unity 6 (6000.x LTS), URP, Input System(신규).
- AR Foundation 6.x + ARCore XR Plugin(주) / ARKit XR Plugin(호환).
- 직렬화: JsonUtility (외부 의존 최소화).
- 타겟: Android(ARCore) 우선, iOS(ARKit) 호환. 세로/가로는 추후 결정 — 임의 가정 금지.

## 아키텍처 규칙 (강제)
1. **4레이어 분리**: Presentation / Gameplay / AR Abstraction / AR Foundation. 위→아래로만 의존.
2. **AR 격리 (최우선 규칙)**: `Gameplay`·`Presentation` 레이어는 AR Foundation 타입(`ARTrackedImage`, `ARPlane`, `XRCpuImage`, `ARAnchor` 등)을 **직접 참조 금지**. 반드시 AR Abstraction 인터페이스(`IImageTrackingService`, `ICameraImageService`, `IPlacementService`, `IEnvironmentService`)를 통해서만 접근.
3. **인터페이스 우선**: 서비스는 인터페이스로 정의하고 구현체를 분리한다. 의존은 인터페이스에 건다(생성자 주입 또는 경량 서비스 로케이터).
4. **이벤트 기반 디커플링**: 모듈 간 통신은 C# `event`로 느슨하게. 매니저끼리 직접 호출 체인 금지.
5. **데이터 주도**: 신규 크리처는 코드 수정 없이 ScriptableObject(`CreatureArchetype`/`CreatureAnimationSet`/`ReferenceImageBinding`) 추가 + `CreatureDatabase` 등록으로만 확장. 애니메이션은 `AnimatorOverrideController`로 클립 교체.

## 코딩 컨벤션
- 네임스페이스 루트: `Prism`. 레이어별 하위: `Prism.AR`, `Prism.Gameplay`, `Prism.Data`, `Prism.UI`.
- 파일 1개 = 타입 1개(작은 struct/enum 동거 허용). 파일명 = 타입명.
- 공개 API는 인터페이스 + XML doc 주석(한국어 가능). 필드는 직렬화 목적 외 `private`.
- 매직 넘버 금지 → 상수/SO 노출. `Update()`에서 매 프레임 할당 금지(특히 픽셀/이미지 경로).
- async는 Awaitable/UniTask 스타일 코루틴 중 프로젝트 1개 방식으로 통일(미정 시 코루틴).

## 폴더 구조
```
Assets/Prism/
  Scripts/AR/         # AR Abstraction 구현체 (AR Foundation 의존 허용)
  Scripts/Gameplay/   # 생성·크리처·대전·수집 (AR 타입 참조 금지)
  Scripts/Data/       # CreatureData, enum, ScriptableObject 정의
  Scripts/UI/
  ScriptableObjects/  # 에셋(.asset): Database/Archetype/AnimationSet/Binding
  Tests/EditMode/     # AR 비의존 로직 단위 테스트
```

## 테스트 규칙
- AR 비의존 로직(`ITraitMapper`, `ITurnResolver`, `CollectionService` JSON)은 **EditMode 단위 테스트 필수**. 더미 `PixelStats`/`CreatureData`로 검증.
- AR 의존 코드(트래킹·오클루전·광원·앵커)는 자동 테스트 대상 아님 → 인터페이스 뒤에 두고, 로직은 모킹으로 테스트.

## 하지 말 것 (Do NOT)
- AR Foundation 동작을 "기기에서 검증했다"고 가정하지 말 것. 실기기 AR 테스트·튜닝은 사람 몫.
- 크리처 메쉬/리깅/애니메이션 클립/VFX/레퍼런스 이미지 등 **에셋을 만들지 말 것**(자리표시자 prefab까지만). 콘텐츠는 사람이 채운다.
- 설계 문서에 없는 시스템·기능을 임의 추가하지 말 것(스코프 크리프). 필요하면 먼저 질문.
- `PlayerPrefs`에 대용량 데이터 저장 금지(수집 데이터는 JSON 파일).
- 대전 로직을 깊게 확장하지 말 것(타입은 3~5개 가위바위보, 자동 턴). 설계 9장 범위 고정.
- 한 작업 지시서의 범위를 넘어서 다른 시스템까지 건드리지 말 것.

## 작업 진행 방식
- 작업 지시서 1건 = 1 PR 단위. 시작 전 의존 인터페이스가 이미 있는지 확인, 없으면 stub로 컴파일 가능 상태 유지.
- 구현 후: 컴파일 통과 + (해당되면) EditMode 테스트 통과 + 무엇을 어떤 파일에 만들었는지 요약 보고.
- 단위 작업이 완료될 때마다 관련 변경만 묶어 커밋하고 원격 저장소에 푸시한다. 여러 WO를 한 커밋에 섞지 않는다.
- 커밋 메시지는 한국어 Conventional Commit 형식을 사용한다: `<타입>: <한국어 요약>`.
- 허용 타입: `feat`(기능), `fix`(수정), `docs`(문서), `test`(테스트), `refactor`(리팩터), `chore`(설정/잡무), `build`(빌드/패키지), `ci`(CI).
- 예시: `feat: WO-0 데이터 모델 추가`, `docs: AR 타입 격리 규칙 정정`, `test: 크리처 데이터베이스 조회 테스트 추가`.
- 불확실하면 가정하지 말고 질문. 특히 플랫폼·입력·저장 위치 관련.
