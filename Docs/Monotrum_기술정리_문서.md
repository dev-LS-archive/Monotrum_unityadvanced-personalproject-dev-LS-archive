# Monotrum 기술정리 문서

**작성자**: 이성규  
**게임명**: Monotrum (모노트럼)  
**개발 환경**: Unity 6 LTS / C# / URP / Cinemachine 3 / Beautify 3

## 개요

프로젝트의 핵심 시스템 설계, 클래스 간 관계, 데이터 흐름을 정리한 기술 문서.

---

## 1. 시스템 아키텍처 전체도

```mermaid
graph TB
    subgraph Core["Core Framework (DontDestroyOnLoad)"]
        GM[GameManager]
        IM[InputManager]
        AM[AudioManager]
        LG[Logger]
        GM -->|RegisterManager| IM
        GM -->|RegisterManager| AM
    endㄷㄷ

    subgraph Audio["Audio System"]
        AA[AudioAnalyzer]
        TD[(TrackData SO)]
        TC[TrackController]
        TM[TrackManager]
    end

    subgraph Game["Game Scene"]
        PC[PlayerController]
        TG[TunnelGenerator]
        TR[TunnelRing]
        CC[CameraControl]
        IGM[InGameManager]
    end

    subgraph UI["UI Layer"]
        SL[SceneLoader]
        AE[AnimationEvents]
    end

    AM -->|OnPlayStateChanged| AA
    IM -->|OnJumpAction| PC
    IM -->|OnCancelAction| IGM
    PC -->|SpeedRatio| TG
    PC -->|SpeedRatio| CC
    PC -->|SetPitch| AM
    AA -->|BandBuffer| TG
    AA -->|BandBuffer| CC
    AA -->|BandBuffer| TM
    TD -->|themeColor, scrollSpeed| TG
    TD -->|clip| AM
    TG -->|Initialize| TR
    TG -->|BeautifySettings| PP[Beautify 3]
    CC -->|CinemachineImpulse| CM[Cinemachine 3]
    IGM -->|Pause/Resume/Stop| AM
    IGM -->|SetInputActive| IM
    TC -->|SelectTrack| AM

    style Core fill:#1A3A3A,stroke:#00E5CC,color:#F0F0F0
    style Audio fill:#1A2A3A,stroke:#00E5CC,color:#F0F0F0
    style Game fill:#2A1A2A,stroke:#FF6B9D,color:#F0F0F0
    style UI fill:#1A1A2E,stroke:#8A8A9A,color:#F0F0F0
```

---

## 2. 클래스 다이어그램

