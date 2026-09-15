namespace DevoidEngine.Metadata
{
    public sealed class ClassInfo
    {
        public ClassAPIType ClassAPIType { get; set; }
        public required Type ClassType { get; set; }
        public required string Name { get; init; }
        public required string AssemblyQualifiedName { get; init; }
        public ClassInfo? BaseClass { get; init; }
        public MethodInfo[] Methods { get; init; } = [];
        public PropertyInfo[] Properties { get; init; } = [];

        public bool IsDerivedFrom(ClassInfo baseClass)
        {
            ClassInfo? current = BaseClass;

            while (current != null)
            {
                if (current == baseClass)
                    return true;

                current = current.BaseClass;
            }

            return false;
        }

        public bool IsClassOrDerivedFrom(ClassInfo baseClass)
        {
            return this == baseClass || IsDerivedFrom(baseClass);
        }
    }
}
