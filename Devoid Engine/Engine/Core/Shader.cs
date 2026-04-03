using DevoidEngine.Engine.Rendering;
using DevoidGPU;
using System.Text;

namespace DevoidEngine.Engine.Core
{
    public class Shader
    {
        public IShader vShader;
        public IShader fShader;
        public IShaderProgram shaderProgram;

        public static void FromSource(string vsSource, string fsSource)
        {

        }

        public Shader(string vsfsPath)
        {
            string vsPath = vsfsPath + ".vert.hlsl";
            string fsPath = vsfsPath + ".frag.hlsl";

            string vSource, fSource;

            using (StreamReader sr = new StreamReader(vsPath, Encoding.UTF8))
            {
                vSource = sr.ReadToEnd();
            }

            using (StreamReader sr = new StreamReader(fsPath, Encoding.UTF8))
            {
                fSource = sr.ReadToEnd();
            }

            string shaderSourceDirectory = Path.GetDirectoryName(vsfsPath);


            vShader = Renderer.GraphicsDevice.ShaderFactory.CreateShader(ShaderType.Vertex, vSource, "VSMain", shaderSourceDirectory);
            fShader = Renderer.GraphicsDevice.ShaderFactory.CreateShader(ShaderType.Fragment, fSource, "PSMain", shaderSourceDirectory);

            shaderProgram = Renderer.GraphicsDevice.ShaderFactory.CreateShaderProgram();
            shaderProgram.AttachShader(vShader);
            shaderProgram.AttachShader(fShader);


        }

        public Shader(string vsPath, string fsPath)
        {
            string vSource, fSource;

            using (StreamReader sr = new StreamReader(vsPath, Encoding.UTF8))
            {
                vSource = sr.ReadToEnd();
            }

            using (StreamReader sr = new StreamReader(fsPath, Encoding.UTF8))
            {
                fSource = sr.ReadToEnd();
            }

            vShader = Renderer.GraphicsDevice.ShaderFactory.CreateShader(ShaderType.Vertex, vSource, "VSMain", Path.GetDirectoryName(vsPath));
            fShader = Renderer.GraphicsDevice.ShaderFactory.CreateShader(ShaderType.Fragment, fSource, "PSMain", Path.GetDirectoryName(fsPath));

            shaderProgram = Renderer.GraphicsDevice.ShaderFactory.CreateShaderProgram();
            shaderProgram.AttachShader(vShader);
            shaderProgram.AttachShader(fShader);

        }

        public void LinkShaders()
        {

        }

        public void Use()
        {
            shaderProgram.Bind();
        }


    }
}