using System.Diagnostics.CodeAnalysis;

namespace BlackListApi.Infrastructure.Exceptions;

[ExcludeFromCodeCoverage]
public class ForbiddenAccessException(string message) : Exception(message);