using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Scripting
{
    public static class ScriptStateTransfer
    {
        public static void CopyState(object source, object destination)
        {
            var srcType = source.GetType();
            var dstType = destination.GetType();

            // Copy fields
            foreach (var srcField in srcType.GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic))
            {
                var dstField = dstType.GetField(
                    srcField.Name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                if (dstField == null)
                    continue;

                if (!dstField.FieldType.IsAssignableFrom(srcField.FieldType))
                    continue;

                var value = srcField.GetValue(source);
                dstField.SetValue(destination, value);
            }

            // Copy properties
            foreach (var srcProp in srcType.GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic))
            {
                if (!srcProp.CanRead)
                    continue;

                var dstProp = dstType.GetProperty(
                    srcProp.Name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                if (dstProp == null || !dstProp.CanWrite)
                    continue;

                if (!dstProp.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                    continue;

                var value = srcProp.GetValue(source);
                dstProp.SetValue(destination, value);
            }
        }
    }
}
