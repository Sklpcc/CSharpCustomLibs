#region Header
// Creado por: Christian
// Fecha: 31/05/2017 11:08
// Actualizado por ultima vez: 31/05/2017 11:08
#endregion

using System;

namespace Sklpcc.DomainAttributes
{
	/// <summary>
	///		Usado para indicar que la propiedad solo será leída mas no escrita en la base de datos
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ReadOnly : Attribute
	{
		
	}
}