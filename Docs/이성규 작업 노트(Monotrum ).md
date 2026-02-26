# Monotrum_작업 노트

**작성자**: 이성규  
**게임명**: Monotrum (모노트럼)  
**작성일**: 2026-02-24  
**최종 수정**: 2026-02-26

## 프로젝트 개요

- **진행 기간**: 2026.02.26(목)~2026.03.06(금): [6일]
- **개발 환경**: Unity / C#
- **유니티 버전**: 6.3 LTS

### **프로젝트 목표**
1. **오디오 리액티브 지형 생성** — `GetSpectrumData()`와 `FFTWindow`를 활용하여 오디오 스펙트럼 데이터 기반의 실시간 지형 및 비주얼 변경 구현
2. **체험형 오디오 런 액션** — 리듬게임의 난이도 장벽을 배제하고, 음악과 시각 경험을 동시에 제공하는 러닝 액션 게임
3. **대비적 툰쉐이딩 캐릭터** — 무채색 배경 위에 화려한 툰쉐이딩 캐릭터를 배치하여 시각적 대비 효과 극대화
4. **최소 리소스, 최대 비주얼** — 포스트 프로세싱 + 절차적 맵 생성을 통해 최소한의 리소스로 만족스러운 시각적 경험 달성

---

## 에셋 및 리소스 구성

### 비주얼 에셋 (개인 소유 유료)

개인 소유 유료 에셋을 통해 게임 비주얼의 향상에 사용한다.

