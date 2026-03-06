using UnityEngine;
using UnityEngine.Events;
using Logger = Core.Logger;

namespace Event
{
    public class AnimationEvents : MonoBehaviour
    {
        public UnityEvent[] animEvent;

        public void AnimEvent(int num)
        {
            if (animEvent != null && num >= 0 && num < animEvent.Length)
            {
                animEvent[num]?.Invoke();
            }
            else
            {
                Logger.Instance.LogWarning($"[AnimationEvents] 잘못된 이벤트 호출. 인덱스: {num}, 오브젝트: {gameObject.name}");
            }
        }
    }
}