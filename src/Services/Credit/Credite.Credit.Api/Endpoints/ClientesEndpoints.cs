using Credite.Credit.Api.Dados;
using Credite.Credit.Api.Dominio;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Credite.Credit.Api.Endpoints;

public static class ClientesEndpoints
{
    public static IEndpointRouteBuilder MapClientesEndpoints(this IEndpointRouteBuilder rotas)
    {
        var grupo = rotas.MapGroup("/clientes").WithTags("Clientes");
        grupo.MapPost("/", Cadastrar);
        grupo.MapGet("/{id:guid}", ConsultarPorId);
        return rotas;
    }

    private static async Task<IResult> Cadastrar(
        CadastrarClienteRequest? requisicao,
        CrediteDbContext banco,
        CancellationToken cancellationToken)
    {
        if (requisicao is null ||
            string.IsNullOrWhiteSpace(requisicao.Nome) ||
            string.IsNullOrWhiteSpace(requisicao.Email))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["cliente"] = ["Nome e e-mail são obrigatórios."]
            });
        }

        try
        {
            var emailNormalizado = requisicao.Email.Trim().ToLowerInvariant();
            var emailJaCadastrado = await banco.Clientes.AnyAsync(
                cliente => cliente.Email == emailNormalizado, cancellationToken);

            if (emailJaCadastrado)
                return Results.Conflict(new { mensagem = "Já existe um cliente com esse e-mail." });

            var cliente = new Cliente(requisicao.Nome, requisicao.Email);
            banco.Clientes.Add(cliente);
            await banco.SaveChangesAsync(cancellationToken);

            return Results.Created($"/clientes/{cliente.Id}", ClienteResposta.De(cliente));
        }
        catch (ArgumentException excecao)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["cliente"] = [excecao.Message]
            });
        }
        catch (DbUpdateException excecao) when (EhEmailDuplicado(excecao))
        {
            return Results.Conflict(new { mensagem = "Já existe um cliente com esse e-mail." });
        }
    }

    private static bool EhEmailDuplicado(DbUpdateException excecao) =>
        excecao.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "ix_clientes_email"
        };

    private static async Task<IResult> ConsultarPorId(
        Guid id,
        CrediteDbContext banco,
        CancellationToken cancellationToken)
    {
        var cliente = await banco.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(cliente => cliente.Id == id, cancellationToken);

        return cliente is null ? Results.NotFound() : Results.Ok(ClienteResposta.De(cliente));
    }
}

public sealed record CadastrarClienteRequest(string Nome, string Email);

public sealed record ClienteResposta(Guid Id, string Nome, string Email, DateTimeOffset CriadoEm)
{
    public static ClienteResposta De(Cliente cliente) =>
        new(cliente.Id, cliente.Nome, cliente.Email, cliente.CriadoEm);
}
