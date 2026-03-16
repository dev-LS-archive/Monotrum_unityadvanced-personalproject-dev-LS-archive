# Monotrum 프로젝트 회고

## 프로젝트 개요

- **프로젝트명**: Monotrum (모노트럼)
- **장르**: 오디오 리액티브 3D 러너
- **팀원 수**: 1인 (솔로)
- **개발 기간**: 9일 (2026.02.26 ~ 2026.03.06)
- **개발 환경**: Unity 6 LTS (6000.3.9f1) / C# / URP
- **프로젝트 목표**
  - FFT 스펙트럼 분석 기반 실시간 오디오 비주얼라이저 구현
  - 난이도 장벽 없이 음악과 시각 경험에 집중하는 체험형 러너
  - 절차적 터널 생성 + 포스트 프로세싱으로 최소 리소스, 최대 비주얼 달성
  - 이전 팀 프로젝트(OverTheSky) 회고에서 도출한 개선점 적용

## 구현한 게임 설명

- **핵심 규칙**
  - W키로 전진(터널 스크롤 + 음악 정상 재생), 해제 시 테이프 스톱 연출
  - Space로 점프 (수학 기반, 비주얼 부가 요소)
  - 곡이 끝나면 클리어
- **플레이 흐름**
  1. 타이틀 씬: 오프닝 연출 → 곡 선택(셀렉터 UI) → 미리듣기 + 비주얼라이저 바 → Play
  2. 게임 씬: 네온 터널 안에서 음악에 반응하는 비주얼을 즐기며 달리기
  3. 클리어: 곡 종료 감지 → 클리어 UI → 타이틀 복귀

## 설계 구조

### 주요 클래스

| 분류 | 클래스명 | 역할 |
|------|----------|------|
| **Core Framework** | `Singleton<T>` | 제네릭 싱글톤. DontDestroyOnLoad, 중복 방지, 종료 시 재생성 방지 |
| | `GameManager` | 매니저 등록, 커서 제어, VSync 해제 및 프레임 고정 |
| | `InputManager` | InputSystem 기반 이벤트 발송, 입력 차단 플래그 |
| | `AudioManager` | 곡 재생/정지, AudioMixer 스냅샷 전환, 피치 제어, 믹서 잠금 |
| | `Logger` | 인게임 오버레이 디버그 로그, Queue 기반 FIFO |
| | `Define` | 전역 상수(씬 이름, 애니메이터 파라미터) |
| **Audio** | `AudioAnalyzer` | FFT → 8밴드 압축 → 가속 감쇠 스무딩 |
| | `TrackData (SO)` | 곡별 정보(이름, 클립, BPM, themeColor, scrollSpeed) |
| | `TrackController` | UI 이벤트와 AudioManager를 연결하는 래퍼 |
| | `TrackManager` | 타이틀 비주얼라이저 바 (8큐브 Y축 스케일) |
| **Player** | `PlayerController` | Lerp 기반 가감속, SpeedRatio 노출, 수학 점프, 오디오 피치 동기화 |
| **Tunnel** | `TunnelGenerator` | 큐브/링 풀링, 트레드밀 스크롤, 에미션 + 색수차 제어 |
| | `TunnelRing` | Sin/Cos 원형 배치, 밴드별 방사 스케일링 |
| **Camera** | `CameraControl` | Focal Length ↔ SpeedRatio, Delta 기반 오디오 임펄스 |
| **UI** | `InGameManager` | 일시정지, 클리어 판정, 프로그레스 바, CompleteMixerReset |
| | `SceneLoader` | Enum 기반 씬 전환 + 에디터 친화적 래퍼 함수 |
| | `AnimationEvents` | UnityEvent 배열을 인덱스로 호출 (오프닝 연출용) |

### 클래스 간 관계

