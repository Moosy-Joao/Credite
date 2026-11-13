using Credite.Web.Models;
using Credite.Web.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Credite.Web.Controllers;

public sealed class ClientesController(ICrediteApiClient api) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new ClienteFormularioViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        ClienteFormularioViewModel modelo,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            var cliente = await api.CadastrarClienteAsync(
                new CadastrarClienteApiRequest(modelo.Nome, modelo.Email),
                cancellationToken);

            TempData["Sucesso"] = "Cliente cadastrado com sucesso.";
            return RedirectToAction(nameof(Consultar), new { id = cliente.Id });
        }
        catch (CrediteApiException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty,
                "Não foi possível conectar à Credit API. Verifique se ela está em execução e se a URL está correta.");
        }

        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Consultar(Guid? id, CancellationToken cancellationToken)
    {
        var modelo = new ClienteConsultaViewModel { Id = id?.ToString() ?? string.Empty };
        if (!id.HasValue)
            return View(modelo);

        try
        {
            modelo.Cliente = await api.ConsultarClienteAsync(id.Value, cancellationToken);
            if (modelo.Cliente is null)
                modelo.Mensagem = "Nenhum cliente foi encontrado com esse identificador.";
        }
        catch (HttpRequestException)
        {
            modelo.Mensagem = "Não foi possível conectar à Credit API. Verifique se ela está em execução.";
        }

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Consultar(ClienteConsultaViewModel modelo)
    {
        if (!Guid.TryParse(modelo.Id, out var id))
        {
            ModelState.AddModelError(nameof(modelo.Id), "Informe um identificador válido.");
            return View(modelo);
        }

        return RedirectToAction(nameof(Consultar), new { id });
    }
}
