using System;
using System.Data.Common;

namespace Rsi.LocalTransactions.Tests
{
	public interface IDbConnectionProvider : IDisposable
	{
		DbConnection Connection { get; }
	}
}