```mermaid
classDiagram
    class Singleton~T~ {
        -static T _instance
        -static bool _isQuitting
        +static T Instance
        +static bool IsQuitting
        #virtual Awake()
        #virtual OnApplicationQuit()
    }

    class GameManager {
        -InitializeManagers()
        -RegisterManager~T~()
        +SetCursorLocked(bool)
    }

    class InputManager {
        -MonotrumInput _input
        -bool _isInputBlocked
        +event Action OnCancelAction
        +event Action OnJumpAction
        +Vector2 MoveInput
        +SetInputActive(bool)
    }

    class AudioManager {
        -AudioSource _audioSource
        -AudioMixer _audioMixer
        -AudioMixerSnapshot _normalSnapshot
        -AudioMixerSnapshot _tapeStopSnapshot
        -bool _isMixerLocked
        +TrackData SelectedTrack
        +event Action~bool~ OnPlayStateChanged
        +float TotalTime
        +float CurrentTime
        +bool IsPlaying
        +PlayTrack(TrackData, bool)
        +SelectTrack(TrackData)
        +PauseMusic()
        +ResumeMusic()
        +StopMusic()
        +SetPitch(float)
        +CompleteMixerReset(float)
        -ClearMixerRoutine(float)
    }

    class AudioAnalyzer {
        -float[] _samples
        -float[] _freqBands
        -float[] _bandBuffer
        -float[] _bufferDecrease
        -bool _isAnalyzing
        +float[] BandBuffer
        -ComputeFreqBands()
        -ApplySmoothing()
    }

    class TrackData {
        +string trackName
        +string trackArtist
        +AudioClip clip
        +float bpm
        +Color themeColor
        +float trackScrollSpeed
    }

    class PlayerController {
        -float _currentSpeed
        -float _maxRunSpeed
        -float _speedLerpRate
        +float SpeedRatio
        -HandleRunAndTapeStop()
        -HandleMathJump()
        -TriggerJump()
    }

    class TunnelGenerator {
        -ObjectPool~TunnelRing~ _ringPool
        -ObjectPool~Transform~ _cubePool
        -Queue~TunnelRing~ _activeRings
        -Material _cubeMaterial
        -SpawnRing()
        -RecycleRings()
        -UpdateRingVisuals()
        -UpdateEmission()
        +UpdateAberrationIntensity(float)
    }

    class TunnelRing {
        -Transform[] _cubes
        -Vector3[] _baseScales
        +Initialize(Transform[], float)
        +UpdateVisual(float[], float)
        +DetachCubes() Transform[]
    }

    class CameraControl {
        -float _sensorHeight
        -float _currentFocalLength
        -float _previousPulse
        -float _shakeCooldown
        -UpdateFocalLength()
        -HandleAudioImpulse()
    }

    class ObjectPool~T~ {
        -Queue~T~ _pool
        -T _prefab
        +Get() T
        +Return(T)
    }

    Singleton~T~ <|-- GameManager
    Singleton~T~ <|-- InputManager
    Singleton~T~ <|-- AudioManager
    Singleton~T~ <|-- Logger

    GameManager --> InputManager : RegisterManager
    GameManager --> AudioManager : RegisterManager
    AudioManager --> AudioAnalyzer : OnPlayStateChanged
    AudioManager ..> TrackData : SelectedTrack
    PlayerController --> AudioManager : SetPitch(SpeedRatio)
    PlayerController --> InputManager : OnJumpAction 구독
    TunnelGenerator --> PlayerController : SpeedRatio 읽기
    TunnelGenerator --> AudioAnalyzer : BandBuffer 읽기
    TunnelGenerator --> TunnelRing : Initialize, UpdateVisual
    TunnelGenerator --> ObjectPool~TunnelRing~ : Get/Return
    TunnelGenerator --> ObjectPool~Transform~ : Get/Return
    CameraControl --> PlayerController : SpeedRatio 읽기
    CameraControl --> AudioAnalyzer : BandBuffer 읽기
```

---

## 3. SpeedRatio 데이터 플로우

게임의 핵심 제어 구조. 하나의 정규화 비율로 전체 시스템을 동기화한다.

```mermaid
flowchart LR
    subgraph Input["입력"]
        W[W키 입력<br/>MoveInput.y]
    end

    subgraph PC["PlayerController"]
        LERP["Lerp 보간<br/>_currentSpeed"]
        SR["SpeedRatio<br/>0.0 ~ 1.0"]
        SNAP["0.99↑→1.0<br/>0.01↓→0.0"]
    end

    subgraph Consumers["소비자"]
        ANIM["Animator<br/>MotionSpeed"]
        TUNNEL["TunnelGenerator<br/>scrollSpeed × ratio"]
        PITCH["AudioManager<br/>SetPitch(ratio)"]
        EMISSION["에미션 강도<br/>Lerp(min, 1+pulse, ratio)"]
        FOCAL["CameraControl<br/>Lerp(75mm, 50mm, ratio)"]
        IMPULSE["임펄스 강도<br/>pulse × mult × ratio"]
    end

    W --> LERP --> SR --> SNAP
    SNAP --> ANIM
    SNAP --> TUNNEL
    SNAP --> PITCH
    SNAP --> EMISSION
    SNAP --> FOCAL
    SNAP --> IMPULSE

    style Input fill:#2A2A1A,stroke:#FFD93D,color:#F0F0F0
    style PC fill:#1A3A3A,stroke:#00E5CC,color:#F0F0F0
    style Consumers fill:#1A1A2E,stroke:#8A8A9A,color:#F0F0F0
```

