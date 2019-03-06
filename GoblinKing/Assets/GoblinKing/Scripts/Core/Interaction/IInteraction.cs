namespace GoblinKing.Core.Interaction
{
    internal interface IInteraction
    {
        // Returns true if interaction should advance game time
        bool Interact(GameManager gameManager);
    }
}