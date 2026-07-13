namespace OpenKondominio.Api.Modulos.Auth.Application.DTOs;
public record UsuarioDto(Guid Id, string Email, string NomeCompleto, bool Ativo);
