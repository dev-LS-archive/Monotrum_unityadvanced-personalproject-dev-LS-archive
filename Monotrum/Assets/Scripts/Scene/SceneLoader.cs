using UnityEngine;
using UnityEngine.SceneManagement;
using Core;
using Logger = Core.Logger;

namespace Scene
{
    public class SceneLoader : MonoBehaviour
    {
        public void LoadScene(SceneType sceneType)
        {
            if(sceneType == SceneType.Title)
                SceneManager.LoadScene(Define.Scene.Title);
            else if(sceneType == SceneType.Game)
                SceneManager.LoadScene(Define.Scene.Game);
        }

        // 인스펙터 할당용 래퍼 함수들
        public void LoadTitleScene() => LoadScene(SceneType.Title);
        public void LoadGameScene() => LoadScene(SceneType.Game);

        // UI의 종료(Quit) 버튼에 할당할 함수
        public void QuitGame()
        {
            Logger.Instance.LogInfo("게임 종료 요청");
            
#if UNITY_EDITOR
            // 유니티 에디터 환경에서는 플레이 모드를 종료
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // 실제 빌드된 환경에서는 애플리케이션 종료
            Application.Quit();
#endif
        }
    }
}