```
GameManager (Singleton, DontDestroyOnLoad)
 ├── InputManager (Singleton, 자식 등록)
 ├── AudioManager (Singleton, 자식 등록)
 │    ├── AudioMixer (스냅샷: Normal / TapeStop)
 │    ├── SelectedTrack (씬 간 데이터 유지)
 │    └── OnPlayStateChanged (이벤트 방송)
 └── Logger (Singleton, 프리팹)

PlayerController
 ├── SpeedRatio → Animator, TunnelGenerator, AudioManager
 ├── InputManager.OnJumpAction (이벤트 구독)
 └── 수학 점프 (Rigidbody 미사용)

TunnelGenerator
 ├── ObjectPool<TunnelRing> + ObjectPool<Transform>
 ├── PlayerController.SpeedRatio (터널 스크롤 속도)
 ├── AudioAnalyzer.BandBuffer (큐브 스케일 + 에미션)
 └── BeautifySettings (색수차 런타임 제어)

CameraControl
 ├── CinemachineCamera (Focal Length → FOV 변환)
 ├── CinemachineImpulseSource (Delta 감지 킥 임펄스)
 └── PlayerController.SpeedRatio (렌즈 + 게이팅)

InGameManager
 ├── AudioManager (CurrentTime/TotalTime, Pause/Resume, CompleteMixerReset)
 ├── InputManager (OnCancelAction → 일시정지, SetInputActive)
 └── ModalWindowManager (Modern UI Pack 모달)
```

## 사용한 C# / Unity 개념

### 1. 이벤트 기반 옵저버 패턴
이전 프로젝트(OverTheSky)에서 폴링 방식의 한계를 느낀 후, 이번에는 전면적으로 이벤트 기반으로 전환했다.
- `InputManager.OnJumpAction`, `OnCancelAction` → PlayerController, InGameManager가 구독
- `AudioManager.OnPlayStateChanged` → AudioAnalyzer가 구독하여 분석 on/off 자동 제어
- 매 프레임 상태를 물어보는 대신, 상태 변경 시점에만 이벤트 발생

### 2. SpeedRatio 단일 제어점
하나의 정규화 비율(0.0~1.0)로 애니메이션 속도, 터널 스크롤, 오디오 피치, 에미션 강도, 카메라 렌즈, 임펄스 강도를 동시에 제어한다. 각 시스템이 같은 값을 읽어 자연스러운 동기화가 보장되며, 제어점이 하나이므로 디버깅과 튜닝이 단순하다.

### 3. AudioMixer 스냅샷 보간 + SetFloat 분리
스냅샷으로 Lowpass/Reverb를 보간하되, Pitch는 SetFloat로 별도 제어하여 충돌을 방지했다. SetFloat로 조작한 파라미터는 스냅샷 전환만으로는 원복되지 않아 ClearFloat 코루틴이 필요했다.

### 4. 제네릭 오브젝트 풀
`ObjectPool<T> where T : Component`로 큐브와 링을 각각 독립 풀링. Queue<T> 기반 FIFO로 균등 순환하며, 풀 고갈 시 자동 확장한다.

### 5. 물리 카메라 수학
Focal Length(mm)를 수직 FOV(도)로 변환하기 위해 `fov = 2 × atan(sensorHeight / (2 × focalLength)) × Rad2Deg` 공식을 직접 구현했다. 센서 높이는 시네머신 카메라에서 런타임에 읽어 하드코딩을 회피했다.

### 6. 믹서 잠금(Race Condition 방어)
씬 초기화 중 PlayerController의 Update가 뒤늦게 실행되어 기껏 초기화한 믹서를 다시 오염시키는 문제를 `_isMixerLocked` 플래그로 해결했다. 초기화 시작 시 잠금 → ClearFloat 완료 후 새 곡 재생 시 해제.

## 잘했다고 생각하는 점

### 1. 이전 프로젝트 회고의 실천적 적용
OverTheSky 회고에서 "이벤트 기반 아키텍처 도입"과 "FSM 적용"을 개선점으로 도출했는데, Monotrum에서 옵저버 패턴 전환과 SpeedRatio 기반 상태 제어를 실제로 적용했다. 회고가 문서로 끝나지 않고 다음 프로젝트의 설계 판단에 반영된 점.

### 2. 스코프 조정 판단
Day 4에 FFT 지형 높이 반응, 중력 반전 QTE, 흑백 전환을 제거하고 비주얼 폴리싱에 집중하는 결정을 내렸다. 결과적으로 기획 외 추가 작업(카메라 연출, 색수차 반응)까지 넣을 여유가 생겨 비주얼 완성도가 원안보다 상승했다.

