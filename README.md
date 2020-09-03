# Rsi.LocalTransactions
Provides an implementation of LocalTransactionScope and DbConnectionScope classes that guarantee a local transaction (doesn't escalate to a distributed transaction) through reusing the same connection in an async code scope

![Build](https://github.com/rsivanov/Rsi.LocalTransactions/workflows/Build%20&%20test%20&%20publish%20Nuget/badge.svg?branch=master)
[![NuGet](https://img.shields.io/nuget/dt/Rsi.LocalTransactions)](https://www.nuget.org/packages/Rsi.LocalTransactions) 
[![NuGet](https://img.shields.io/nuget/v/Rsi.LocalTransactions)](https://www.nuget.org/packages/Rsi.LocalTransactions)

Why?
===
TransactionScope class itself doesn't guarantee that the transaction won't escalate to MSDTC - it depends on database type and version [Avoid unwanted Escalation to Distributed Transactions](https://petermeinl.wordpress.com/2011/03/13/avoiding-unwanted-escalation-to-distributed-transactions/)]. The only way to always get a local transaction is to reuse the same connection instance inside the transaction scope. That's exactly what LocalTransactionScope does.

How to use
===
Just use LocalTransactionScope the same way you would use standard TransactionScope and get a connection only through DbConnectionScope.Current. Below is an example of a test code, in a real application you would need to use SqlClientFactory and SqlConnection instead of mocks.

```csharp
private DbConnection GetOpenConnection()
{
    if (DbConnectionScope.Current == null)
        return new MockDbConnection {ConnectionString =  MockConnectionString};

    return DbConnectionScope.Current.GetOpenConnection(_dbProviderFactory, MockConnectionString);
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
```
