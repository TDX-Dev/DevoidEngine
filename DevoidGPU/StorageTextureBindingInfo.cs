using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public sealed class StorageTextureBindingInfo
    {
        public required string Name { get; init; }

        public required int BindSlot { get; init; }

        public required ShaderStage Stage { get; set; }

        public bool ReadWrite { get; init; }
    }
}
