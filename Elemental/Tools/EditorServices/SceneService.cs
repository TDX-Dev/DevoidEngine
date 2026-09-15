using DevoidEngine.Core;
using System.Numerics;
using System.Text;

namespace Elemental.Tools.EditorServices
{
    public class SceneService : IEditorService
    {
        public void LoadScene()
        {

        }

        public void SaveScene()
        {
            // for starters lets try saving to the root directory of the project.

            Scene testScene = new();

            GameObject go = testScene.AddGameObject("MyObject1");
            GameObject go1 = testScene.AddGameObject("MyObject11");
            GameObject go2 = testScene.AddGameObject("MyObject111");
            GameObject go3 = testScene.AddGameObject("MyObject1111");

            go1.Transform.Position = new Vector3(1, 10, 20);

            ResourceFormatText.Save("scene.dscn", testScene);

        }

    }
}
