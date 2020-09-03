using System;
using System.Data;
using System.Data.Common;

namespace Rsi.LocalTransactions.Tests
{
	public class MockDbConnection : DbConnection
	{
		private ConnectionState _state;

		public MockDbConnection()
		{
			_state = ConnectionState.Closed;
		}
		
		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			return new MockDbTransaction(this, isolationLevel);
		}

		public override void ChangeDatabase(string databaseName)
		{
			throw new NotImplementedException();
		}

		public override void Close()
		{
			_state = ConnectionState.Closed;
		}

		public override void Open()
		{
			_state = ConnectionState.Open;
		}

		public override string ConnectionString { get; set; }
		public override string Database => String.Empty;
		public override ConnectionState State => _state;
		public override string DataSource => String.Empty;
		public override string ServerVersion => String.Empty;

		protected override DbCommand CreateDbCommand()
		{
			throw new System.NotImplementedException();
		}
	}
}