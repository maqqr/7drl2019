using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core
{
    internal enum VisibilityLevel
    {
        Hidden,
        Partial,
        Visible
    }

    internal static class Visibility
    {
        private static readonly Dictionary<VisibilityLevel, Color> visibilityColors = new Dictionary<VisibilityLevel, Color>() {
            { VisibilityLevel.Hidden, new Color(0, 115, 160) },
            { VisibilityLevel.Partial, new Color(4, 180, 255) },
            { VisibilityLevel.Visible, new Color(193, 237, 255) }
        };

        public static Color GetGemColor(VisibilityLevel level)
        {
            if (visibilityColors.ContainsKey(level))
            {
                return visibilityColors[level];
            }
            Debug.LogError("Missing visibility color!");
            return Color.magenta;
        }

        public static VisibilityLevel Calculate(Vector3 position, List<LightSource> lightSources)
        {
            VisibilityLevel visibility = VisibilityLevel.Hidden;

            for (int i = 0; i < lightSources.Count; i++)
            {
                LightSource light = lightSources[i];
                float distanceSq = Vector3.SqrMagnitude(light.gameObject.transform.position - position);
                float shortRadius = light.lightRadius - 1.0f;

                if (distanceSq < light.lightRadius * light.lightRadius)
                {
                    visibility = VisibilityLevel.Partial;
                }

                if (distanceSq < shortRadius * shortRadius)
                {
                    visibility = VisibilityLevel.Visible;
                }
            }

            return visibility;
        }
    }
}