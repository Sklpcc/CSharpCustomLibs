#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:53
// Actualizado por ultima vez: 07/05/2017 01:03

#endregion

using System;

namespace Sklpcc.DomainAttributes
{
	/// <summary>
	///     Indica si una propiedad puede ser nula
	/// </summary>
	/// <remarks>
	///     Sin uso actual
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property)]
	public class Null : Attribute
	{
	}
}