using System.Net;

namespace Credite.Web.Servicos;

public sealed class CrediteApiException(HttpStatusCode statusCode, string mensagem)
    : Exception(mensagem)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
