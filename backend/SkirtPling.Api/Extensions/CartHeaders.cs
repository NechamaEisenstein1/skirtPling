namespace SkirtPling.Api.Extensions;

public static class CartHeaders
{
    /// <summary>Random id the browser generates and sends so guests keep their cart without an account.</summary>
    public const string GuestCartId = "X-Guest-Cart-Id";
}
