using UnityEngine;

namespace GoblinKing.Core
{
    public class Keybindings
    {
        public KeyCode[] MoveForward = { KeyCode.UpArrow, KeyCode.W };
        public KeyCode[] MoveBackward = { KeyCode.DownArrow, KeyCode.S };
        public KeyCode[] MoveLeft = { KeyCode.LeftArrow, KeyCode.A };
        public KeyCode[] MoveRight = { KeyCode.RightArrow, KeyCode.D };

        public KeyCode[] PickUp = { KeyCode.Space, KeyCode.Mouse0 };

        public KeyCode OpenPerkTree = KeyCode.P;
        public KeyCode OpenInventory = KeyCode.I;

        // Inventory view keys:
        public KeyCode DropItem = KeyCode.D;
    }
}