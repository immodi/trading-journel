namespace Application.Exceptions;

/// <summary>
/// Thrown when a request conflicts with the current state of a resource, for
/// example a duplicate market name or deleting a market that still has trades.
/// Translated to <c>409 Conflict</c> by the global exception handler.
/// </summary>
public sealed class ConflictException(string message) : Exception(message);
