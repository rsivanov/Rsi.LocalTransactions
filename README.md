# Rsi.LocalTransactions
Provides an implementation of LocalTransactionScope and DbConnectionScope classes that guarantee a local transaction (doesn't escalate to a distributed transaction) through reusing the same connection in an async code scope

![Build](https://github.com/rsivanov/Rsi.LocalTransactions/workflows/Build%20&%20test%20&%20publish%20Nuget/badge.svg?branch=master)
[![NuGet](https://img.shields.io/nuget/dt/Rsi.LocalTransactions)](https://www.nuget.org/packages/Rsi.LocalTransactions) 
[![NuGet](https://img.shields.io/nuget/v/Rsi.LocalTransactions)](https://www.nuget.org/packages/Rsi.LocalTransactions)

Why?
===
TransactionScope class itself doesn't guarantee that the transaction won't escalate to MSDTC - it depends on database type and version ([Avoid unwanted Escalation to Distributed Transactions](https://petermeinl.wordpress.com/2011/03/13/avoiding-unwanted-escalation-to-distributed-transactions/)). The only way to always get a local transaction is to reuse the same connection instance inside the transaction scope. That's exactly what LocalTransactionScope does.

How to use
===
Just use LocalTransactionScope the same way you would use standard TransactionScope and get a connection only through DbConnectionScope.Current. Below is an example of a test code, in a real application you would need to use SqlClientFactory and SqlConnection instead of mocks.

```csharp
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
```
