using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonDogStudios.UnitySoFunctional.StateMachines.Utils
{
    public static class EditorHelpers
    {
        public static IEnumerable<string> GetFieldsFromInterfaceImplementations(Type @interface)
        {
            var list = new List<string>();
            var assembly = AppDomain.CurrentDomain.GetAssemblies();
            var types = assembly.SelectMany(x => x.DefinedTypes)
                .Where(t => t.ImplementedInterfaces.Contains(@interface))
                .Select(t => t.AsType());
            foreach (var type in types)
            {
                list.AddRange(type.GetFields()
                    .Select(field => field.GetValue(null) as string));
            }

            return list.OrderBy(x => x);
        }
    }
}