namespace SkirtPling.BL.Exceptions;

/// <summary>Base type for expected business failures. The API maps each subtype to an HTTP status code.</summary>
public abstract class AppException(string message) : Exception(message);

/// <summary>The resource does not exist, or the caller is not allowed to know it exists.</summary>
public class NotFoundException(string message) : AppException(message);

/// <summary>The request is well-formed but breaks a business rule (e.g. empty cart, invalid quantity).</summary>
public class BusinessRuleException(string message) : AppException(message);

/// <summary>The request conflicts with the current state (e.g. duplicate SKU, stock ran out).</summary>
public class ConflictException(string message) : AppException(message);

/// <summary>A system the request depends on (e.g. the legacy warehouse) is unavailable.</summary>
public class ServiceUnavailableException(string message) : AppException(message);
