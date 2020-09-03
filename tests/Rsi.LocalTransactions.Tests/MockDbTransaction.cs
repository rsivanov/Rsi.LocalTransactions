using System.Data;
using System.Data.Common;

namespace Rsi.LocalTransactions.Tests
{
	public class MockDbTransaction : DbTransaction
	{
		public MockDbTransaction(MockDbConnection connection, IsolationLevel isolationLevel)
		{
			DbConnection = connection;
			IsolationLevel = isolationLevel;
		}
		
		public override void Commit()
		{
		}

		public override void Rollback()
		{
		}

		protected override DbConnection DbConnection { get; }

		public override IsolationLevel IsolationLevel { get; }
	}
}