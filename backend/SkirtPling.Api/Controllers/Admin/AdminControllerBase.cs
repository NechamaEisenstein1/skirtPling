using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkirtPling.DAL.Seed;

namespace SkirtPling.Api.Controllers.Admin;

/// <summary>Every admin endpoint inherits this, so none can be added without the Admin role check.</summary>
[ApiController]
[Authorize(Roles = Roles.Admin)]
public abstract class AdminControllerBase : ControllerBase;
