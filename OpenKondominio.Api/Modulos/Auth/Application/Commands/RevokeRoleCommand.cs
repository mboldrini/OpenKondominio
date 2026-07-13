namespace OpenKondominio.Api.Modulos.Auth.Application.Commands;
public record RevokeRoleCommand(Guid UsuarioId, Guid CondominioId, Guid RoleId);
