using Credite.Web.Servicos;

var construtor = WebApplication.CreateBuilder(args);

construtor.Services.AddControllersWithViews();

var urlApi = construtor.Configuration["ApiSettings:BaseUrl"];
if (!Uri.TryCreate(urlApi, UriKind.Absolute, out var enderecoApi) ||
    (enderecoApi.Scheme != Uri.UriSchemeHttp && enderecoApi.Scheme != Uri.UriSchemeHttps))
{
    throw new InvalidOperationException(
        "Configure ApiSettings:BaseUrl com uma URL HTTP ou HTTPS válida da Credit API.");
}

construtor.Services.AddHttpClient<ICrediteApiClient, CrediteApiClient>(cliente =>
{
    cliente.BaseAddress = enderecoApi;
    cliente.Timeout = TimeSpan.FromSeconds(15);
});

var aplicacao = construtor.Build();

if (!aplicacao.Environment.IsDevelopment())
{
    aplicacao.UseExceptionHandler("/Home/Erro");
    aplicacao.UseHsts();
}

aplicacao.UseHttpsRedirection();
aplicacao.UseStaticFiles();
aplicacao.UseRouting();
aplicacao.UseAuthorization();

aplicacao.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await aplicacao.RunAsync();

public partial class Program { }
