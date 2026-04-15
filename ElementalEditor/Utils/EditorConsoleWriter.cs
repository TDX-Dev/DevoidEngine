using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Utils
{
    class EditorConsoleWriter : TextWriter
    {
        LogType type;

        public EditorConsoleWriter(LogType type = LogType.Info)
        {
            this.type = type;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            if (type == LogType.Error)
                EditorConsole.Error(value);
            else
                EditorConsole.Log(value);
        }
    }
}
