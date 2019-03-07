namespace GoblinKing.Core.Interaction
{
    public interface IInteraction
    {
        // Returns true if interaction should advance game time
        bool Interact(GameManager gameManager);
    }
}