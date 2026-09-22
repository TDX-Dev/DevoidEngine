using DevoidEngine.Nodes;
using System;
using System.Collections.Generic;

namespace Elemental.Tools
{
    public static class NodeIcons
    {
        public static readonly IReadOnlyDictionary<Type, string> NodeIconMapping =
            new Dictionary<Type, string>
            {
                { typeof(Node), LucideIconFont.IconLineDotRightHorizontal },
                { typeof(Node3D), LucideIconFont.IconScale3d },
                { typeof(MeshNode), LucideIconFont.IconBox },
                { typeof(Camera3D), LucideIconFont.IconVideo },
                { typeof(LightNode), LucideIconFont.IconLightbulb },
            };

        public static string GetIconOrDefault(Type type)
        {
            NodeIconMapping.TryGetValue(type, out var icon);
            return icon ?? LucideIconFont.IconLineDotRightHorizontal;
        }
    }
}