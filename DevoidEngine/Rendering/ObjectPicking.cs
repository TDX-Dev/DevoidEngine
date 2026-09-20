using System.Numerics;

namespace DevoidEngine.Rendering
{
    public readonly record struct ObjectPickRequest(Vector2 Location);
    public readonly record struct ObjectPickResult(uint ObjectId);
}
