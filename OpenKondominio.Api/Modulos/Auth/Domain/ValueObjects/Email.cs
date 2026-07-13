using System.Text.RegularExpressions;

namespace OpenKondominio.Api.Modulos.Auth.Domain.ValueObjects;

public sealed class Email : IEquatable<Email>
{
    private static readonly Regex Pattern =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Valor { get; }

    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Email não pode ser vazio.", nameof(valor));

        if (!Pattern.IsMatch(valor))
            throw new ArgumentException($"Formato de email inválido: '{valor}'.", nameof(valor));

        Valor = valor.ToLowerInvariant();
    }

    public bool Equals(Email? other) => other is not null && Valor == other.Valor;
    public override bool Equals(object? obj) => obj is Email e && Equals(e);
    public override int GetHashCode() => Valor.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Valor;
}
