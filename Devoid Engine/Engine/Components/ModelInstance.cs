using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Components
{
    public class ModelInstance : Component
    {
        public override string Type => nameof(ModelInstance);
        public Guid ModelGuid;

        public override void OnStart()
        {
            Rebuild();
        }

        public void Rebuild()
        {
            if (ModelGuid == Guid.Empty)
                return;
            var model = AssetManager.Load<Model>(ModelGuid);
            if (model == null)
                return;

            model.Instantiate(gameObject.Scene);
        }
    }
}
