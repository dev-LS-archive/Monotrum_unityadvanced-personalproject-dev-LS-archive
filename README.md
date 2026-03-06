[![Open in Visual Studio Code](https://classroom.github.com/assets/open-in-vscode-2e0aaae1b6195c2367325f4f02e2d04e9abb55f0b24a779b69b11b9e10269abc.svg)](https://classroom.github.com/online_ide?assignment_repo_id=22871659&assignment_repo_type=AssignmentRepo)
---

<img src="https://img.shields.io/badge/Unity-2022.3.62f2-black?style=for-the-badge&logo=unity" alt="Unity"> <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C#"> <img src="https://img.shields.io/badge/Platform-Windows-0078D6?style=for-the-badge&logo=windows" alt="Platform">

---
## 문서 바로가기
[![작업노트-이성규](https://img.shields.io/badge/작업_노트-Click_Here-blue?style=for-the-badge&logo=readme)](./Docs/이성규%20작업%20노트(Monotrum%20).md)

---

마감 직전 최고의 판단입니다! 유저나 평가자가 가장 먼저 보게 되는 것이 **조작법**이어야 바로 게임을 실행해볼 수 있으니까요. 가독성을 극대화하여 조작법을 최상단으로 올린 최종 README 양식입니다.

지금 바로 복사해서 깃허브 `README.md`에 붙여넣으세요!

---

# 🎵 Monotrum (모노트럼)

**음악의 파동을 시각적으로 체험하는 3D 리듬 액션 게임**

## 조작법 (Controls)

게임을 시작하기 전, 아래 조작법을 확인해 주세요!

* **W (Hold)**: **전진** - 유지 시 정속 주행 및 음악이 정상 재생됩니다.
* **W (Release)**: **감속** - 키를 떼면 '테이프 스탑' 연출과 함께 세상이 멈춥니다.
* **Space**: **점프** - 캐릭터의 역동적인 움직임을 위한 비주얼 연출 요소입니다.
* **Esc**: **일시정지** - Pause 패널을 통해 타이틀로 돌아가거나 설정을 변경할 수 있습니다.

---

## 플레이 영상

> **[Monotrum 플레이 영상 (유튜브 링크)]**
> *(https://youtu.be/BigXLCgWW_8?si=ZeLk_Svd6pPhRDfs)*

---

## 프로젝트 개요

**Monotrum**은 실시간 오디오 스펙트럼 분석 기술(FFT)을 활용하여 음악과 시각적 경험을 하나로 통합한 체험형 리듬 액션 게임입니다.

* **개발 기간**: 2026.02.26 ~ 2026.03.06 (9일)
* **개발 환경**: Unity 6 (6000.3.9f1 LTS) / URP
* **핵심 컨셉**: 무채색 배경 위 화려한 툰쉐이딩 캐릭터의 대비

---

## 핵심 시스템 (Core Features)

### 1. 실시간 오디오 리액티브 (FFT)

* 오디오 신호를 512개 샘플로 분해하여 8개 주파수 밴드로 시각화합니다.
* 터널 큐브들이 저음/고음 밴드에 맞춰 실시간으로 확장 및 발광합니다.

### 2. 테이프 스톱 (Tape Stop) 연출

* 플레이어의 속도와 오디오 피치(Pitch), 로우패스 필터를 동기화했습니다.
* 감속 시 화면이 흑백으로 전환되며 적막한 분위기를 자아냅니다.

### 3. 고품질 비주얼 및 연출

* **Beautify 3**: 아나모픽 플레어 및 블룸을 활용한 사이버네틱 비주얼을 구현했습니다.
* **Cinemachine Impulse**: 음악의 킥 베이트에 맞춘 역동적인 카메라 진동을 제공합니다.

---

## 기술적 최적화 (Optimization)

* **백그라운드 연산 차단**: 일시정지 시 오디오 분석 이벤트를 정지하여 CPU 누수를 막았습니다.
* **프레임 안정화**: 144 FPS 고정 및 VSync 해제로 부드러운 조작감을 확보했습니다.
* **멀티스레드 물리**: Magica Cloth 2와 Unity Jobs/Burst를 활용해 물리 연산 부하를 최적화했습니다. (에셋사용)

---

## 사용 에셋

* **Visual**: Beautify 3, Modern UI Pack, Unity Toon Shader
* **Physics**: Magica Cloth 2
* **Character**: Unity-Chan (Sunny Side Up)

---

임시 문서입니다.
