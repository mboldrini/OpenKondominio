using Microsoft.AspNetCore.Authorization;

namespace OpenKondominio.Api.Modulos.Auth.Infrastructure.Security;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission) => Permission = permission;
}
