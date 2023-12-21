using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using System.Runtime.ConstrainedExecution;

namespace EmberaEngine.Engine.Core
{

    public class PhysicsManager2D
    {
        public static float PPM = 64f;

        World _world;

        public void Initialize()
        {
            _world = new World();

            _world.Gravity = new nkast.Aether.Physics2D.Common.Vector2(0, -9f);
        }
        // This uses the MKS system
        public Body CreateBox(BodyType bodyType, Vector2 position ,float rotation, float width, float height, float density, Vector2 offset)
        {
            Body box = _world.CreateBody(new nkast.Aether.Physics2D.Common.Vector2(position.X, position.Y), rotation, (nkast.Aether.Physics2D.Dynamics.BodyType)bodyType);

            box.CreateRectangle(width, height, density, new nkast.Aether.Physics2D.Common.Vector2(offset.X, offset.Y));
            
            return box;
        }

        public void Update(float dt)
        {
            _world.Step(dt);
        }

    }

    public enum BodyType
    {
        Static,
        Kinematic,
        Dynamic
    }
}