### 3. 성능을 고려한 설계
오브젝트 풀링(GC Alloc 0B), SRP Batcher 배칭 유지(sharedMaterial.SetColor), UI 갱신 주기 제한(0.5초 타이머) 등 처음부터 성능을 염두에 둔 설계 덕분에 4K 100fps 이상을 유지하면서 포스트 프로세싱(Bloom, 아나모픽 플레어, 비네팅, 색수차)을 전부 올릴 수 있었다.

### 4. 체계적 문서화
매일 작업 노트를 작성하고, 구현 원리(FFT 수학, 라디안, 아크탄젠트)까지 기록했다. 학습용 프로젝트에서 "왜 이렇게 구현했는지"를 남기는 것이 포트폴리오 가치를 높이는 핵심이라고 판단했고, 실제로 면접에서 설명할 수 있는 깊이가 확보되었다.

### 5. 버그 원인 분석과 체계적 해결
싱글톤 자식 소실, 믹서 스냅샷 복구 실패, Race Condition 등 복잡한 버그를 "현상 → 원인 → 수정 → 교훈" 형식으로 추적하고 해결했다. 특히 Destroy 지연 실행과 SetFloat/스냅샷 충돌은 Unity 내부 동작에 대한 이해가 깊어진 계기가 되었다.

## 아쉬웠던 점

### 1. InGameManager의 비대화
일시정지, 클리어, 프로그레스 바, 씬 리셋을 한 클래스에 몰아넣었다. PauseController, GameProgressTracker로 분리했으면 단일 책임 원칙에 더 부합했을 것. 막바지 일정에 쫓기며 구조보다 동작을 우선한 결과다.

### 2. Day 3~4 공백
생일 및 개인 일정으로 이틀간 개발에 시간을 거의 못 씀. 이 공백이 없었으면 UI까지 여유롭게 끝나고 마지막 날 밤샘을 피할 수 있었다. 일정 관리의 버퍼를 더 확보했어야 했다.

### 3. SelectedTrack이 public 필드
외부에서 직접 대입이 가능한 상태. 프로퍼티로 감싸서 set을 제한했으면 의도치 않은 변경을 방지할 수 있었다.

### 4. 인터페이스 및 이벤트 채널 부재
AudioManager, TunnelGenerator 등이 전부 `AudioManager.Instance.SetPitch()` 같은 싱글톤 직접 접근 방식이다. 돌이켜 보면 결합도를 낮추는 두 가지 패턴을 상황에 맞게 적용할 수 있었다.

- **인터페이스 (행위 계약)**: `SetPitch`, `PlayTrack`처럼 "~해줘"라고 명령하는 1:1 호출에 적합. `IAudioService`를 두면 구현체를 FMOD로 교체하거나 테스트용 Mock을 꽂아도 호출 코드가 안 변한다.
- **SO 이벤트 채널 (상태 방송)**: `OnPlayStateChanged`처럼 "~됐어"라고 불특정 다수에게 알리는 1:N 방송에 적합. 발신자와 구독자가 서로의 존재를 모르고, 인스펙터에서 같은 SO를 꽂으면 연결되므로 싱글톤 참조가 완전히 사라진다.

판단 기준은 단순하다: **동사가 "~해줘"면 인터페이스, "~됐어"면 SO 채널**. 처음부터 SO 채널로 만들 필요는 없고, `event Action`으로 시작해서 구독자가 2~3개를 넘어가며 싱글톤 참조가 퍼지기 시작하면 그때 SO로 승격하면 된다. 솔로 프로젝트의 짧은 일정에서는 오버엔지니어링이라 판단하여 생략했지만, 기준 자체는 다음 프로젝트에서 적용할 수 있다.

## 다시 만든다면 개선하고 싶은 점

### 1. InGameManager 분리
```
InGameManager (조율만)
 ├── PauseController (일시정지/재개/커서)
 └── GameProgressTracker (프로그레스 바 + 클리어 판정)
```

### 2. AudioManager에서 믹서 로직 분리
```
AudioManager (재생 전담)
 └── MixerController (SetPitch, 스냅샷, CompleteMixerReset, ClearFloat)
```

### 3. 인터페이스 도입 (행위 계약)
"~해줘" 패턴의 싱글톤 직접 호출을 인터페이스로 감싼다.
```csharp
public interface IAudioService
{
    void SetPitch(float ratio);
    void PlayTrack(TrackData track);
    bool IsPlaying { get; }
}

// PlayerController는 구체 클래스가 아닌 인터페이스에 의존
private IAudioService _audio;
void Update() => _audio.SetPitch(SpeedRatio);
```
`AudioManager`를 FMOD 래퍼로 교체하더라도 `IAudioService`만 구현하면 호출 코드는 수정 불필요.

