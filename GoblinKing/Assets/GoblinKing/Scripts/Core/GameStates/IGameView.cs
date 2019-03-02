namespace GoblinKing.Core.GameStates
{
    internal interface IGameView
    {
        void Initialize(GameManager gameManager);
        void CloseView();
        bool UpdateView();
    }
}