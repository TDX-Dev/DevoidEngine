using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public sealed class MaterialLayout
    {
        public string BufferName = "";

        public int BufferSize;

        public int BufferBindSlot;

        public Dictionary<string, ShaderVariableInfo> Variables = [];

        public Dictionary<string, TextureBindingInfo> Textures = [];
    }
}
