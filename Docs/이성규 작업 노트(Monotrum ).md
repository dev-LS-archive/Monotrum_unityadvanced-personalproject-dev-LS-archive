# Monotrum_작업 노트
작업자: 이성규

게임명: Monotrum (모노트럼) 

## 프로젝트 개요

- **진행 기간**: 2026.02.26(목)~2026.03.06(금): [6일]
- **개발 환경**: Unity / C#
- **유니티 버전**: 6.3 LTS
- **프로젝트 목표**
  - 유니티가 제공하는 `GetSpectrumData()`와 `FFTWindow` 기능을 사용하여 오디오 스펙트럼 데이터로 오디오 비주얼 라이징과 비슷한 방식으로 오디오 스팩트럼 데이터를 활용한 실시간 지형 및 비주얼 변경의 구현
  - 리듬게임의 어려움을 배제한 체험형 음악과 시각적 경험을 동시 제공
  - 대비적으로 화려한 툰쉐이딩 캐릭을 통한 오디오 런 액션 구현
  - 포스트 프로세싱과 캐릭터, 오디오를 통한 절차적 맵생성을 통해 최소한의 리소스로 만족스런 시각적 경험 개발.

## 작업 노트

### 2026-02-24 (사전 작업)

프로젝트 Monotrum 기획서 작성 및 사용 기술 검토

### 2026-02-26 (작업 시작)


#### 에셋 및 리소스 선정

##### 비주얼 요소

개인 소유 유료 에셋을 통해 게임 비주얼의 향상에 사용한다.

**Modern UI Pack**  
![alt text](Resources/image-4.png)
게임 분위기에 맞는 깔끔한 느낌의 UI 에셋 사용

[[ AssetStore-RiderFlow ]](https://assetstore.unity.com/packages/tools/gui/modern-ui-pack-201717)

**Beautify 3 - Advanced Post Processing**
![alt text](Resources/Beautify_3.png)
BIRP 및 URP와 호환되는 포스트 프로세싱 에셋으로 기존 유니티 기본 후처리보다 다양하고 퀄리티 높은 후처리 기술을 다수 제공한다.

[[ AssetStore-Beautify 3 ]](https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/beautify-3-advanced-post-processing-233073)

**LUT Pack for Beautify**
![alt text](Resources/LUT_Pack.png)
Beautify 3를 위해 만들어진 LUT 텍스쳐로 비주얼 개선 및 화면 전체 색상의 일관성을 확보한다.


[[ AssetStore-LUT Pack for Beautify ]](https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/lut-pack-for-beautify-202502)

##### 편의성 플러그인 설치

**RiderFlow**
- 에디터상에서 쉽게 코드를 보거나 수정할 수 있다.
- 쉽게 에셋을 검색해 맵 상에 배치할 수 있다.
- 게임 오브젝트 북마크로 쉽게 오브젝트를 찾을 수 있다.
- 씬뷰 카메라 시점을 저장해두고 편하게 씬뷰를 변경할 수 있다.
- 하이어라키 커스텀이나 메모등을 남겨둘 수 있다.

[[ AssetStore-RiderFlow ]](https://assetstore.unity.com/packages/tools/level-design/riderflow-218574?locale=ko-KR&srsltid=AfmBOopfpDas0VOG9BZpZUti8Bt4dKOoAT_jWbs3Mk3szN50XD26S4OO)


##### 유니티 패키지
**ProBuilder**
큐브 이외의 도형이 필요한 경우 사용

**Recorder**
인게임 스크린샷 및 녹화를 위해서 설치

**TextMeshPro**
UI 표시용으로 필수 설치

**Post Processing**
비주얼을 잡아주기 위한 후보정 작업용.  

##### 음악
유튜브 오디오 보관함 사용
로열티 없는 음악 사용 가능
![alt text](Resources/ytAudio.png)

사용에는 문제가 없지만 2차 배포는 금지기에 라이선스 준수를 위해 이그노어 처리한 임포트 폴더에 넣어둠.

**사용 캐릭터**: Unity-Chan 공식 캐릭터 (ユニティちゃんSunny Side Up)
![alt text](Resources/image-1.png)
  - 다운로드 링크: https://unity-chan.com/download/releaseNote.php?id=ssu_urp


#### 기초 프로젝트 세팅

유니티 6000.3.9f1 버전으로 URP 3D 프로젝트 생성


---

**작성일**: 2026-02-24  
**최종 수정**: 2026-02-26