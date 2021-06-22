using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace TestingThingy2
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        static MethodInfo LoadPlugInFromFile(string fileName)
        {
            var asm = Assembly.LoadFrom(fileName);
            var type = asm.GetType("Plugin");
            if (type == null)
            {
                return null;
            }
            return type.GetRuntimeMethods().FirstOrDefault(Method => Method.Name == "PluginStartMethod");
        }
        public static void Main()
        {
            var PluginsDirectory = Path.Combine(new[]{
                Directory.GetCurrentDirectory(),
                "plugins",
                "csharp"
            });
            var plugins = Directory.GetFiles(PluginsDirectory, "*.dll");
            foreach (var plugin in plugins)
            {
                var plugIn = LoadPlugInFromFile(plugin);
                if (plugIn != null)
                {
                    plugIn.Invoke(null, null);
                }
            }
        }
    }
}
