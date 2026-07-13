using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OpenKondominio.Api.Modulos.Auth.Application.Services;

namespace OpenKondominio.Api.Modulos.Auth.Infrastructure.Security;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly RbacService _rbacService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionAuthorizationHandler(RbacService rbacService, IHttpContextAccessor httpContextAccessor)
    {
        _rbacService = rbacService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null) { context.Fail(); return; }

        var userIdClaim = context.User.FindFirst("sub");
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        { context.Fail(); return; }

        var condominioIdStr = httpContext.Request.Headers["X-Condominio-Id"].FirstOrDefault();
        if (!Guid.TryParse(condominioIdStr, out var condominioId))
        { context.Fail(); return; }

        var permissions = await _rbacService.GetPermissionsAsync(userId, condominioId);

        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
        else
            context.Fail();
    }
}
