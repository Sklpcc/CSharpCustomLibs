#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:53
// Actualizado por ultima vez: 07/05/2017 01:02

#endregion

using System;

namespace Sklpcc.DomainAttributes
{
	/// <summary>
	///     Indica el menor tamaño que puede tener la propiedad
	/// </summary>
	/// <remarks>
	///     Sin uso actual
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property)]
	public class MinLength : Attribute
	{
		#region Properties

		public int length {get; set;}

		#endregion

		#region Constructors

		public MinLength(int length)
		{
			if(length < 0)
			{
				throw new ArgumentException("El valor del tamaño minimo debe ser mayor igual a 0");
			}
			this.length = length;
		}

		#endregion
	}
}