### 4. ScriptableObject 이벤트 채널 (상태 방송)
"~됐어" 패턴의 이벤트를 SO 채널로 분리하여 싱글톤 참조를 완전히 제거한다.
```csharp
[CreateAssetMenu(menuName = "Events/BoolEventChannel")]
public class BoolEventChannel : ScriptableObject
{
    public event Action<bool> OnEventRaised;
    public void Raise(bool value) => OnEventRaised?.Invoke(value);
}

// AudioManager: 채널에 방송 (누가 듣는지 모름)
_playStateChannel.Raise(false);

// AudioAnalyzer: 채널을 구독 (AudioManager의 존재를 모름)
_playStateChannel.OnEventRaised += SetAnalyzeState;
```
처음부터 SO로 만들 필요는 없다. `event Action`으로 시작해서 구독자가 2~3개를 넘어가면 SO로 승격하는 것이 실용적인 판단 기준.

## 이전 프로젝트(OverTheSky)와의 비교

| 항목 | OverTheSky | Monotrum |
|------|-----------|----------|
| 입력 처리 | 폴링(ConsumeCancel) | 이벤트(OnCancelAction, OnJumpAction) |
| 상태 감지 | 프로퍼티 setter 직접 호출 | event Action 방송 + 구독 |
| 물리 | Rigidbody 직접 제어 | 수학 기반 (물리 미사용) |
| 문서화 | R&D 문서 + 역할 분담 | 매일 작업 노트 + 원리 설명 |
| 스코프 | 기능 구현에 시간 소모 | Day 4 스코프 조정으로 비주얼 집중 |
| 시스템 결합 | 싱글톤 직접 접근만 사용 | 옵저버 패턴 도입 (다음 단계: 인터페이스 + SO 채널) |

## 이번 프로젝트를 통해 배운 점

### 기술적 학습
- **FFT 오디오 분석**: GetSpectrumData, Blackman Window, 지수적 밴드 압축, 가속 감쇠 스무딩
- **AudioMixer 심화**: 스냅샷 보간, SetFloat vs 스냅샷 충돌, ClearFloat의 필요성
- **절차적 생성**: Sin/Cos 원형 배치, 트레드밀 무한 스크롤, 오브젝트 풀링
- **물리 카메라 수학**: Focal Length → FOV 아크탄젠트 변환
- **SRP Batcher**: SetGlobalColor vs Material.SetColor, UnityPerMaterial CBUFFER 우선순위

### 설계적 학습
- **단일 제어점(SpeedRatio)의 위력**: 하나의 값으로 전체 시스템을 동기화하면 확장과 디버깅이 압도적으로 쉬워진다
- **옵저버 패턴의 실전 적용**: 이전 프로젝트의 회고를 실천으로 옮긴 결과, 시스템 간 결합도가 확연히 줄었다
- **결합도를 낮추는 두 갈래**: "~해줘"(명령)는 인터페이스, "~됐어"(방송)는 SO 이벤트 채널. `event Action` 구독자가 2~3개를 넘으면 SO 승격을 고려하는 것이 실용적인 타이밍
- **Race Condition 방어**: Destroy 지연 실행, 코루틴 중첩, Update 잔존 실행 등 Unity 생명주기의 함정을 체감

### 프로젝트 관리 학습
- **스코프 조정은 감점이 아니다**: 제거한 기능 대신 비주얼 퀄리티를 올린 것이 오히려 더 높은 완성도로 이어졌다
- **문서화는 미래의 자산**: 작업 노트에 원리를 기록해둔 덕분에 회고록과 포트폴리오를 빠르게 작성할 수 있었다

## 한 줄 총평

> "FFT 오디오 분석부터 AudioMixer 스냅샷, 절차적 터널 생성, 물리 카메라 수학까지 — 9일간 하나의 비율값(SpeedRatio)으로 세계의 시간을 제어하는 공감각적 경험을 만들면서, 이전 프로젝트의 회고를 설계에 반영하는 성장 사이클을 완성했다."

---

**작성일**: 2026-03-06  
**최종 수정**: 2026-03-16  
**작성자**: 이성규