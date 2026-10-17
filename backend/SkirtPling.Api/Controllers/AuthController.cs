using Microsoft.AspNetCore.Mvc;
using SkirtPling.Api.Auth;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAccountService accounts, JwtTokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<AuthResultDto> Register(RegisterDto dto, CancellationToken ct) =>
        tokens.CreateToken(await accounts.RegisterAsync(dto, ct));

    [HttpPost("login")]
    public async Task<AuthResultDto> Login(LoginDto dto, CancellationToken ct) =>
        tokens.CreateToken(await accounts.ValidateCredentialsAsync(dto, ct));
}
