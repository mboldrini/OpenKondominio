using OpenKondominio.Api.Modulos.Auth.Domain.ValueObjects;

namespace OpenKondominio.Tests.Auth.Domain;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("user.name+tag@sub.domain.org")]
    public void Email_AceitaFormatoValido(string valor)
    {
        var email = new Email(valor);
        Assert.Equal(valor.ToLowerInvariant(), email.Valor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("semArroba")]
    [InlineData("@semLocal.com")]
    [InlineData("sem@dominio")]
    [InlineData("dois@@arroba.com")]
    public void Email_RejeitaFormatoInvalido(string valor)
    {
        Assert.Throws<ArgumentException>(() => new Email(valor));
    }

    [Fact]
    public void Email_NullLancaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Email(null!));
    }

    [Fact]
    public void Email_IgualdadePorValor()
    {
        var a = new Email("user@example.com");
        var b = new Email("USER@EXAMPLE.COM");
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Email_Diferentes_NaoSaoIguais()
    {
        var a = new Email("a@example.com");
        var b = new Email("b@example.com");
        Assert.NotEqual(a, b);
    }
}
