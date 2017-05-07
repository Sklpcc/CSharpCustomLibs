#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:54
// Actualizado por ultima vez: 07/05/2017 01:04

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sklpcc.Persistence
{
	/// <summary>
	///     Extensiones
	/// </summary>
	public static class DAOExtensions
	{
		#region methods

		/// <summary>
		///     HOLA
		/// </summary>
		/// <param name="parameterName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int AddParameterWithValue(this IDbCommand c, string parameterName, object value)
		{
			var param = c.CreateParameter();
			param.ParameterName = parameterName;
			param.Value = value;
			return c.Parameters.Add(param);
		}

		public static T get<T>(this IDataReader source, string columnName)
		{
			var value = source[columnName];

			var t = typeof(T);
			t = Nullable.GetUnderlyingType(t) ?? t;

			return value == null || DBNull.Value.Equals(value) ? default(T) : (T)Convert.ChangeType(value, t);
		}

		public static T get<T>(this IDataReader source, int index)
		{
			var value = source[index];

			var t = typeof(T);
			t = Nullable.GetUnderlyingType(t) ?? t;

			return value == null || DBNull.Value.Equals(value) ? default(T) : (T)Convert.ChangeType(value, t);
		}

		/// <summary>
		///     Extends the System.Type-type to search for a given extended MethodeName.
		/// </summary>
		/// <param name="MethodeName">Name of the Method</param>
		/// <returns>the found Methode or null</returns>
		public static MethodInfo GetExtensionMethod(this Type t, string MethodeName)
		{
			var mi = from methode in t.GetExtensionMethods() where methode.Name == MethodeName select methode;
			if(mi.Count() <= 0)
			{
				return null;
			}
			return mi.First();
		}

		/// <summary>
		///     This Methode extends the System.Type-type to get all extended methods. It searches hereby in all assemblies which
		///     are known by the current AppDomain.
		/// </summary>
		/// <remarks>
		///     Insired by Jon Skeet from his answer on
		///     http://stackoverflow.com/questions/299515/c-sharp-reflection-to-identify-extension-methods
		/// </remarks>
		/// <returns>returns MethodInfo[] with the extended Method</returns>
		public static MethodInfo[] GetExtensionMethods(this Type t)
		{
			var AssTypes = new List<Type>();

			foreach(var item in AppDomain.CurrentDomain.GetAssemblies())
				AssTypes.AddRange(item.GetTypes());

			var query = from type in AssTypes
						where type.IsSealed && !type.IsGenericType && !type.IsNested
						from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
						where method.IsDefined(typeof(ExtensionAttribute), false)
						where method.GetParameters()[0].ParameterType == t
						select method;
			return query.ToArray();
		}

		public static MethodInfo[] GetExtensionMethods(this Type t, string MethodeName, Type argType)
		{
			var AssTypes = new List<Type>();

			foreach(var item in AppDomain.CurrentDomain.GetAssemblies())
				AssTypes.AddRange(item.GetTypes());

			var query = from type in AssTypes
						where type.IsSealed && !type.IsGenericType && !type.IsNested
						from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
						where method.Name == MethodeName
						where method.IsDefined(typeof(ExtensionAttribute), false)
						where method.GetParameters()[0].ParameterType == t
						where method.GetParameters().Length > 2 ? method.GetParameters()[1].ParameterType == argType : true
						select method;
			return query.ToArray();
		}

		public static string getQueryLikeString(this string str)
		{
			if(str != null)
			{
				var str2 = "";
				var partes = str.Split(' ');
				foreach(var parte in partes)
				{
					if(str2 != "")
					{
						str += " ";
					}
					str2 += "%" + parte + "%";
				}
				return str2;
			}
			return null;
		}

		public static bool HasAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute
		{
			var atts = provider.GetCustomAttributes(typeof(T), true);
			return atts.Length > 0;
		}

		#endregion
	}
}