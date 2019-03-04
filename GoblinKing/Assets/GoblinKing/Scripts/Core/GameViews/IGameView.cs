namespace GoblinKing.Core.GameStates
{
    internal interface IGameView
    {
        void Initialize(GameManager gameManager);
        void Destroy();
        void OpenView();
        void CloseView();
        bool UpdateView();
    }
}