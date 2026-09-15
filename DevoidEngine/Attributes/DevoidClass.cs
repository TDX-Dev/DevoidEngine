namespace DevoidEngine.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DevoidClass : Attribute
    {
        public Type? type;
        public Type? inheritsFrom;

        public DevoidClass()
        {

        }

        public DevoidClass(Type type, Type inheritsFrom)
        {
            this.type = type;
            this.inheritsFrom = inheritsFrom;
        }

        public DevoidClass(Type type)
        {
            this.type = type;
        }
    }
}
