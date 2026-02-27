using UnityEngine;

namespace Core
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        private static bool _isQuitting = false;
    
        public static T Instance
        {
            get
            {
                // 종료 상태면 null 반환
                if (_isQuitting)
                {
                    Debug.LogWarning($"[Singleton] 인스턴스 '{typeof(T)}' 는 이미 파괴됨. 재생성 방지.");
                    return null;
                }
            
                if (_instance == null)
                {
                    // 씬에 존재하는 인스턴스 찾아오기 
                    _instance = FindFirstObjectByType<T>();
                
                    // 인스턴스가 없다면 자동 생성
                    if (_instance == null)
                    {
                        var obj = new GameObject
                        {
                            // 개발 편의성을 위해 이름에 [Singleton] 추가.
                            name = $"[Singleton] {typeof(T).Name}"
                        };
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                // 루트 오브젝트일 때만 DontDestroyOnLoad 호출
                if (transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] {typeof(T).Name} 의 중복 인스턴스를 발견. 중복 삭제 실행.");
                Destroy(gameObject);
            }
        }
        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }
    }
}
