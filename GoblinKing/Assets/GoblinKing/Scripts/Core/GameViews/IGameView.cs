namespace GoblinKing.Core.GameViews
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