**Modern UI Pack** — [Asset Store](https://assetstore.unity.com/packages/tools/gui/modern-ui-pack-201717)  
![alt text](Resources/image-4.png)
게임 분위기에 맞는 깔끔한 느낌의 UI 에셋 사용

**Beautify 3 - Advanced Post Processing** — [Asset Store](https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/beautify-3-advanced-post-processing-233073)
![alt text](Resources/Beautify_3.png)
BIRP 및 URP와 호환되는 포스트 프로세싱 에셋으로 기존 유니티 기본 후처리보다 다양하고 퀄리티 높은 후처리 기술을 다수 제공한다.

**LUT Pack for Beautify**
![alt text](Resources/LUT_Pack.png) — [Asset Store](https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/lut-pack-for-beautify-202502)
Beautify 3를 위해 만들어진 LUT 텍스쳐로 비주얼 개선 및 화면 전체 색상의 일관성을 확보한다.

### 편의성 플러그인

**RiderFlow** — [Asset Store](https://assetstore.unity.com/packages/tools/level-design/riderflow-218574)

에디터 내 코드 확인/수정, 에셋 검색 및 배치, 게임 오브젝트 북마크, 씬뷰 카메라 시점 저장, 하이어라키 커스텀 및 메모 기능 제공

### Unity 패키지

**ProBuilder**: 큐브 이외의 도형이 필요한 경우 사용<br>
**Recorder**: 인게임 스크린샷 및 녹화를 위해서 설치<br>
**TextMeshPro**: UI 표시용으로 필수 설치<br>
**Cinemachine**: 카메라의 동적인 움직임을 위하여 설치

### 음악

![alt text](Resources/ytAudio.png)

유튜브 오디오 보관함의 로열티 프리 음악을 사용한다. 2차 배포가 금지되어 있으므로 `.gitignore` 처리된 임포트 폴더에 별도 관리한다.

### 폰트

**Google Noto Sans** — https://fonts.google.com/noto  
SIL Open Font License로 배포 허용. 라이선스 파일을 포함하여 커밋 가능.

### 3D 캐릭터

**Unity-Chan (ユニティちゃん Sunny Side Up)**  
다운로드: https://unity-chan.com/download/releaseNote.php?id=ssu_urp

**선정 이유**: 무채색 배경과의 색상 대비 효과가 뛰어나고, 캐릭터의 후드와 신발의 외형이 음악을 들으며 달리는 게임 컨셉에 적합하다.

---

## 3. 작업 일지

### 2026-02-24 (사전 작업)

프로젝트 Monotrum 기획서 작성 및 사용 기술 검토

### Day 1 — 2026-02-26 (작업 시작)

#### 기초 프로젝트 세팅

- Unity 6000.3.9f1 버전으로 URP 3D 프로젝트 생성
- 각종 에셋 및 패키지 임포트
- 폰트 임포트 및 SDF 생성

##### 캐릭터 임포트

캐릭터를 가져오기 전, 다음 패키지를 선행 설치한다

- **Unity Toon Shader** - [매뉴얼](https://docs.unity3d.com/Packages/com.unity.toonshader@0.13/manual/installation.html)
  - `com.unity.toonshader`을 통해 패키지 파일을 설치한다.
- **Unity Chan Spring Bone** - [GitHub](https://github.com/unity3d-jp/UnityChanSpringBone.git#1.2.1-preview)
  - 캐릭터 본 물리 움직임을 담당한다.

#### 캐릭터 가져오기
![alt text](Resources/UnityChanSSU.png)
- 다운로드 받은 프로젝트 파일을 연다.
  - 사진과 같이 잘 세팅된 캐릭터를 확인할 수 있다.

- ![alt text](Resources/SSU_P.png)
- 다운로드 받은 프로젝트에서 캐릭터만 패키지 파일로 추출하여 작업 프로젝트에 임포트했다

- ![alt text](Resources/GetUT_SSU.png)
- 가져오는데 성공한 모습

#### 추가 리소스 임포트

![alt text](Resources/Unity-Chan!_Model.png)
**Unity-Chan! Model** 에셋을 추가 임포트하여 부족한 애니메이션 리소스를 보충했다.

다만 해당 에셋의 애니메이션이 개발 방향에 적합하지 않아, **Starter Assets - ThirdPerson** ([Asset Store](https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526)) 에셋의 애니메이션을 대신 활용한다.

![alt text](Resources/st.png)

#### 정상동작 확인 및 성능 이슈 확인
![alt text](Resources/Test_1.png)
스타터 에셋의 애니메이션이 정상 동작하는걸 확인

![alt text](Resources/Test_2.png)
하지만 테스트 중에 프레임이 무거워지거나 끊기는 현상 확인.
프레임이 일정하게 유지 되지 않는다.

![alt text](Resources/Test_3.png)
따라서 프로파일러를 확인한 결과 캐릭터의 본을 움직여주는 Spring Bone에서 1.1 KB GC Alloc이 발생하는 성능 이슈를 확인했다.

CPU Main 9.9ms: 스크린샷에서는 CPU 연산 시간이 9.9ms로 렌더 스레드(5.0ms)보다 훨씬 길게 잡히며 그 이유 중 하나가 이 높은 폴리곤 수와 스프링본 연산의 결합 때문일 가능성이 크다.

![alt text](Resources/Test_4.png)
또한 해당 캐릭터는 58개의 스프링본을 처리하는데 있어 성능을 많이 잡아먹는 것을 유추할 수 있었다.

#### 해결 과정

`Unity Chan Spring Bone`은 꽤 옛날에 만들어진 시스템으로 현대에 와서 사용하기엔 성능 이슈가 있을 수 있다. 따라서 쉽고 빠르게 성능 좋은 피직스본을 도입하기 위해 `Magica Cloth 2` 에셋을 도입한다.

![alt text](Resources/MagicaCloth2-AssetStore.png)
[Magica Cloth 2-AssetStore](https://assetstore.unity.com/packages/tools/physics/magica-cloth-2-242307)

Unity DOTS를 사용해 멀티스레딩을 지원하므로 CPU에 코어(스레드)가 많은 기기일수록 성능 향상 폭이 크다.  
지속적인 업데이트 및 유니티6로 넘어와서 생긴 신기능들에 대한 지원에도 적극적인 에셋이다.

[Magica Cloth 2_성능 분석 포스트](https://wisdom-atelier.tistory.com/m/170)  
[MagicaCloth2-공식문서](https://magicasoft.jp/en/magica-cloth-2-2/)

![alt text](Resources/AnimationRigging.png)
또한 캐릭터의 본을 시각적으로 확인하기 위해 `Animation Rigging` 패키지를 설치했다.

![alt text](image.png)

본 교체 작업 도중의 프로파일러지만 Magica Cloth는 일절의 GC Alloc을 발생시키지 않았다.
다만 에디터 차원에서 부하가 커졌는데 이는 Magica Cloth가 버스트(Burst)와 잡 시스템(Job System)을 사용해 에디터가 이를 모니터링 하는등의 부하가 발생해 빌드시 성능보다 에디터상에서 성능이 떨어지게 된다.

상기에서 언급한 Magica Cloth 2_성능 분석 포스트를 보면 Burst는 에디터에서만 런타임(Just-In-Time Compiler)에서 컴파일된다. 따라서 PlayerSettings의 Editor 탭에 있는 Enter Play Mode Options을 사용하면 반복적인 플레이 작업 후에도 Burst가 다시 JIT 컴파일되지 않는다.
