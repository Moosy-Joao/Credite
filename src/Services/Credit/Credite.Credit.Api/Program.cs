// Host inicial do serviço. Endpoints técnicos (saúde) entram na Etapa 5.
// Endpoints de negócio dependem da ADR-0015 e não devem ser criados antes dela.
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Run();
