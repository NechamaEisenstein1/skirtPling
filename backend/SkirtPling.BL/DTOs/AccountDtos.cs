using System.ComponentModel.DataAnnotations;

namespace SkirtPling.BL.DTOs;

public record RegisterDto(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(100, MinimumLength = 8)] string Password,
    [Required, StringLength(150, MinimumLength = 2)] string FullName);

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record UserDto(string Id, string Email, string FullName, IReadOnlyList<string> Roles);

public record AuthResultDto(string Token, DateTime ExpiresAt, UserDto User);

public record UpdateProfileDto(
    [Required, StringLength(150, MinimumLength = 2)] string FullName,
    [Phone, StringLength(30)] string? PhoneNumber);

public record ProfileDto(string Id, string Email, string FullName, string? PhoneNumber);

public record AddressDto(int Id, string Label, string FullName, string Phone, string Street, string City, string PostalCode, string Country);

public record AddressUpsertDto(
    [Required, StringLength(50)] string Label,
    [Required, StringLength(150)] string FullName,
    [Required, Phone, StringLength(30)] string Phone,
    [Required, StringLength(200)] string Street,
    [Required, StringLength(100)] string City,
    [Required, StringLength(20)] string PostalCode,
    [Required, StringLength(100)] string Country);
