// Host inicial do Worker Service. O consumidor de eventos (Kafka) pertence à Etapa 4
// e depende da ADR de contratos de eventos. Este serviço não expõe endpoints de negócio.
var builder = Host.CreateApplicationBuilder(args);

var host = builder.Build();

host.Run();
