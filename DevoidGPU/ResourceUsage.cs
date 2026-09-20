namespace DevoidGPU
{
    public enum ResourceUsage
    {
        //
        // Summary:
        //     A resource that requires read and write access by the GPU. This is likely to
        //     be the most common usage choice.
        Default,
        //
        // Summary:
        //     A resource that can only be read by the GPU. It cannot be written by the GPU,
        //     and cannot be accessed at all by the CPU. This type of resource must be initialized
        //     when it is created, since it cannot be changed after creation.
        Immutable,
        //
        // Summary:
        //     A resource that is accessible by both the GPU (read only) and the CPU (write
        //     only). A dynamic resource is a good choice for a resource that will be updated
        //     by the CPU at least once per frame. To update a dynamic resource, use a Map method.
        //     For info about how to use dynamic resources, see How to: Use dynamic resources.
        Dynamic,
        //
        // Summary:
        //     A resource that supports data transfer (copy) from the GPU to the CPU.
        Staging
    }
}