---

## 4. FFT 오디오 파이프라인

```mermaid
sequenceDiagram
    participant AS as AudioSource
    participant AM as AudioManager
    participant AA as AudioAnalyzer
    participant TG as TunnelGenerator
    participant CC as CameraControl

    Note over AM: 곡 재생 시작
    AM->>AM: OnPlayStateChanged(true)
    AM-->>AA: _isAnalyzing = true

    loop 매 프레임 (Update)
        AA->>AM: GetSpectrumData(_samples)
        AM->>AS: AudioSource.GetSpectrumData(512, Blackman)
        AS-->>AA: float[512] FFT bins

        Note over AA: ComputeFreqBands()
        AA->>AA: 512 bins → 8밴드 압축<br/>지수적 bin 배분 + 가중치 보상

        Note over AA: ApplySmoothing()
        AA->>AA: 상승: 즉시 반영<br/>하강: bufferDecrease *= 1.2f

        AA-->>TG: BandBuffer[8]
        TG->>TG: UpdateRingVisuals()<br/>큐브 스케일 = base + band[i%8]
        TG->>TG: UpdateEmission()<br/>intensity = Lerp(min, 1+pulse, ratio)
        TG->>TG: UpdateAberrationIntensity()<br/>CA = pulse × maxAberration

        AA-->>CC: BandBuffer[0]
        CC->>CC: pulseDelta = current - previous
        alt pulseDelta > threshold && cooldown <= 0
            CC->>CC: GenerateImpulseWithForce(pulse × mult × ratio)
        end
    end
```

---

## 5. 테이프 스톱 연출 시퀀스

```mermaid
sequenceDiagram
    participant User as 플레이어
    participant IM as InputManager
    participant PC as PlayerController
    participant AM as AudioManager
    participant TG as TunnelGenerator
    participant CC as CameraControl
    participant BF as Beautify

    Note over User: W키 해제
    User->>IM: MoveInput.y = 0

    loop 매 프레임 (감속)
        PC->>PC: _currentSpeed = Lerp(current, 0, dt × rate)
        PC->>PC: SpeedRatio 감소 (1.0 → 0.0)

        PC->>AM: SetPitch(SpeedRatio)
        AM->>AM: Pitch = Clamp(ratio, 0.05, 1.0)
        AM->>AM: TransitionToSnapshots([Normal, TapeStop], [ratio, 1-ratio])

        Note over TG: moveStep = scrollSpeed × SpeedRatio × dt
        TG->>TG: 터널 감속 → 정지

        Note over TG: intensity = Lerp(0.1, 1+pulse, ratio)
        TG->>TG: 에미션 암전
        TG->>BF: chromaticAberration 활성화

        CC->>CC: focalLength = Lerp(75mm, 50mm, ratio)<br/>→ 75mm 망원 (압축감)
    end

    Note over User: W키 재입력
    User->>IM: MoveInput.y = 1

    loop 매 프레임 (가속)
        PC->>PC: SpeedRatio 증가 (0.0 → 1.0)
        PC->>AM: SetPitch(1.0)
        AM->>AM: _normalSnapshot.TransitionTo(0f)<br/>원음 즉시 복구
    end
```

---

## 6. 씬 전환 흐름

