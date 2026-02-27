namespace Core
{
    public static class Define
    {
        // 씬 이름 관리 (SceneManager.LoadScene 할 때 오타 방지)
        public static class Scene
        {
            public const string Title = "TitleScene";
            public const string Game = "GameScene"; // 혹은 MainScene
            public const string Lobby = "LobbyScene";
        }
        
        // 애니메이터 파라미터 이름 (SetBool/SetFloat 할 때)
        public static class Anim
        {
            public const string Speed = "Speed";
            public const string IsJump = "Jump";
            public const string IsGrounded = "Grounded";
            public const string IsFalling = "FreeFall";
            public const string MotionSpeed = "MotionSpeed";
        }

        public static class Music
        {
            public const string Title = "Neon nights - Patrick Patrikios";
        }
    }

    // 전역에서 사용할 Enum (게임 상태 등)
    // 클래스 밖에 선언해서 Define.GameState가 아니라 그냥 GameState로 쓰게 함
    public enum GameState
    {
        None,       // 초기화 전
        Title,      // 타이틀 화면
        Playing,    // 게임 플레이 중
        Pause,      // 일시 정지
        Clear       // 엔딩
    }
    
    public enum PlayerState
    {
        Idle,
        Move,
        Jump
    }
}