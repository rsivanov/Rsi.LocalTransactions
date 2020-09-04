using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace Rsi.LocalTransactions.Tests
{
	public class LocalTransactionScopeTests
	{
		[Fact]
		public async Task GetOpenConnection_WithoutTransactionScope_AlwaysReturnsNewInstance()
		{
			using var connectionProvider1 = new MockDbConnectionProvider();
			var connection1 = connectionProvider1.Connection;
			await Task.Delay(0);
			using var connectionProvider2 = new MockDbConnectionProvider();
			var connection2 = connectionProvider2.Connection;
			
			Assert.NotSame(connection1, connection2);
		}

		[Fact]
		public async Task GetOpenConnection_InsideRequiredTransactionScope_ReturnsTheSameInstance()
		{
			using var transactionScope = new LocalTransactionScope();
			
			using var connectionProvider1 = new MockDbConnectionProvider();
			var connection1 = connectionProvider1.Connection;
			await Task.Delay(0);
			using var connectionProvider2 = new MockDbConnectionProvider();
			var connection2 = connectionProvider2.Connection;
			
			Assert.Same(connection1, connection2);
			
			transactionScope.Complete();
		}
		
		[Fact]
		public async Task GetOpenConnection_InsideRequiresNewTransactionScope_ReturnsTheSameInstance()
		{
			using var transactionScope = new LocalTransactionScope(TransactionScopeOption.RequiresNew);
			
			using var connectionProvider1 = new MockDbConnectionProvider();
			var connection1 = connectionProvider1.Connection;
			await Task.Delay(0);
			using var connectionProvider2 = new MockDbConnectionProvider();
			var connection2 = connectionProvider2.Connection;
			
			Assert.Same(connection1, connection2);
			
			transactionScope.Complete();
		}
		
		[Fact]
		public async Task GetOpenConnection_InsideSuppressTransactionScope_AlwaysReturnsNewInstance()
		{
			using var transactionScope = new LocalTransactionScope(TransactionScopeOption.Suppress);
			
			using var connectionProvider1 = new MockDbConnectionProvider();
			var connection1 = connectionProvider1.Connection;
			await Task.Delay(0);
			using var connectionProvider2 = new MockDbConnectionProvider();
			var connection2 = connectionProvider2.Connection;
			
			Assert.NotSame(connection1, connection2);
			
			transactionScope.Complete();
		}
		
		[Fact]
		public async Task GetOpenConnection_InsideNestedRequiredTransactionScope_ReturnsTheSameInstance()
		{
			using (var transactionScope = new LocalTransactionScope())
			{
				using var connectionProvider1 = new MockDbConnectionProvider();
				var connection1 = connectionProvider1.Connection;
				using (var nestedTransactionScope = new LocalTransactionScope())
				{
					using var connectionProvider2 = new MockDbConnectionProvider();
					var connection2 = connectionProvider2.Connection;
					await Task.Delay(0);
					using var connectionProvider3 = new MockDbConnectionProvider();
					var connection3 = connectionProvider3.Connection;
					
					Assert.Same(connection2, connection3);
					Assert.Same(connection1, connection2);
					
					nestedTransactionScope.Complete();
				}
				
				using var connectionProvider4 = new MockDbConnectionProvider();
				var connection4 = connectionProvider4.Connection;
				
				Assert.Same(connection1, connection4);				

				transactionScope.Complete();
			}
		}
		
		[Fact]
		public async Task GetOpenConnection_InsideNestedRequiresNewTransactionScope_ReturnsTheSameInstanceInNestedScope()
		{
			using (var transactionScope = new LocalTransactionScope())
			{
				using var connectionProvider1 = new MockDbConnectionProvider();
				var connection1 = connectionProvider1.Connection;
				using (var nestedTransactionScope = new LocalTransactionScope(TransactionScopeOption.RequiresNew))
				{
					using var connectionProvider2 = new MockDbConnectionProvider();
					var connection2 = connectionProvider2.Connection;
					await Task.Delay(0);
					using var connectionProvider3 = new MockDbConnectionProvider();
					var connection3 = connectionProvider3.Connection;
					
					Assert.Same(connection2, connection3);
					Assert.NotSame(connection1, connection2);
					
					nestedTransactionScope.Complete();
				}
				
				using var connectionProvider4 = new MockDbConnectionProvider();
				var connection4 = connectionProvider4.Connection;
				
				Assert.Same(connection1, connection4);				

				transactionScope.Complete();
			}
		}
		
		[Fact]
		public async Task GetOpenConnection_InsideNestedSuppressTransactionScope_AlwaysReturnsNewInstanceInNestedScope()
		{
			using (var transactionScope = new LocalTransactionScope())
			{
				using var connectionProvider1 = new MockDbConnectionProvider();
				var connection1 = connectionProvider1.Connection;
				using (var nestedTransactionScope = new LocalTransactionScope(TransactionScopeOption.Suppress))
				{
					using var connectionProvider2 = new MockDbConnectionProvider();
					var connection2 = connectionProvider2.Connection;
					await Task.Delay(0);
					using var connectionProvider3 = new MockDbConnectionProvider();
					var connection3 = connectionProvider3.Connection;
					
					Assert.NotSame(connection2, connection3);
					Assert.NotSame(connection1, connection2);
					
					nestedTransactionScope.Complete();
				}
				
				using var connectionProvider4 = new MockDbConnectionProvider();
				var connection4 = connectionProvider4.Connection;
				
				Assert.Same(connection1, connection4);				

				transactionScope.Complete();
			}
		}
	}
}