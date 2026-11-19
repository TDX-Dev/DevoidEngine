using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Metadata
{
    public static class ClassDb
    {
        private static readonly Dictionary<Type, ClassInfo> Classes = [];

        public static void Register(ClassInfo classInfo)
        {
            Classes.Add(classInfo.ClassType, classInfo);
        }

        public static void PrintAllTypes()
        {
            foreach (ClassInfo classInfo in Classes.Values)
            {
                Console.WriteLine(classInfo.ClassType);
            }
        }

        public static ClassInfo? GetClass(Type type)
        {
            Classes.TryGetValue(type, out ClassInfo? classInfo);
            return classInfo;
        }
        public static IEnumerable<ClassInfo> GetDerivedClasses(Type baseType)
        {
            if (!Classes.TryGetValue(baseType, out ClassInfo? baseClass))
                yield break;

            foreach (ClassInfo classInfo in Classes.Values)
            {
                if (classInfo.IsDerivedFrom(baseClass))
                    yield return classInfo;
            }
        }
        public static string[] GetClassList()
        {
            string[] classList = new string[Classes.Count];
            int i = 0;
            foreach(ClassInfo classInfo in Classes.Values)
            {
                classList[i++] = classInfo.Name;
            }
            return classList;
        }
    }
}
