#region Header

// Creado por: Christian
// Fecha: 07/05/2017 00:57
// Actualizado por ultima vez: 07/05/2017 01:04

#endregion

using System.Data;
using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace Sklpcc.Persistence.DBContexts
{
	public class FbContext : ADbContext
	{
		#region Constructors

		public FbContext(string database, string server = "localhost", string user = "SYSDBA", string pass = "masterkey")
		{
			this.connection =
				new FbConnection(
								 $"User={user};Password={pass};Database={database};DataSource={server};Port=3050;Dialect=3;Charset=NONE;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0");
		}

		#endregion

		#region methods

		/// <inheritdoc />
		public override int? GetLastInsertedID(DbCommand command, string pkName = "")
		{
			var com = command as FbCommand;
			// ReSharper disable once PossibleNullReferenceException
			com.Parameters.Add(pkName, FbDbType.Integer).Direction = ParameterDirection.Output;
			return (int?)com.ExecuteScalar();
		}

		/// <inheritdoc />
		public override DbCommand GetPkCommand(string table, string fields, string paramValues, string pkName = "")
		{
			return this.GetNewSQLCommand($"INSERT INTO {table}({fields}) VALUES ({paramValues}) returning {pkName}");
		}

		#endregion
	}
}