```mermaid
flowchart TD
    subgraph Title["타이틀 씬"]
        direction TB
        OPEN[오프닝 연출]
        SELECT[곡 선택 셀렉터]
        PREVIEW[미리듣기 + 비주얼라이저]
        PLAY[Play 버튼]

        OPEN -->|AnimationEvents| SELECT
        SELECT -->|OnItemSelect| PREVIEW
        PREVIEW --> PLAY
    end

    subgraph Transit["씬 전환 (SceneLoader)"]
        direction TB
        STOP[StopMusic]
        INPUT_OFF[Input 차단]
        LOAD[LoadScene]
        
        STOP --> INPUT_OFF --> LOAD
    end

    subgraph Game["게임 씬"]
        direction TB
        INIT[PlaySelectedTrack]
        INPUT_ON[Input 활성화]
        RUN[게임 플레이]
        
        PAUSE{ESC 입력?}
        CLEAR{재생 종료?}
        
        PAUSE_UI[일시정지 UI]
        CLEAR_UI[클리어 UI]
        RESET[Mixer Reset + Scene Reset]
        
        INIT --> INPUT_ON --> RUN
        RUN --> PAUSE & CLEAR
        
        PAUSE -->|Yes| PAUSE_UI
        CLEAR -->|Yes| CLEAR_UI
        
        PAUSE_UI -->|Resume| RUN
        PAUSE_UI -->|Title| RESET
        CLEAR_UI -->|Title| RESET
    end

    PLAY ==> Transit
    LOAD ==> INIT
    RESET ==> STOP

    style Title fill:#1A3A3A,stroke:#00E5CC,color:#F0F0F0
    style Transit fill:#2A2A1A,stroke:#FFD93D,color:#F0F0F0
    style Game fill:#2A1A2A,stroke:#FF6B9D,color:#F0F0F0
```

---

## 7. 오브젝트 풀링 & 터널 순환

```mermaid
flowchart LR
    subgraph Pool["오브젝트 풀"]
        RP["링 풀<br/>Queue&lt;TunnelRing&gt;"]
        CP["큐브 풀<br/>Queue&lt;Transform&gt;"]
    end

    subgraph Active["활성 터널"]
        R1["링 1 (가장 오래됨)"]
        R2["링 2"]
        R3["..."]
        RN["링 40 (최신)"]
    end

    subgraph Recycle["회수 조건"]
        CHECK{"ring.z <<br/>player.z - despawn?"}
    end

    RP -->|Get| RN
    CP -->|Get ×64| RN
    R1 --> CHECK
    CHECK -->|Yes| DETACH["DetachCubes()"]
    DETACH -->|큐브 반환| CP
    DETACH -->|링 반환| RP
    CHECK -->|No| KEEP["유지"]

    style Pool fill:#1A3A3A,stroke:#00E5CC,color:#F0F0F0
    style Active fill:#1A1A2E,stroke:#8A8A9A,color:#F0F0F0
```

---

## 8. AudioMixer 스냅샷 구조

```mermaid
flowchart TB
    subgraph Mixer["AudioMixer"]
        PITCH["Pitch<br/>(SetFloat 별도 제어)"]
        subgraph Normal["Normal 스냅샷"]
            NL["Lowpass: 22000Hz"]
            NR["Reverb Room: -10000"]
        end
        subgraph TapeStop["TapeStop 스냅샷"]
            TL["Lowpass: 400Hz"]
            TR2["Reverb Room: -1000"]
        end
    end

    SR["SpeedRatio"] -->|ratio >= 1.0| INSTANT["Normal.TransitionTo(0f)<br/>즉시 전환 (원음 보장)"]
    SR -->|ratio < 1.0| BLEND["TransitionToSnapshots<br/>[ratio, 1-ratio]"]
    SR -->|Clamp 0.05~1.0| PITCH

    subgraph Reset["CompleteMixerReset"]
        LOCK["_isMixerLocked = true"]
        SET["SetFloat → 기본값"]
        SNAP["Normal.TransitionTo"]
        CLEAR["ClearFloat (코루틴)<br/>WaitForSecondsRealtime"]
        LOCK --> SET --> SNAP --> CLEAR
    end

    style Mixer fill:#1A1A2E,stroke:#8A8A9A,color:#F0F0F0
    style Normal fill:#1A3A3A,stroke:#00E5CC,color:#F0F0F0
    style TapeStop fill:#2A1A2A,stroke:#FF6B9D,color:#F0F0F0
    style Reset fill:#2A2A1A,stroke:#FFD93D,color:#F0F0F0
```

