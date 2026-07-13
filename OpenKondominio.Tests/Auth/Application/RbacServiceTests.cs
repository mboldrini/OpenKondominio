using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using OpenKondominio.Api.Modulos.Auth.Application.Commands;
using OpenKondominio.Api.Modulos.Auth.Application.Services;
using OpenKondominio.Api.Modulos.Auth.Domain.Entities;
using OpenKondominio.Api.Modulos.Auth.Domain.Interfaces;
using OpenKondominio.Api.Modulos.Auth.Domain.ValueObjects;

namespace OpenKondominio.Tests.Auth.Application;

public class RbacServiceTests
{
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly IRoleRepository _roleRepo = Substitute.For<IRoleRepository>();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    private RbacService CriarService() => new(_roleRepo, _usuarioRepo, _cache);

    [Fact]
    public async Task AssignRole_UsuarioNaoExiste_LancaInvalidOperationException()
    {
        _usuarioRepo.BuscarPorIdAsync(Arg.Any<Guid>()).Returns((UsuarioEntity?)null);

        var svc = CriarService();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.AssignRoleAsync(new AssignRoleCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid())));
    }

    [Fact]
    public async Task AssignRole_RoleNaoExiste_LancaInvalidOperationException()
    {
        var usuario = new UsuarioEntity(new Email("u@test.com"), "U", "h");
        _usuarioRepo.BuscarPorIdAsync(usuario.Id).Returns(usuario);
        _roleRepo.BuscarPorIdAsync(Arg.Any<Guid>()).Returns((RoleEntity?)null);

        var svc = CriarService();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.AssignRoleAsync(new AssignRoleCommand(usuario.Id, Guid.NewGuid(), Guid.NewGuid())));
    }

    [Fact]
    public async Task AssignRole_VinculoJaExiste_NaoAdicionaDeNovo()
    {
        var usuario = new UsuarioEntity(new Email("u@test.com"), "U", "h");
        var role = new RoleEntity("Admin", "desc");
        _usuarioRepo.BuscarPorIdAsync(usuario.Id).Returns(usuario);
        _roleRepo.BuscarPorIdAsync(role.Id).Returns(role);
        _roleRepo.ExisteVinculoAsync(usuario.Id, Arg.Any<Guid>(), role.Id).Returns(true);

        var svc = CriarService();
        await svc.AssignRoleAsync(new AssignRoleCommand(usuario.Id, Guid.NewGuid(), role.Id));

        await _roleRepo.DidNotReceive().AdicionarVinculoAsync(Arg.Any<UsuarioCondominioRoleEntity>());
    }

    [Fact]
    public async Task AssignRole_Novo_AdicionaVinculoESalva()
    {
        var usuario = new UsuarioEntity(new Email("u@test.com"), "U", "h");
        var role = new RoleEntity("Admin", "desc");
        var condId = Guid.NewGuid();
        _usuarioRepo.BuscarPorIdAsync(usuario.Id).Returns(usuario);
        _roleRepo.BuscarPorIdAsync(role.Id).Returns(role);
        _roleRepo.ExisteVinculoAsync(usuario.Id, condId, role.Id).Returns(false);

        var svc = CriarService();
        await svc.AssignRoleAsync(new AssignRoleCommand(usuario.Id, condId, role.Id));

        await _roleRepo.Received(1).AdicionarVinculoAsync(
            Arg.Is<UsuarioCondominioRoleEntity>(v =>
                v.UsuarioId == usuario.Id && v.CondominioId == condId && v.RoleId == role.Id));
        await _roleRepo.Received(1).SalvarAsync();
    }

    [Fact]
    public async Task GetPermissions_RetornaChavesDeTodasAsRoles()
    {
        var userId = Guid.NewGuid();
        var condId = Guid.NewGuid();
        var perm1 = new PermissionEntity("financeiro:ler");
        var perm2 = new PermissionEntity("ata:criar");
        var role = new RoleEntity("Sindico", "Sindico");
        role.AdicionarPermissao(perm1);
        role.AdicionarPermissao(perm2);

        _roleRepo.BuscarRolesPorUsuarioCondominioAsync(userId, condId)
            .Returns(new[] { role });

        var svc = CriarService();
        var perms = (await svc.GetPermissionsAsync(userId, condId)).ToList();

        Assert.Contains("financeiro:ler", perms);
        Assert.Contains("ata:criar", perms);
    }

    [Fact]
    public async Task GetPermissions_SegundaChamada_UsaCache()
    {
        var userId = Guid.NewGuid();
        var condId = Guid.NewGuid();
        var role = new RoleEntity("Sindico", "Sindico");
        _roleRepo.BuscarRolesPorUsuarioCondominioAsync(userId, condId)
            .Returns(new[] { role });

        var svc = CriarService();
        await svc.GetPermissionsAsync(userId, condId);
        await svc.GetPermissionsAsync(userId, condId);

        await _roleRepo.Received(1).BuscarRolesPorUsuarioCondominioAsync(userId, condId);
    }

    [Fact]
    public async Task RevokeRole_ChamaSalvar()
    {
        var cmd = new RevokeRoleCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var svc = CriarService();
        await svc.RevokeRoleAsync(cmd);

        await _roleRepo.Received(1).RemoverVinculoAsync(cmd.UsuarioId, cmd.CondominioId, cmd.RoleId);
        await _roleRepo.Received(1).SalvarAsync();
    }
}
