using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Globalization;
using System.Reflection;

namespace Launcher.Any
{
    public class TranslateKeyScanner
    {
        public static string[] CollectKeys(params Assembly[] assembliesToScan)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var asm in assembliesToScan)
            {
                using var module = ModuleDefinition.ReadModule(asm.Location, new ReaderParameters { ReadSymbols = false });
                var methods = module.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody).ToList();

                foreach (var method in methods)
                {
                    var il = method.Body.Instructions;
                    for (int i = 0; i < il.Count - 1; i++)
                    {
                        if (il[i].OpCode == OpCodes.Ldstr &&
                            il[i + 1].OpCode == OpCodes.Call &&
                            il[i + 1].Operand is MethodReference mr &&
                            mr.Name == "Translate" &&
                            mr.DeclaringType.Name == "Dictionary")
                        {
                            var literal = (string)il[i].Operand;
                            keys.Add(literal);
                        }
                    }
                }
            }

            var ruCulture = new CultureInfo("ru-RU");
            var sorted = keys.OrderBy(s => s,
                         StringComparer.Create(ruCulture, ignoreCase: false))
                             .ToArray();

            return sorted;
        }
    }
}
