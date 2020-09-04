using System.Data.Common;

namespace Rsi.LocalTransactions.Tests
{
	public class MockDbConnectionProvider : IDbConnectionProvider
	{
		private readonly MockDbConnection _connection;
		private static readonly MockDbProviderFactory _dbProviderFactory = new MockDbProviderFactory();
		private const string MockConnectionString = "ReallyImportantProductionDatabase";

		public MockDbConnectionProvider()
		{
			if (DbConnectionScope.Current == null)
			{
				_connection = new MockDbConnection {ConnectionString = MockConnectionString};
				_connection.Open();
			}
		}

		public void Dispose()
		{
			_connection?.Dispose();
		}

		public DbConnection Connection => _connection ??
		                                  DbConnectionScope.Current.GetOpenConnection(_dbProviderFactory,
			                                  MockConnectionString);
	}
}