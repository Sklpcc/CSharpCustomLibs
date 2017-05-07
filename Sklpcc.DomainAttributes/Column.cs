#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:53
// Actualizado por ultima vez: 07/05/2017 01:02

#endregion

using System;

namespace Sklpcc.DomainAttributes
{
	/// <summary>
	///     Usado para indicar propiedades de la propiedad atada a la columna
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class Column : Attribute
	{
		#region Properties

		//public int? Order {get; set;}
		public string Name {get; set;}

		#endregion

		#region Constructors

		public Column(string name /*, int? order = null*/)
		{
			//this.Order = order;
			this.Name = name.Length == 0 ? null : name;
		}

		#endregion
	}
}