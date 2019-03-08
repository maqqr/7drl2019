using UnityEngine;

namespace GoblinKing.Core
{
    public class Keybindings
    {
        public KeyCode[] MoveForward = { KeyCode.UpArrow, KeyCode.W };
        public KeyCode[] MoveBackward = { KeyCode.DownArrow, KeyCode.S };
        public KeyCode[] MoveLeft = { KeyCode.LeftArrow, KeyCode.A };
        public KeyCode[] MoveRight = { KeyCode.RightArrow, KeyCode.D };

        public KeyCode[] PeekLeft = { KeyCode.RightArrow, KeyCode.Q };
        public KeyCode[] PeekRight = { KeyCode.RightArrow, KeyCode.E };

        public KeyCode[] PickUp = { KeyCode.Space };
        public KeyCode[] Wait = {KeyCode.LeftShift, KeyCode.Z};

        public KeyCode OpenPerkTree = KeyCode.P;
        public KeyCode Push = KeyCode.F;
        public KeyCode OpenInventory = KeyCode.I;
        public KeyCode ThrowLeftHand = KeyCode.Mouse0;
        public KeyCode ThrowRightHand = KeyCode.Mouse1;

        // Inventory view keys:
        public KeyCode DropItem = KeyCode.D;
        public KeyCode ConsumeItem = KeyCode.E;
    }
}