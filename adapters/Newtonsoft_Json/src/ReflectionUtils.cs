using System;
using System.Linq;

namespace MagicConfig.Adapters.Newtonsoft_Json
{
	public static class ReflectionUtils
	{
		public static Type FindInTypeHierarchy(Func<Type, bool> predicate, Type childType)
		{
			for (Type t = childType; t != null; t = t.BaseType) {
				if (predicate(t)) {
					return t;
				}
			}
			return null;
		}

		public static object CallMethod(object obj, string name, Type[] typeArgs, object[] args)
		{
			return obj
				.GetType()
				.GetMethods()
				.First(
					m => m.Name == name
					&& m.GetGenericArguments().Length == typeArgs.Length
					&& m.GetParameters().Length == args.Length
				)
				.MakeGenericMethod(typeArgs)
				.Invoke(obj, args);
		}
	}
}
