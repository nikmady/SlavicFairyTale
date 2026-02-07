namespace Game.Bootstrap.States
{
    public interface IGameState
    {
        string StateId { get; }
        void Enter();
        void Exit();
        void Tick(float deltaTime);
    }
}
