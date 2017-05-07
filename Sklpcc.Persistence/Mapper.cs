#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:54
// Actualizado por ultima vez: 07/05/2017 01:04

#endregion

using System;
using System.Collections.Generic;
using System.Data;

namespace Sklpcc.Persistence
{
	/// <summary>
	///     Realiza el mapeo del resulset devuelto por la consulta o procedimiento almacenado en la clase del dominio
	/// </summary>
	/// <typeparam name="TResult">Clase del dominio</typeparam>
	public class Mapper<TResult> where TResult : class, new()
	{
		#region Properties

		private List<ColumnInfo> columns {get;}
		private ColumnInfo pk {get;}

		#endregion

		#region Constructors

		public Mapper(ColumnInfo pk, List<ColumnInfo> columns)
		{
			this.pk = pk;
			this.columns = columns;
		}

		#endregion

		#region methods

		/// <summary>
		///     Mapea todas las filas retornadas de la base de datos
		/// </summary>
		/// <param name="dataReader"></param>
		/// <returns></returns>
		public IEnumerable<TResult> MapAll(IDataReader dataReader)
		{
			var list = new List<TResult>();
			try
			{
				while(dataReader.Read())
					list.Add(this.MapRow(dataReader));
				return list;
			}
			catch(Exception)
			{
				throw;
			}
			finally
			{
				if(!dataReader.IsClosed)
				{
					dataReader.Close(); //also called in IDispose when 'using'
				}
			}
		}

		/// <summary>
		///     Mapea un solo registro a una clase con comprobacion
		/// </summary>
		/// <param name="dataReader"></param>
		/// <returns>
		///     La clase mapeada
		/// </returns>
		public TResult MapOne(IDataReader dataReader)
		{
			var obj = new TResult();
			var count = 0;
			try
			{
				while(dataReader.Read())
				{
					count++;
					if(count > 1)
					{
						throw new Exception("Mulitple rows were encountered when only one was expected. Mapper.cs");
					}
					obj = this.MapRow(dataReader);
				}
				if(count == 1)
				{
					return obj;
				}
				else
				{
					return null;
				}
			}
			catch(Exception)
			{
				throw;
			}
			finally
			{
				if(!dataReader.IsClosed)
				{
					dataReader.Close(); //also called in IDispose when 'using'
				}
			}
		}

		/// <summary>
		///     Mapea una sola fila a una clase del dominio sin comprobacion
		/// </summary>
		/// <remarks>No usar directamente</remarks>
		/// <param name="dr"></param>
		/// <returns></returns>
		private TResult MapRow(IDataReader dr)
		{
			var entity = new TResult();
			var getMethod = typeof(IDataReader).GetExtensionMethods("get", typeof(string))[0];

			var id = getMethod.MakeGenericMethod(this.pk.Type).Invoke(dr, new object[] {dr, this.pk.Name});

			var prop = typeof(TResult).GetProperty(this.pk.RealName);
			prop.SetValue(entity, id);

			foreach(var t in this.columns)
			{
				var method = getMethod.MakeGenericMethod(t.Type);
				typeof(TResult).GetProperty(t.RealName).SetValue(entity, method.Invoke(dr, new object[] {dr, t.Name}));
			}

			return entity;
		}

		#endregion
	}
}