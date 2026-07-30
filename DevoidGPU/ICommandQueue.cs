namespace DevoidGPU
{
    public interface ICommandQueue
    {
        void Submit(ICommandList commandList);
    }
}
