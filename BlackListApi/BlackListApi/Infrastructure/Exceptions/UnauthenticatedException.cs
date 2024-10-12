using System.Diagnostics.CodeAnalysis;

namespace BlackListApi.Infrastructure.Exceptions;

[ExcludeFromCodeCoverage]
public class UnauthenticatedException(string message) : Exception(message);