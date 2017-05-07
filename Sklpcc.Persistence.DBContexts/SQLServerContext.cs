#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:57
// Actualizado por ultima vez: 07/05/2017 01:05

#endregion

using System.Data.Common;
using System.Data.SqlClient;

namespace Sklpcc.Persistence.DBContexts
{
	public class SQLServerContext : ADbContext
	{
		#region Constructors

		public SQLServerContext(string database, string server = ".", string user = "", string pass = "")
		{
			this.connection = new SqlConnection($"Data source={server}; Initial Catalog={database};Integrated Security=true");
		}

		#endregion

		#region methods

		/// <inheritdoc />
		public override int? GetLastInsertedID(DbCommand command, string pkName = "")
		{
			var com = command as SqlCommand;
			return (int?)com.ExecuteScalar();
		}

		/// <inheritdoc />
		public override DbCommand GetPkCommand(string table, string fields, string paramValues, string pkName = "")
		{
			return this.GetNewSQLCommand($"INSERT INTO {table}({fields}) OUTPUT INSERTED.{pkName} VALUES ({paramValues})");
		}

		#endregion
	}
}