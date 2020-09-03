using System.Data.Common;

namespace Rsi.LocalTransactions.Tests
{
	public class MockDbProviderFactory : DbProviderFactory
	{
		public override DbConnection CreateConnection()
		{
			return new MockDbConnection();
		}
	}
}