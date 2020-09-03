# Rsi.LocalTransactions
Provides an implementation of LocalTransactionScope and DbConnectionScope classes that guarantee a local transaction (doesn't escalate to a distributed transaction) through reusing the same connection in an async code scope