---

## 9. 핵심 수식 정리

| 시스템 | 수식 | 설명 |
|--------|------|------|
| **SpeedRatio** | `_currentSpeed / _maxRunSpeed` | 0.0~1.0 정규화, 0.99↑→1.0 / 0.01↓→0.0 스냅 |
| **에미션 강도** | `Lerp(minEmission, 1 + audioPulse, SpeedRatio)` | ratio 1: themeColor + 맥박, ratio 0: 10%까지 암전 |
| **FOV 변환** | `2 × atan(sensorHeight / (2 × focalLength)) × Rad2Deg` | 물리 카메라 센서(24mm) + 초점거리(50~75mm) → 수직 FOV |
| **임펄스 강도** | `currentPulse × multiplier × SpeedRatio` | Delta 감지(pulseDelta > threshold) 시에만 발동 |
| **스냅샷 보간** | `TransitionToSnapshots([Normal, TapeStop], [ratio, 1-ratio], dt)` | Pitch는 SetFloat 별도 제어 (충돌 방지) |
| **호 길이** | `2πr / cubesPerRing` | 터널 반지름과 큐브 수로 빈틈 없는 크기 자동 계산 |
| **원형 배치** | `(Cos(θ)×r, Sin(θ)×r, 0)` | θ = (360° / 큐브 수) × i × Deg2Rad |
| **스무딩 감쇠** | `bandBuffer -= bufferDecrease; bufferDecrease *= 1.2` | 매 프레임 가속 감쇠 → 자연스러운 하강 곡선 |
| **색수차** | `Clamp(audioPulse × maxChromaticAberration, 0, 0.1)` | 킥 순간만 색수차 발생 |

---

## 10. 해결한 주요 버그

| 버그 | 원인 | 해결 |
|------|------|------|
| 싱글톤 자식 소실 | Destroy 지연 실행 중 InitializeManagers가 reparent | `if (Instance != this) return` 가드 추가 |
| 믹서 스냅샷 미복구 | SetFloat가 스냅샷 통제를 탈취 | ClearFloat 코루틴으로 오버라이드 해제 |
| 코루틴 정지 | Time.timeScale 0에서 WaitForSeconds 무한 대기 | WaitForSecondsRealtime으로 교체 |
| 믹서 막타 오염 | 초기화 중 PlayerController.Update 잔존 실행 | _isMixerLocked 플래그로 원천 차단 |
| nextSpawnZ 이격 | 링 이동량과 스폰 좌표 미동기 | moveStep만큼 nextSpawnZ도 감산 |
| 스케일 덮어쓰기 | UpdateVisual에서 Vector3.one × scale | _baseScales 보존 후 방사 방향만 가산 |
| SetGlobalColor 무반응 | SRP Batcher CBUFFER가 글로벌보다 우선 | sharedMaterial.SetColor로 직접 갱신 |
| ResumeMusic 처음 재생 | Play()가 처음부터 재생 | UnPause()로 교체 |

---

## 11. 성능 지표

| 항목 | 수치 | 비고 |
|------|------|------|
| 4K FPS | 100+ | 포스트 프로세싱(Bloom, 아나모픽, 비네팅, 색수차) 전부 포함 |
| GC Alloc | 0B | TunnelGenerator.Update 기준 |
| 활성 오브젝트 | 2,560 | 64큐브 × 40링 |
| Batches | 135 | SRP Batcher 유지 (sharedMaterial 방식) |
| CPU Main | 13.8ms | 에디터 기준 (빌드 시 더 낮음) |