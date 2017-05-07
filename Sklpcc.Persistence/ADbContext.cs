#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:57
// Actualizado por ultima vez: 07/05/2017 01:03

#endregion

using System;
using System.Data;
using System.Data.Common;

namespace Sklpcc.Persistence
{
	public abstract class ADbContext
	{
		#region Fields

		protected DbConnection connection;

		protected DbTransaction transaction;

		#endregion

		#region methods

		/// <summary>
		///     Empieza una nueva transaccion
		/// </summary>
		public void BeginTransaction()
		{
			try
			{
				if(this.connection.State == ConnectionState.Closed)
				{
					throw new Exception("Se necesita abrir una conexion antes de iniciar una transaccion.");
				}
				this.transaction = this.connection.BeginTransaction();
			}
			catch(Exception)
			{
				throw;
			}
		}

		/// <summary>
		///     Cierra la conexión
		/// </summary>
		public void Close()
		{
			try
			{
				if(this.transaction != null)
				{
					this.EndTransaction();
				}
			}
			finally
			{
				if(this.connection.State == ConnectionState.Open)
				{
					this.connection.Close();
				}
			}
		}

		/// <summary>
		///     Realiza la confirmacion de la transaccion actual. Llamar a EndTrasaction al finalizar.
		/// </summary>
		public void CommitTransaction()
		{
			this.transaction.Commit();
		}

		/// <summary>
		///     Termina la transaccion
		/// </summary>
		// ReSharper disable once MemberCanBePrivate.Global
		public void EndTransaction()
		{
			this.transaction.Dispose();
			this.transaction = null;
		}

		//Kill me again
		/// <summary>
		///     Retorna el ultimo ID insertado en la tabla
		/// </summary>
		/// <param name="command">Comando que contiene la consulta para la insercion</param>
		/// <param name="pkName">Nombre de la llave primaria de la tabla</param>
		/// <returns>Ultimo ID insertado en la tabla o null</returns>
		public abstract int? GetLastInsertedID(DbCommand command, string pkName = "");

		/// <summary>
		///     Genera un nuevo comando para realizar consultas.
		/// </summary>
		/// <param name="SQLQuery">Consulta a relizar</param>
		/// <param name="isSP">Indica si la consulta es un procedimiento almacenado</param>
		/// <returns>
		///     Comando creado por la conexion
		/// </returns>
		public DbCommand GetNewSQLCommand(string SQLQuery, bool isSP = false)
		{
			if(SQLQuery == null)
			{
				throw new ArgumentNullException(nameof(SQLQuery), "La consulta no puede ser null");
			}
			if(SQLQuery.Length == 0)
			{
				throw new ArgumentException(nameof(SQLQuery), "La consulta debe tener contenido");
			}
			var command = this.connection.CreateCommand();
			if(this.transaction != null)
			{
				command.Transaction = this.transaction;
			}
			command.CommandText = SQLQuery;
			command.CommandType = isSP ? CommandType.StoredProcedure : CommandType.Text;
			return command;
		}

		//Kill me
		/// <summary>
		///     Genera el comando de inserción, agregando la sentencia que
		///     devuelve el ultimo ID creado en la conexión
		/// </summary>
		/// <param name="table">Tabla en la que se insertara el nuevo registro</param>
		/// <param name="fields">Campos cuyos valores estarán presententes en la insercion</param>
		/// <param name="paramValues">Parametros que contendran los valores a insertar</param>
		/// <param name="pkName">Nombre de llave primaria de la tabla</param>
		/// <returns>Comando con la consulta de insercion</returns>
		public abstract DbCommand GetPkCommand(string table, string fields, string paramValues, string pkName = "");

		/// <summary>
		///     Comprueba si la conexion esta abierta
		/// </summary>
		/// <returns>
		///     true: Conexion abierta
		/// </returns>
		public bool IsOpen()
		{
			return this.connection.State == ConnectionState.Open;
		}

		/// <summary>
		///     Comprueba si hay una transaccion activa en la conexion
		/// </summary>
		/// <returns></returns>
		public bool IsTransaction()
		{
			return this.transaction != null;
		}

		/// <summary>
		///     Abre la conexion
		/// </summary>
		public void Open()
		{
			if(this.connection.State == ConnectionState.Closed)
			{
				this.connection.Open();
			}
		}

		/// <summary>
		///     Realiza el revertimiento de los cambios realizados en la transaccion. Llamar a EndTrasaction al finalizar
		/// </summary>
		public void RollbackTransaction()
		{
			this.transaction.Rollback();
		}

		#endregion
	}
}