namespace OpenKondominio.Api.Modulos.Auth.Application.Commands;
public record AssignRoleCommand(Guid UsuarioId, Guid CondominioId, Guid RoleId);
