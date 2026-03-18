using Microsoft.Data.Sqlite;
using SQLitePCL;
using System.Data;
using System.Data.Common;

namespace OmdbWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly HttpClient _httpClient;
        private static Random random = new();
        //private readonly IDbConnection _connection;
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)//, IDbConnection connection)
        {
            _logger = logger;            
            _httpClient = httpClientFactory.CreateClient();
            //_connection = connection;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("SqliteConnection");
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
                    //var connectionString = "Data Source=omdb.db";

                    string requestUri = $"http://www.omdbapi.com/?apikey=b0dd6377&plot=full&t={titulo}&page={pagina}&y={ano}";

                    var response = await _httpClient.GetAsync(requestUri, stoppingToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var conteudo = await response.Content.ReadAsStringAsync(stoppingToken);

                        //SQLitePCL.raw.SetProvider(new SQLite3Provider_e_sqlite3());
                        //SQLitePCL.raw.SetProvider(new SQLite3Provider_e_sqlite3());
                        //SQLitePCL.Batteries.Init(); 
                        //SQLitePCL.Batteries_V2.Init();
                        //SQLitePCL.
                        //SQLitePCL


                        //var sqlInsert = "INSERT INTO Produtos (Nome, Preco) VALUES (@Nome, @Preco)";
                        //var novoProduto = new { Nome = "Notebook", Preco = 3500.00 };
                        //_connection.Execute(sqlInsert, novoProduto);

                        //_connection.Open();
                        //var insertCmd = _connection.CreateCommand();
                        //insertCmd.CommandText = "INSERT INTO Omdb (InformacaoFilme) VALUES (@InformacaoFilme)";
                        //var novaInformacaoFilme = new { InformacaoFilme = conteudo };
                        ////insertCmd.Parameters.Add("$conteudo", conteudo);
                        //insertCmd.ExecuteNonQuery();


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

                            //var novaInformacaoFilme = new { InformacaoFilme = conteudo };
                            //insertCmd.Parameters.Add(conteudo);
                            

                            //var novaLog = new { Msg = "Worker ativo", Date = DateTime.Now };
                            //var sql = "INSERT INTO Logs (Message, CreatedAt) VALUES (@Msg, @Date)";

                            //await db.ExecuteAsyn(sql, novaLog);
                            //_logger.LogInformation("Log inserido com sucesso.");
                        } // Conexão fechada e liberada aqui (Dispose)



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
