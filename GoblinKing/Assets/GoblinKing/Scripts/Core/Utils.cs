using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core
{
    internal static class Utils
    {
        public static int ConvertToGameCoord(float pos)
        {
            return pos < 0 ? ((int)pos) - 1 : (int)pos;
        }

        public static Vector2Int ConvertToGameCoord(Vector3 pos)
        {
            return new Vector2Int(ConvertToGameCoord(pos.x), ConvertToGameCoord(pos.z));
        }

        public static float ConvertToWorldCoord(int pos)
        {
            return pos + 0.5f;
        }

        public static Vector3 ConvertToWorldCoord(Vector2Int pos)
        {
            return new Vector3(ConvertToWorldCoord(pos.x), 0f, ConvertToWorldCoord(pos.y));
        }

        public static bool IsPressed(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        public static bool IsPressed(KeyCode[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKeyDown(keys[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsDown(KeyCode key)
        {
            return Input.GetKey(key);
        }

        public static bool IsDown(KeyCode[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKey(keys[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsReleased(KeyCode key)
        {
            return Input.GetKeyUp(key);
        }

        public static bool IsReleased(KeyCode[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKeyUp(keys[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}