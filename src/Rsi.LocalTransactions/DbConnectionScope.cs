using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Transactions;

namespace Rsi.LocalTransactions
{
	/// <summary>
	/// A helper class to assist in managing connection lifetimes inside scopes on a particular async context scope.
	/// </summary>
	public sealed class DbConnectionScope : IDisposable
	{
		private static readonly AsyncLocal<DbConnectionScope> _currentScope = new AsyncLocal<DbConnectionScope>();

		private readonly DbConnectionScope _priorScope;

		private Dictionary<string, DbConnection> _connections;

		private bool _disposed;

		/// <summary>
		/// Obtain the currently active connection scope
		/// </summary>
		public static DbConnectionScope Current => _currentScope.Value;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public DbConnectionScope() : this(DbConnectionScopeOption.Required)
		{
		}

		/// <summary>
		/// Constructor with options
		/// </summary>
		/// <param name="option"></param>
		public DbConnectionScope(DbConnectionScopeOption option)
		{
			if (option == DbConnectionScopeOption.Suppress)
			{
				_priorScope = _currentScope.Value;
				_currentScope.Value = null;
			}
			else if (option == DbConnectionScopeOption.RequiresNew ||
				(option == DbConnectionScopeOption.Required && _currentScope.Value == null))
			{
				_connections = new Dictionary<string, DbConnection>();
				_priorScope = _currentScope.Value;
				_currentScope.Value = this;
			}
			_disposed = false;
		}

		/// <summary>
		/// Shut down this instance.  
		/// Disposes all connections it holds and restores the prior scope.
		/// </summary>
		public void Dispose()
		{
			if (_disposed)
				return;

			if (Current == null || Current == this)
			{
				DbConnectionScope prior = _priorScope;
				while (prior != null && prior._disposed)
				{
					prior = prior._priorScope;
				}

				_currentScope.Value = prior;
			}

			if (_connections != null)
			{
				foreach (var connection in _connections.Values)
				{
					connection.Dispose();
				}
			}
			_disposed = true;
		}

		/// <summary>
		/// This method gets the connection using the connection string as a key.  If no connection is
		/// associated with the string, the connection factory is used to create the connection.
		/// Finally, if the resulting connection is in the closed state, it is opened.
		/// </summary>
		/// <param name="factory">Factory to use to create connection if it is not already present</param>
		/// <param name="connectionString">Connection string to use</param>
		/// <returns>Connection in open state</returns>
		public DbConnection GetOpenConnection(DbProviderFactory factory, string connectionString)
		{
			if (_disposed)
				throw new ObjectDisposedException("DbConnectionScope");

			DbConnection result;

			if (Current == null)
			{
				result = factory.CreateConnection();
			}
			else if (!TryGetConnection(connectionString, out result))
			{
				// didn't find it, so create it.
				result = factory.CreateConnection();
				_connections[connectionString] = result;
			}

			if (string.IsNullOrEmpty(result.ConnectionString))
			{
				result.ConnectionString = connectionString;
			}

			// however we got it, open it if it's closed.
			if (result.State == ConnectionState.Closed)
			{
				result.Open();
			}

			return result;
		}

		/// <summary>
		/// Get the connection associated with this key.
		/// </summary>
		/// <param name="connectionString">Key to use for lookup</param>
		/// <param name="connection">Associated connection</param>
		/// <returns>True if connection found, false otherwise</returns>
		private bool TryGetConnection(string connectionString, out DbConnection connection)
		{
			bool found = _connections.TryGetValue(connectionString, out connection);
			if (found &&
				(Transaction.Current == null || Transaction.Current.TransactionInformation.Status != TransactionStatus.Active))
			{
				_connections.Remove(connectionString);
				connection.Dispose();
				connection = null;
				found = false;
			}

			return found;
		}
	}
}