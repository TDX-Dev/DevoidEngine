using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class Transform : Component
    {
        public override string Type => nameof(Transform);

        public Vector3 position = Vector3.Zero;
        public Vector3 rotation = Vector3.Zero;
        public Vector3 scale = Vector3.One;

        
        public override void OnStart()
        {
            
        }

        public override void OnUpdate(float dt)
        {

        }



    }
}
