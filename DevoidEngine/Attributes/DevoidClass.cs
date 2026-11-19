namespace DevoidEngine.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DevoidClass : Attribute
    {
        public Type? Type;
        public Type? InheritsFrom;

        public DevoidClass()
        {

        }

        public DevoidClass(Type type, Type inheritsFrom)
        {
            this.Type = type;
            this.InheritsFrom = inheritsFrom;
        }

        public DevoidClass(Type type)
        {
            this.Type = type;
        }
    }
}
