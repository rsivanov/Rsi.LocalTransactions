using System;
using System.Transactions;

namespace Rsi.LocalTransactions
{
	/// <summary>
	/// A helper class to use instead of TransactionScope to guarantee local transactions (doesn't escalate to a distibuted one) through reusing the same connection instance in an async code scope
	/// </summary>
	public sealed class LocalTransactionScope : IDisposable
	{
		private readonly TransactionScope _transactionScope;
		private readonly DbConnectionScope _connectionScope;

		public LocalTransactionScope() : this(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = Transaction.Current != null ? Transaction.Current.IsolationLevel : IsolationLevel.ReadCommitted })
		{
		}

		public LocalTransactionScope(TransactionScopeOption scopeOption) : this(scopeOption, new TransactionOptions { IsolationLevel = Transaction.Current != null ? Transaction.Current.IsolationLevel : IsolationLevel.ReadCommitted })
		{
			
		}

		public LocalTransactionScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions)
		{
			_transactionScope = new TransactionScope(scopeOption, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);
			_connectionScope = new DbConnectionScope(ToDbConnectionScopeOption(scopeOption));
		}

		/// <summary>
		/// Is it possible to commit the current transaction
		/// </summary>
		public bool IsActive => Transaction.Current.TransactionInformation.Status == TransactionStatus.Active;

		public TransactionScope TransactionScope => _transactionScope;

		public void Complete()
		{
			_transactionScope.Complete();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			_connectionScope.Dispose();
			_transactionScope.Dispose();
		}

		private static DbConnectionScopeOption ToDbConnectionScopeOption(TransactionScopeOption scopeOption)
		{
			if (scopeOption == TransactionScopeOption.Required)
				return DbConnectionScopeOption.Required;
			if (scopeOption == TransactionScopeOption.RequiresNew)
				return DbConnectionScopeOption.RequiresNew;
			return DbConnectionScopeOption.Suppress;
		}
	}
}
