using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public static class SceneManager
    {
        public static Scene CurrentScene;

        public static void LoadScene(Scene scene)
        {
            CurrentScene = scene;
        }

    }
}
