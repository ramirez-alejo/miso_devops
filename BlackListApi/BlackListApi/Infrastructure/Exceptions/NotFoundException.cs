using System.Diagnostics.CodeAnalysis;

namespace BlackListApi.Infrastructure.Exceptions;

[ExcludeFromCodeCoverage]
public class NotFoundException(string message) : Exception(message);