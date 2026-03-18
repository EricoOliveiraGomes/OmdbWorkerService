using Microsoft.Data.Sqlite;
using System.Data;

namespace OmdbWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly HttpClient _httpClient;
        private static Random random = new();        
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly string _baseUri;

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        { 
            _logger = logger;            
            _httpClient = httpClientFactory.CreateClient();            
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("SqliteConnection")!;            
            _baseUri = _configuration.GetValue<string>("BaseUri")!;
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
                    
                    string requestUri = $"{_baseUri}&t={titulo}&page={pagina}&y={ano}";

                    var response = await _httpClient.GetAsync(requestUri, stoppingToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var conteudo = await response.Content.ReadAsStringAsync(stoppingToken);

                        using (IDbConnection db = new SqliteConnection(_connectionString))
                        {                            
                            db.Open();

                            var selectCmd = db.CreateCommand();
                            selectCmd.CommandText = "SELECT 1 FROM Omdb WHERE InformacaoFilme = @InformacaoFilme";
                            selectCmd.Parameters.Add(new SqliteParameter("@InformacaoFilme", conteudo));

                            var retorno = selectCmd.ExecuteReader();
                            if (!retorno.Read())
                            {
                                var insertCmd = db.CreateCommand();
                                insertCmd.CommandText = "INSERT INTO Omdb (InformacaoFilme) VALUES (@InformacaoFilme)";
                                insertCmd.Parameters.Add(new SqliteParameter("@InformacaoFilme", conteudo));
                                insertCmd.ExecuteNonQuery();
                            }
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
