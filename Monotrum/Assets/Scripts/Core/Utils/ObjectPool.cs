using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 제네릭 오브젝트 풀. Component를 상속한 모든 타입에 사용 가능하다.
    /// Queue 기반 FIFO 구조로, 먼저 반환된 오브젝트가 먼저 재사용된다.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly T _prefab;       // 복제 원본
        private readonly Transform _parent; // 생성된 오브젝트의 부모 (하이어라키 정리용)
        
        // 풀 초기화. initialSize만큼 미리 생성하여 런타임 Instantiate 부하를 분산한다.
        public ObjectPool(T prefab, int initialSize, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < initialSize; i++)
                _pool.Enqueue(CreateInstance());
        }
        
        // 프리팹을 복제하고 비활성 상태로 풀에 대기시킨다.
        private T CreateInstance()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            return obj;
        }

        /// <summary>
        /// 풀에서 오브젝트를 꺼내 활성화한다.
        /// 풀이 비어있으면 새로 생성하여 반환한다 (풀 고갈 방지).
        /// </summary>
        public T Get()
        {
            var obj = _pool.Count > 0 ? _pool.Dequeue() : CreateInstance();
            obj.gameObject.SetActive(true);
            return obj;
        }
        
        // 사용이 끝난 오브젝트를 비활성화하고 풀에 반환한다.
        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}