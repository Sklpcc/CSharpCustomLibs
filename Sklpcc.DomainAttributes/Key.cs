#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:53
// Actualizado por ultima vez: 07/05/2017 01:02

#endregion

using System;

namespace Sklpcc.DomainAttributes
{
	/// <summary>
	///     Indica que la propiedad es la llave primaria de la clase de dominio
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class Key : Attribute
	{
		#region Properties

		/// <summary>
		///     Fuerza a usar el nombre del atributo como nombre de llave primaria
		/// </summary>
		public bool Force {get; set;}

		#endregion

		#region Constructors

		public Key()
		{
			this.Force = false;
		}

		#endregion
	}
}