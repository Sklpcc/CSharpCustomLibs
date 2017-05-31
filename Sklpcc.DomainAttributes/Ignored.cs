#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:53
// Actualizado por ultima vez: 07/05/2017 01:02

#endregion

using System;

namespace Sklpcc.DomainAttributes
{
	/// <summary>
	///     Usado para indicar que la propiedad debe ser ignorada por la clase DAOBase
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class Ignored : Attribute
	{
	}
}