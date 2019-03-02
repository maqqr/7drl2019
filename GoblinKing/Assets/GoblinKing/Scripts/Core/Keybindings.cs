using UnityEngine;

namespace GoblinKing.Core
{
    public class Keybindings
    {
        public KeyCode[] moveForward = { KeyCode.UpArrow, KeyCode.W };
        public KeyCode[] moveBackward = { KeyCode.DownArrow, KeyCode.S };
        public KeyCode[] moveLeft = { KeyCode.LeftArrow, KeyCode.A };
        public KeyCode[] moveRight = { KeyCode.RightArrow, KeyCode.D };

        public KeyCode openPerkTree = KeyCode.P;
    }
}