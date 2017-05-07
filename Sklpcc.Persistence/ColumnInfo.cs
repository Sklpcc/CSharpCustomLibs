#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:54
// Actualizado por ultima vez: 07/05/2017 01:04

#endregion

using System;
using System.Reflection;
using Sklpcc.DomainAttributes;

namespace Sklpcc.Persistence
{
	/// <summary>
	///     Almacena la informacion de los atributos de dominio
	/// </summary>
	public class ColumnInfo
	{
		#region Properties

		public bool CanBeNull {get; set;}
		public bool IsPK {get; set;}
		public bool IsPKForced {get; set;}
		public int? MaxLength {get; set;}
		public int? MinLength {get; set;}
		public string Name {get; set;}
		public bool NameIsChanged {get; set;}

		public PropertyInfo PropertyInfo {get; set;}
		public string RealName {get; set;}
		public Type Type {get; set;}

		#endregion

		#region Constructors

		public ColumnInfo(string tableName, PropertyInfo prop)
		{
			this.IsPK = false;
			this.Name = null;
			this.MinLength = null;
			this.MaxLength = null;
			this.CanBeNull = false;
			this.NameIsChanged = false;
			this.Type = prop.PropertyType;
			this.RealName = prop.Name;

			if(string.IsNullOrEmpty(tableName))
			{
				throw new ArgumentNullException(nameof(tableName));
			}

			var keyAttr = prop.GetCustomAttribute(typeof(Key), true) as Key;
			this.IsPK = keyAttr != null;
			this.IsPKForced = keyAttr?.Force ?? false;

			var columnAttr = prop.GetCustomAttribute(typeof(Column), true) as Column;
			this.Name = columnAttr?.Name;
			if(this.Name != null)
			{
				this.NameIsChanged = true;
			}

			var maxLengthAttr = prop.GetCustomAttribute(typeof(MaxLength), true) as MaxLength;
			this.MaxLength = maxLengthAttr?.length;

			var minLengthAttr = prop.GetCustomAttribute(typeof(MinLength), true) as MinLength;
			this.MinLength = minLengthAttr?.length;

			if(this.MinLength.HasValue && this.MaxLength.HasValue && this.MaxLength.Value < this.MinLength.Value)
			{
				throw new ArgumentException("Maximum column size cannot be less than minimum column size");
			}

			var Null = prop.GetCustomAttribute(typeof(Null), true) as Null;
			this.CanBeNull = Null != null;

			if(this.Name == null)
			{
				this.Name = prop.Name;
			}
		}

		#endregion
	}
}