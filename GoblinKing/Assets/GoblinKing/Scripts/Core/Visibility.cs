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
            { VisibilityLevel.Hidden, new Color(0, 83/255f, 108/255f) },
            { VisibilityLevel.Partial, new Color(4/255f, 150/255f, 200/255f) },
            { VisibilityLevel.Visible, new Color(153/255f, 207/255f, 255/255f) }
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
                float partialRadius = light.lightRadius + 0.8f;

                if (distanceSq < partialRadius * partialRadius)
                {
                    Vector3 raycastDir = position - light.transform.position;
                    if (Physics.Raycast(light.transform.position, raycastDir, raycastDir.magnitude, LayerMask.NameToLayer("Player"), QueryTriggerInteraction.Ignore))
                    {
                        // Something is blocking the light ray
                        return VisibilityLevel.Hidden;
                    }

                    visibility = VisibilityLevel.Partial;

                    if (distanceSq < light.lightRadius * light.lightRadius)
                    {
                        visibility = VisibilityLevel.Visible;
                    }
                }
            }

            return visibility;
        }
    }
}