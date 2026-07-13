using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using OpenKondominio.Api.Modulos.Auth.Application.Services;
using OpenKondominio.Api.Modulos.Auth.Domain.Interfaces;
using OpenKondominio.Api.Modulos.Auth.Infrastructure.Security;
using Microsoft.Extensions.Caching.Memory;

namespace OpenKondominio.Tests.Auth.Infrastructure;

public class PermissionHandlerTests
{
    private readonly IRoleRepository _roleRepo = Substitute.For<IRoleRepository>();
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    private (PermissionAuthorizationHandler handler, DefaultHttpContext http) CriarHandler(
        Guid? userId = null, Guid? condominioId = null)
    {
        var http = new DefaultHttpContext();

        if (userId.HasValue)
        {
            http.User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim("sub", userId.Value.ToString()) }, "test"));
        }

        if (condominioId.HasValue)
            http.Request.Headers["X-Condominio-Id"] = condominioId.Value.ToString();

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(http);

        var rbac = new RbacService(_roleRepo, _usuarioRepo, _cache);
        var handler = new PermissionAuthorizationHandler(rbac, accessor);
        return (handler, http);
    }

    [Fact]
    public async Task Handler_SemClaimSub_Falha()
    {
        var (handler, _) = CriarHandler(condominioId: Guid.NewGuid());
        var requirement = new PermissionRequirement("financeiro:ler");
        var authContext = new AuthorizationHandlerContext(
            [requirement], new ClaimsPrincipal(), null);

        await handler.HandleAsync(authContext);

        Assert.True(authContext.HasFailed);
    }

    [Fact]
    public async Task Handler_SemHeaderCondominioId_Falha()
    {
        var (handler, _) = CriarHandler(userId: Guid.NewGuid());
        var requirement = new PermissionRequirement("financeiro:ler");
        var authContext = new AuthorizationHandlerContext(
            [requirement], new ClaimsPrincipal(), null);

        await handler.HandleAsync(authContext);

        Assert.True(authContext.HasFailed);
    }

    [Fact]
    public async Task Handler_PermissaoPresente_Sucede()
    {
        var userId = Guid.NewGuid();
        var condId = Guid.NewGuid();
        var (handler, _) = CriarHandler(userId, condId);

        var perm = new OpenKondominio.Api.Modulos.Auth.Domain.Entities.PermissionEntity("financeiro:ler");
        var role = new OpenKondominio.Api.Modulos.Auth.Domain.Entities.RoleEntity("Sindico", "S");
        role.AdicionarPermissao(perm);

        _roleRepo.BuscarRolesPorUsuarioCondominioAsync(userId, condId)
            .Returns(new[] { role });

        var requirement = new PermissionRequirement("financeiro:ler");
        var claims = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("sub", userId.ToString()) }, "test"));
        var authContext = new AuthorizationHandlerContext([requirement], claims, null);

        await handler.HandleAsync(authContext);

        Assert.True(authContext.HasSucceeded);
    }

    [Fact]
    public async Task Handler_PermissaoAusente_Falha()
    {
        var userId = Guid.NewGuid();
        var condId = Guid.NewGuid();
        var (handler, _) = CriarHandler(userId, condId);

        _roleRepo.BuscarRolesPorUsuarioCondominioAsync(userId, condId)
            .Returns(Array.Empty<OpenKondominio.Api.Modulos.Auth.Domain.Entities.RoleEntity>());

        var requirement = new PermissionRequirement("financeiro:ler");
        var claims = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("sub", userId.ToString()) }, "test"));
        var authContext = new AuthorizationHandlerContext([requirement], claims, null);

        await handler.HandleAsync(authContext);

        Assert.True(authContext.HasFailed);
    }
}
