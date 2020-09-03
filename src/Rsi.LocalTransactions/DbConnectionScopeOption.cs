namespace Rsi.LocalTransactions
{
	/// <summary>
	/// Defines how DbConnectionScope.Current is affected while constructing a new scope.
	/// </summary>
	public enum DbConnectionScopeOption
	{
		/// <summary>
		/// Sets self as currentScope if there isn't one already available in the current async context, otherwise doesn't do anything.
		/// </summary>
		Required,

		/// <summary>
		/// Pushes self as currentScope (tracks prior scope and restores it on dispose).
		/// </summary>
		RequiresNew,

		/// <summary>
		/// Pushes null reference as currentScope (tracks prior scope and restores it on dispose).
		/// </summary>
		Suppress
	}
}