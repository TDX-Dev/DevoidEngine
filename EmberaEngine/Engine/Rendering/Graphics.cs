using EmberaEngine.Engine.Core;
using System;
using System.Collections.Generic;

using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Rendering
{
    public static class Graphics
    {

        public static Matrix4 CreateOrthographicCenter(float left, float right, float bottom, float top, float depthNear, float depthFar)
        {
            return Matrix4.CreateOrthographicOffCenter(
                    left,
                    right,
                    bottom,
                    top,
                    depthNear,
                    depthFar
            );
        }

        public static Matrix4 CreateOrthographic2D(float width, float height, float depthNear, float depthFar)
        {
            return Matrix4.CreateOrthographicOffCenter(
                    0,
                    width,
                    0,
                    height,
                    depthNear,
                    depthFar
            );
        }

    }
}
