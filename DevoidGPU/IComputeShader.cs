﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public interface IComputeShader
    {
        string Name { get; }
        public void Use();
        public void Dispatch(int x, int y, int z);
        public void Wait();
        public void Compile(string source, string entryPoint);
        public void Dispose();

    }
}
