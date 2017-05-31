#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:54
// Actualizado por ultima vez: 31/05/2017 11:17

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Sklpcc.DomainAttributes;

namespace Sklpcc.Persistence
{
	/// <summary>
	///     Clase base para los DAO de las clases de dominio
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public abstract class DAOBase<TEntity> where TEntity : class, new()
	{
		#region Static

		protected static List<ColumnInfo> columns {get; set;}
		protected static string fullSelectString {get; set;}
		protected static string parameters {get; set;}
		protected static ColumnInfo pk {get; set;}

		protected static string selectString {get; set;}
		protected static string tableName {get; set;}
		protected static string updateString {get; set;}

		#endregion

		#region Fields

		protected readonly ADbContext dbContext;
		protected readonly Mapper<TEntity> mapper;

		#endregion

		#region Constructors

		static DAOBase()
		{
			columns = new List<ColumnInfo>();
			var tName = typeof(TEntity).GetCustomAttribute(typeof(Table), true) as Table;
			var className = typeof(TEntity).Name;
			tableName = tName?.Name ?? className.ToLower();

			var entityType = typeof(TEntity);

			var props =
				entityType.GetProperties()
						.Where(
							 c =>
								 c.GetCustomAttribute(typeof(Property), true) == null && c.GetCustomAttribute(typeof(Ignored), true) == null);

			foreach(var propertyInfo in props)
			{
				var cInfo = new ColumnInfo(tableName, propertyInfo);
				if(cInfo.IsPK)
				{
					pk = cInfo;
				}
				else
				{
					columns.Add(cInfo);
				}
			}

			if(pk == null)
			{
				throw new ApplicationException($"La clase ${className} no tiene una llave primaria.");
			}

			var pkName = $"{className}ID";

			if(!pk.NameIsChanged && pk.Name != pkName && !pk.IsPKForced)
			{
				pk.Name = pkName;
			}

			selectString = string.Join(",", columns.Select(c => c.Name));
			fullSelectString = string.Join(",", columns.Select(c => $"{tableName}.{c.Name}"));
			parameters = string.Join(",", columns.Where(c => !c.IsReadOnly).Select(c => "@p_" + c.Name));
			updateString = "set " + parameters;
		}

		public DAOBase(ADbContext dbContext)
		{
			this.dbContext = dbContext;
			this.mapper = new Mapper<TEntity>(pk, columns);
		}

		#endregion

		#region Methods

		/// <summary>
		///     Consulta INSERT
		/// </summary>
		/// <param name="entity">Registro a insertar</param>
		/// <returns>
		///     true: La consulta tuvo exito
		///     false: La consulta fallo
		/// </returns>
		public virtual bool Add(TEntity entity)
		{
			//var sqlQuery = $"insert into {tableName}({selectString}) values ({parameters})";
			using(var command = this.dbContext.GetPkCommand(tableName, selectString, parameters, pk.Name))
			{
				this.AssingParameters(command, entity);
				var id = this.dbContext.GetLastInsertedID(command, pk.Name);
				if(id != null)
				{
					typeof(TEntity).GetProperty(pk.RealName).SetValue(entity, id.Value);
				}
				return id != null;
			}
		}

		//TODO: Añadir todo en VALUES(), verificar sintaxis especificas
		/// <summary>
		///     Consulta INSERT de un grupo de registros
		/// </summary>
		/// <param name="entities">Registros a insertar</param>
		/// <returns>
		///     true: TODOS los registros fueron insertados con exito.
		/// </returns>
		public bool AddRange(IEnumerable<TEntity> entities)
		{
			this.CheckTransaction();
			return entities.All(this.Add);
		}

		/// <summary>
		///     Asigna los valores de los parametros de una consulta
		/// </summary>
		/// <remarks>
		///     Contiene todos los parametros de la entidad
		/// </remarks>
		/// <param name="command">Comando</param>
		/// <param name="entity">Entidad</param>
		/// <param name="addPk">Indica si se agrega la llave primaria</param>
		protected void AssingParameters(IDbCommand command, TEntity entity, bool addPk = false)
		{
			if(addPk)
			{
				var param = command.CreateParameter();
				param.ParameterName = $"@p_{pk.Name}";
				param.Value = typeof(TEntity).GetProperty(pk.Name).GetValue(entity, null);

				command.AddParameterWithValue($"@p_{pk.Name}", typeof(TEntity).GetProperty(pk.Name).GetValue(entity, null));
			}
			foreach(var column in columns)
			{
				command.AddParameterWithValue($"@p_{column.Name}",
											typeof(TEntity).GetProperty(column.RealName).GetValue(entity, null));
			}
		}

		/// <summary>
		///     Asegura que la conexion tenga una trasaccion activa
		/// </summary>
		/// <exception cref="ApplicationException">En caso no cumpla la condicion</exception>
		private void CheckTransaction()
		{
			if(!this.dbContext.IsTransaction())
			{
				throw new ApplicationException("No se puede realizar esta operacion fuera de una transacción.");
			}
		}

		/// <summary>
		///     Consulta SELECT a traves de la llave primaria de la tabla
		/// </summary>
		/// <param name="id">Valor de la llave primaria de la tabla</param>
		/// <param name="eagerLoading">Indica si se deben cargar las relaciones foraneas de la tabla</param>
		/// <returns></returns>
		public virtual TEntity Get(int id, bool eagerLoading = false)
		{
			var sqlQuery = $"select {pk.Name}, {selectString} from {tableName} where {pk.Name} = @p_{pk.Name}";

			using(var command = this.dbContext.GetNewSQLCommand(sqlQuery))
			{
				command.AddParameterWithValue($"@p_{pk.Name}", id);
				using(var reader = command.ExecuteReader())
				{
					var one = this.mapper.MapOne(reader);
					if(eagerLoading)
					{
						this.Load(one);
					}
					return one;
				}
			}
		}

		public virtual IEnumerable<TEntity> GetAll(bool eagerLoading = false)
		{
			var sqlQuery = $"select {pk.Name}, {selectString} from {tableName}";

			using(var command = this.dbContext.GetNewSQLCommand(sqlQuery))
			{
				using(var reader = command.ExecuteReader())
				{
					var many = this.mapper.MapAll(reader);
					if(eagerLoading)
					{
						foreach(var entity in many)
							this.Load(entity);
					}
					return many;
				}
			}
		}

		//TODO: realizar el proceso automatico con el atributo Property
		/// <summary>
		///     Carga las clases realacionadas a la tabla
		/// </summary>
		/// <param name="entity"></param>
		public virtual void Load(TEntity entity)
		{
			if(entity == null)
			{
			}
		}

		/// <summary>
		///     Consulta DELETE
		/// </summary>
		/// <param name="id">Llave primaria del registro a eliminar</param>
		/// <returns>
		///     true: en caso de exito
		/// </returns>
		public virtual bool Remove(int id)
		{
			var sqlQuery = $"delete from {tableName} where {pk.Name} = @p_{pk.Name}";

			using(var command = this.dbContext.GetNewSQLCommand(sqlQuery))
			{
				command.AddParameterWithValue($"@p_{pk.Name}", id);
				return command.ExecuteNonQuery() == 1;
			}
		}

		/// <summary>
		///     Consulta DELETE para varios registros
		/// </summary>
		/// <param name="entities">Registros a eliminar</param>
		/// <returns>
		///     true: en caso de exito
		/// </returns>
		public bool RemoveRange(IEnumerable<TEntity> entities)
		{
			this.CheckTransaction();
			return entities.All(entity => this.Remove((int)typeof(TEntity).GetProperty(pk.RealName).GetValue(entity)));
		}

		/// <summary>
		///     Consulta UPDATE
		/// </summary>
		/// <param name="oldEntity">Entidad antigua a actualizar</param>
		/// <param name="newEntity">Nueva entidad</param>
		/// <returns></returns>
		public virtual bool Update(TEntity oldEntity, TEntity newEntity)
		{
			var sqlQuery = $"update {tableName} ";
			var updatedColumns = new List<ColumnInfo>();
			foreach(var columnInfo in columns)
			{
				var oldVal = typeof(TEntity).GetProperty(columnInfo.RealName).GetValue(oldEntity);
				var newVal = typeof(TEntity).GetProperty(columnInfo.RealName).GetValue(newEntity);
				if(oldVal != newVal)
				{
					updatedColumns.Add(columnInfo);
				}
			}
			if(updatedColumns.Count == 0)
			{
				return true;
			}
			sqlQuery +=
				$"set {string.Join(",", updatedColumns.Select(c => c.Name + " = @p_" + c.Name))} where {pk.Name} = @p_{pk.Name}";
			using(var command = this.dbContext.GetNewSQLCommand(sqlQuery))
			{
				command.AddParameterWithValue($"@p_{pk.Name}", typeof(TEntity).GetProperty(pk.RealName).GetValue(oldEntity));
				foreach(var columnInfo in updatedColumns)
				{
					command.AddParameterWithValue($"@p_{columnInfo.Name}",
												typeof(TEntity).GetProperty(columnInfo.RealName).GetValue(newEntity));
				}
				return command.ExecuteNonQuery() == 1;
			}
		}

		/// <summary>
		///     Consulta UPDATE para varios registros
		/// </summary>
		/// <param name="oldEntities">Entidades antiguas a actualizar</param>
		/// <param name="newEntites">Nuevas entidades</param>
		/// <remarks>
		///     Esta funcion se encarga de separar las entidades, en caso sean nuevas (INSERT), ya no esten (DELETE) o se tengan
		///     que actualizar (UPDATE) basada en la coincidencia de llaves primarias
		/// </remarks>
		/// <returns>
		///     true: en caso de exito
		/// </returns>
		public virtual bool UpdateRange(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntites)
		{
			this.CheckTransaction();
			var _newEntities = new List<TEntity>(newEntites);
			var _oldEntities = new List<TEntity>(oldEntities);
			var toAdd = new List<TEntity>();
			var toUpdateOld = new List<TEntity>();
			var toUpdateNew = new List<TEntity>();
			var toRemove = new List<TEntity>();

			foreach(var oldEntity in _oldEntities)
			{
				var oldKey = typeof(TEntity).GetProperty(pk.RealName).GetValue(oldEntity);
				//si está en la nueva lista, update
				var newEntity = _newEntities.FirstOrDefault(x => typeof(TEntity).GetProperty(pk.RealName).GetValue(x) == oldKey);
				if(newEntity != null)
				{
					toUpdateOld.Add(oldEntity);
					toUpdateNew.Add(newEntity);
					_newEntities.Remove(newEntity);
				}
				else
				{
					toRemove.Add(oldEntity);
				}
			}
			foreach(var newEntity in _newEntities)
			{
				var newKey = typeof(TEntity).GetProperty(pk.RealName).GetValue(newEntity);
				//si está en la nueva lista, update
				var oldEntity = _oldEntities.FirstOrDefault(x => typeof(TEntity).GetProperty(pk.RealName).GetValue(x) == newKey);
				if(oldEntity == null)
				{
					toAdd.Add(newEntity);
				}
			}
			return this.AddRange(toAdd) && this.UpdateRangeCore(toUpdateOld, toUpdateNew) && this.RemoveRange(toRemove);
		}

		/// <summary>
		///     Consulta UPDATE para varios registros
		/// </summary>
		/// <param name="oldEntities">Entidades antiguas a actualizar</param>
		/// <param name="newEntites">Nuevas entidades</param>
		/// <remarks>
		///     Esta funcion NO se encarga de separar las entidades basada en la coincidencia de llaves primarias. Usar solo si se
		///     tiene certeza que ambas colleciones tienen los mismos objetos (con la misma llave primaria, las demas propiedades
		///     pueden cambiar).
		/// </remarks>
		/// <returns>
		///     true: en caso de exito
		/// </returns>
		public virtual bool UpdateRangeCore(List<TEntity> oldEntities, List<TEntity> newEntites)
		{
			if(oldEntities.Count != newEntites.Count)
			{
				throw new ApplicationException("No se pueden actualizar listas de distintos tamaños.");
			}
			this.CheckTransaction();
			return !oldEntities.Where((t, i) => !this.Update(t, newEntites[i])).Any();
		}

		#endregion
	}
}