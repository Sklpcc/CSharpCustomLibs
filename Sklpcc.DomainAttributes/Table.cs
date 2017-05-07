#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:53
// Actualizado por ultima vez: 07/05/2017 01:03

#endregion

using System;

namespace Sklpcc.DomainAttributes
{
	/// <summary>
	///     Usado para indicar propiedades de la tabla asociada a la clase de dominio
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class Table : Attribute
	{
		#region Properties

		public string Name {get; set;}

		#endregion
	}
}