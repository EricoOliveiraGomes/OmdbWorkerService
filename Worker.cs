namespace OmdbWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly HttpClient _httpClient;

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;            
            _httpClient = httpClientFactory.CreateClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                _logger.LogInformation("Verificando API: {time}", DateTimeOffset.Now);

                try {

                    var response = await _httpClient.GetAsync("http://www.omdbapi.com/?i=tt3896198&apikey=b0dd6377", stoppingToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var conteudo = await response.Content.ReadAsStringAsync(stoppingToken);
                        _logger.LogInformation("API Respondeu com sucesso.");
                    }
                    else
                    {
                        _logger.LogWarning("API retornou erro: {statusCode}", response.StatusCode);
                    }
                }                
                catch (Exception ex)
                {
                    _logger.LogError("Erro ao acessar API: {message}", ex.Message);
                }

                await Task.Delay(120000, stoppingToken);
            }
        }
    }
}
