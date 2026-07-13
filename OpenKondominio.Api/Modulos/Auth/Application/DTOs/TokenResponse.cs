namespace OpenKondominio.Api.Modulos.Auth.Application.DTOs;
public record TokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);
