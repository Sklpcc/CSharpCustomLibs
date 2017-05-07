#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:57
// Actualizado por ultima vez: 07/05/2017 01:05

#endregion

using System.Data.Common;
using MySql.Data.MySqlClient;

namespace Sklpcc.Persistence.DBContexts
{
	public class MySQLContext : ADbContext
	{
		#region Constructors

		public MySQLContext(string database, string server = "localhost", string user = "root", string pass = "")
		{
			this.connection = new MySqlConnection($"server={server};database={database};Uid={user};Pwd={pass}");
		}

		#endregion

		#region methods

		/// <inheritdoc />
		public override int? GetLastInsertedID(DbCommand command, string pkName = "")
		{
			var com = command as MySqlCommand;
			com.ExecuteNonQuery();
			return (int?)com.LastInsertedId;
		}

		/// <inheritdoc />
		public override DbCommand GetPkCommand(string table, string fields, string paramValues, string pkName = "")
		{
			return this.GetNewSQLCommand($"INSERT INTO {table}({fields}) VALUES ({paramValues})");
		}

		#endregion
	}
}