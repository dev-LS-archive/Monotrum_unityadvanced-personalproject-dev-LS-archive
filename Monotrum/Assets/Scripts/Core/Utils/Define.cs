namespace Core
{
    public static class Define
    {
        // 씬 이름 관리 (SceneManager.LoadScene 할 때 오타 방지)
        public static class Scene
        {
            public const string Title = "Title_Mono";
            public const string Game = "Game_AudioCave";
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
    }
    
    public enum SceneType
    {
        Title,
        Game
    }
}