#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:53
// Actualizado por ultima vez: 07/05/2017 01:03

#endregion

using System;

namespace Sklpcc.DomainAttributes
{
	/// <summary>
	///     Atributo usado para generar relaciones entre clases del dominio
	/// </summary>
	/// <remarks>
	///     Aun no tiene mas uso que solo evitar que sea considerado durante el analisis de la clase DAOBase, usar el atributo
	///     Ignored
	/// </remarks>
	public class Property : Attribute
	{
		#region Properties

		/// <summary>
		///     La clase de dominio esta siendo referenciada por otra clase
		/// </summary>
		public bool IsReferenced {get; set;}

		/// <summary>
		///     La clase de dominio referencia a otra clase de dominio
		/// </summary>
		public bool IsReferencing {get; set;}

		#endregion
	}
}