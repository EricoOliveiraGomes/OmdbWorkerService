using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace OmdbWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly HttpClient _httpClient;
        private static Random random = new();

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

                    char titulo = ObterLetraAleatoria();
                    int pagina = ObterPaginaAleatoria();
                    int ano = ObterAnoAleatorio();
                    var connectionString = "Data Source=omdb.db";

                    string requestUri = $"http://www.omdbapi.com/?apikey=b0dd6377&plot=full&t={titulo}&page={pagina}&y={ano}";

                    var response = await _httpClient.GetAsync(requestUri, stoppingToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var conteudo = await response.Content.ReadAsStringAsync(stoppingToken);

                        SQLitePCL.raw.SetProvider(new SQLite3Provider_e_sqlite3());


                        {
                            connection.Open();
                                                        
                            var insertCmd = connection.CreateCommand();
                            insertCmd.CommandText = "INSERT INTO omdb (movie_information) VALUES ($conteudo)";
                            insertCmd.Parameters.AddWithValue("$conteudo", conteudo);
                            insertCmd.ExecuteNonQuery();
                        }

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

        public static char ObterLetraAleatoria()
        {
            string alfabeto = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int index = random.Next(alfabeto.Length);
            return alfabeto[index];
        }

        public static int ObterPaginaAleatoria()
        {            
            return random.Next(1, 101);
        }
        public static int ObterAnoAleatorio()
        {
            return random.Next(1888, DateTime.Now.Year+1);
        }
    }
}
