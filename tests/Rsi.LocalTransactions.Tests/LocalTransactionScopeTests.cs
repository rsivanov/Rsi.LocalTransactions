using System.Data.Common;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace Rsi.LocalTransactions.Tests
{
	public class LocalTransactionScopeTests
	{
		private static readonly MockDbProviderFactory _dbProviderFactory = new MockDbProviderFactory();

		private const string MockConnectionString = "ReallyImportantProductionDatabase";

		private DbConnection GetOpenConnection()
		{
			if (DbConnectionScope.Current == null)
				return new MockDbConnection {ConnectionString =  MockConnectionString};

			return DbConnectionScope.Current.GetOpenConnection(_dbProviderFactory, MockConnectionString);
		}
		
		[Fact]
		public async Task GetOpenConnection_WithoutTransactionScope_AlwaysReturnsNewInstance()
		{
			var connection1 = GetOpenConnection();
			await Task.Delay(0);
			var connection2 = GetOpenConnection();
			Assert.NotSame(connection1, connection2);
		}

		[Fact]
		public async Task GetOpenConnection_InsideRequiredTransactionScope_ReturnsTheSameInstance()
		{
			using var transactionScope = new LocalTransactionScope();
			var connection1 = GetOpenConnection();
			await Task.Delay(0);
			var connection2 = GetOpenConnection();
			Assert.Same(connection1, connection2);
			transactionScope.Complete();
		}
		
		[Fact]
		public async Task GetOpenConnection_InsideRequiresNewTransactionScope_ReturnsTheSameInstance()
		{
			using var transactionScope = new LocalTransactionScope(TransactionScopeOption.RequiresNew);
			var connection1 = GetOpenConnection();
			await Task.Delay(0);
			var connection2 = GetOpenConnection();
			Assert.Same(connection1, connection2);
			transactionScope.Complete();
		}
		
		[Fact]
		public async Task GetOpenConnection_InsideSuppressTransactionScope_AlwaysReturnsNewInstance()
		{
			using var transactionScope = new LocalTransactionScope(TransactionScopeOption.Suppress);
			var connection1 = GetOpenConnection();
			await Task.Delay(0);
			var connection2 = GetOpenConnection();
			Assert.NotSame(connection1, connection2);
			transactionScope.Complete();
		}
		
		[Fact]
		public async Task GetOpenConnection_InsideNestedRequiredTransactionScope_ReturnsTheSameInstance()
		{
			using (var transactionScope = new LocalTransactionScope())
			{
				var connection1 = GetOpenConnection();
				using (var nestedTransactionScope = new LocalTransactionScope())
				{
					var connection2 = GetOpenConnection();
					await Task.Delay(0);
					var connection3 = GetOpenConnection();
					Assert.Same(connection2, connection3);
					Assert.Same(connection1, connection2);
					nestedTransactionScope.Complete();
				}
				
				var connection4 = GetOpenConnection();
				Assert.Same(connection1, connection4);				

				transactionScope.Complete();
			}
		}
		
		[Fact]
		public async Task GetOpenConnection_InsideNestedRequiresNewTransactionScope_ReturnsTheSameInstanceInNestedScope()
		{
			using (var transactionScope = new LocalTransactionScope())
			{
				var connection1 = GetOpenConnection();
				using (var nestedTransactionScope = new LocalTransactionScope(TransactionScopeOption.RequiresNew))
				{
					var connection2 = GetOpenConnection();
					await Task.Delay(0);
					var connection3 = GetOpenConnection();
					Assert.Same(connection2, connection3);
					Assert.NotSame(connection1, connection2);
					nestedTransactionScope.Complete();
				}
				
				var connection4 = GetOpenConnection();
				Assert.Same(connection1, connection4);				

				transactionScope.Complete();
			}
		}
		
		[Fact]
		public async Task GetOpenConnection_InsideNestedSuppressTransactionScope_AlwaysReturnsNewInstanceInNestedScope()
		{
			using (var transactionScope = new LocalTransactionScope())
			{
				var connection1 = GetOpenConnection();
				using (var nestedTransactionScope = new LocalTransactionScope(TransactionScopeOption.Suppress))
				{
					var connection2 = GetOpenConnection();
					await Task.Delay(0);
					var connection3 = GetOpenConnection();
					Assert.NotSame(connection2, connection3);
					Assert.NotSame(connection1, connection2);
					nestedTransactionScope.Complete();
				}
				
				var connection4 = GetOpenConnection();
				Assert.Same(connection1, connection4);				

				transactionScope.Complete();
			}
		}
	}
}