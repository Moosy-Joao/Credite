using Credite.Web.Models;
using Credite.Web.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Credite.Web.Controllers;

public sealed class SolicitacoesController(ICrediteApiClient api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        Guid? clienteId,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        if (pagina < 1)
            pagina = 1;

        if (tamanhoPagina is < 1 or > 100)
            tamanhoPagina = 20;

        var modelo = new SolicitacoesListaViewModel
        {
            ClienteId = clienteId,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        };

        try
        {
            var resultado = await api.ListarSolicitacoesAsync(
                clienteId, pagina, tamanhoPagina, cancellationToken);

            modelo = new SolicitacoesListaViewModel
            {
                ClienteId = clienteId,
                Pagina = resultado.Pagina,
                TamanhoPagina = resultado.TamanhoPagina,
                TotalRegistros = resultado.TotalRegistros,
                TotalPaginas = resultado.TotalPaginas,
                Itens = resultado.Itens
            };
        }
        catch (CrediteApiException excecao)
        {
            ViewBag.Erro = excecao.Message;
        }
        catch (HttpRequestException)
        {
            ViewBag.Erro = "Não foi possível conectar à Credit API. Verifique se ela está em execução.";
        }

        return View(modelo);
    }

    [HttpGet]
    public IActionResult Criar() => View(new SolicitacaoFormularioViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(
        SolicitacaoFormularioViewModel modelo,
        CancellationToken cancellationToken)
    {
        if (modelo.ClienteId == Guid.Empty)
            ModelState.AddModelError(nameof(modelo.ClienteId), "Informe um identificador de cliente válido.");

        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            var solicitacao = await api.CriarSolicitacaoAsync(
                new CriarSolicitacaoApiRequest(modelo.ClienteId, modelo.ValorSolicitado),
                cancellationToken);

            TempData["Sucesso"] = "Solicitação registrada. Status inicial: Em análise.";
            return RedirectToAction(nameof(Detalhes), new { id = solicitacao.Id });
        }
        catch (CrediteApiException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty,
                "Não foi possível conectar à Credit API. Verifique se ela está em execução.");
        }

        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var solicitacao = await api.ConsultarSolicitacaoAsync(id, cancellationToken);
            if (solicitacao is null)
                return NotFound("Solicitação não encontrada.");

            return View(solicitacao);
        }
        catch (CrediteApiException excecao)
        {
            return StatusCode((int)excecao.StatusCode, excecao.Message);
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                "Não foi possível conectar à Credit API.");
        }
    }
}
