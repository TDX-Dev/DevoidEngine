using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Metadata
{
    public static class ClassDB
    {
        private static readonly Dictionary<Type, ClassInfo> classes = [];

        public static void Register(ClassInfo classInfo)
        {
            classes.Add(classInfo.ClassType, classInfo);
        }

        public static void PrintAllTypes()
        {
            foreach (ClassInfo classInfo in classes.Values)
            {
                Console.WriteLine(classInfo.ClassType);
            }
        }

        public static ClassInfo? GetClass(Type type)
        {
            classes.TryGetValue(type, out ClassInfo? classInfo);
            return classInfo;
        }
        public static IEnumerable<ClassInfo> GetDerivedClasses(Type baseType)
        {
            if (!classes.TryGetValue(baseType, out ClassInfo? baseClass))
                yield break;

            foreach (ClassInfo classInfo in classes.Values)
            {
                if (classInfo.IsDerivedFrom(baseClass))
                    yield return classInfo;
            }
        }
        public static string[] GetClassList()
        {
            string[] classList = new string[classes.Count];
            int i = 0;
            foreach(ClassInfo classInfo in classes.Values)
            {
                classList[i++] = classInfo.Name;
            }
            return classList;
        }
    }
}
