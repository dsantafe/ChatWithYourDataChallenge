namespace ChatWithYourData.Application.Services
{
    using ChatWithYourData.Application.DTOs;
    using ChatWithYourData.Application.Interfaces;
    using ChatWithYourData.Application.Utils;
    using ChatWithYourData.Domain.Enums;
    using ChatWithYourData.Infrastructure.Managers;
    using Microsoft.Extensions.Caching.Memory;
    using Newtonsoft.Json;
    using System.Dynamic;
    using System.Xml.Linq;

    public class NLToSQLQueryService(
        IMemoryCache memoryCache) : INLToSQLQueryService
    {
        public DatabaseType GetDatabaseType(
            string database)
        {
            return (DatabaseType)Enum.Parse(typeof(DatabaseType), database ?? "MSSQL", true);
        }

        private double GetRetailPriceTokens(
            string skuName,
            int tokenCount)
        {
            string cacheKey = $"RetailPrice|{skuName}";
            if (!memoryCache.TryGetValue(cacheKey, out BillingInfoDTO billingInfo))
            {
                string retailPricesApi = ConfigurationManager.GetValue("OPENAI_RETAIL_PRICES");
                retailPricesApi += $"?api-version=2023-01-01-preview&currencyCode=USD&$filter=armRegionName eq 'eastus'  and skuName eq '{skuName}'";
                string response = HttpClientUtils.Get(retailPricesApi, 120);
                billingInfo = JsonConvert.DeserializeObject<BillingInfoDTO>(response);

                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                   .SetSlidingExpiration(TimeSpan.FromSeconds(900)) //15min
                   .SetAbsoluteExpiration(TimeSpan.FromSeconds(1800)) // 30min
                   .SetPriority(CacheItemPriority.Normal)
                   .SetSize(1);

                memoryCache.Set(cacheKey, billingInfo, cacheEntryOptions);
            }

            if (billingInfo != null && billingInfo.Items.Count > 0)
                return (billingInfo.Items[0].RetailPrice * tokenCount) / 1000;
            else
                return 0;
        }

        public TokensConsumptionDTO GetTokensConsumption(
            string naturalLanguageQuery,
            string templatesPath)
        {
            string database;
            try
            {
                string routerApi = ConfigurationManager.GetValue("OPENAI_ROUTER_API");
                dynamic expando = new ExpandoObject();
                expando.query = naturalLanguageQuery;
                database = HttpClientUtils.Post(routerApi, expando, 120);
            }
            catch (Exception) { throw new Exception("InvalidDatabaseType"); }
            DatabaseType databaseType = database switch
            {
                "cosmosdb" => DatabaseType.CosmosDB,
                "sqlserver" => DatabaseType.MSSQL,
                "sqlite" => DatabaseType.SQLite,
                _ => throw new Exception("InvalidDatabaseType")
            };

            return CalculateTokensConsumption(naturalLanguageQuery, databaseType, templatesPath);
        }

        public TokensConsumptionDTO GetTokensConsumption(
            string naturalLanguageQuery,
            DatabaseType databaseType,
            string templatesPath)
        {
            return CalculateTokensConsumption(naturalLanguageQuery, databaseType, templatesPath);
        }

        private TokensConsumptionDTO CalculateTokensConsumption(
            string naturalLanguageQuery,
            DatabaseType databaseType,
            string templatesPath)
        {
            string templateName = GetDatabaseTemplate(databaseType);
            string filePath = Path.Combine(templatesPath, $"{templateName}.txt");
            string template = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;

            int tokenCount = OpenAiManager.GetTokenizerTokenCount(naturalLanguageQuery, template);
            double retailPrice = GetRetailPriceTokens("gpt-35-turbo4K-Inp-glbl", tokenCount);

            return new TokensConsumptionDTO
            {
                SelectedDatabaseType = databaseType.ToString(),
                TokenCount = tokenCount,
                RetailPrice = retailPrice
            };
        }

        public GenerateSqlQueryDTO GenerateSqlQuery(
            string naturalLanguageQuery,
            DatabaseType databaseType,
            string templatesPath)
        {
            string templateName = GetDatabaseTemplate(databaseType);
            string filePath = Path.Combine(templatesPath, $"{templateName}.txt");
            string template = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;

            // Define the endpoint URL and API key
            string deploymentName = ConfigurationManager.GetValue("OPENAI_DEPLOYMENT_NAME");
            string endpointUrl = ConfigurationManager.GetValue("OPENAI_ENDPOINT_URL");
            string apiKey = ConfigurationManager.GetValue("OPENAI_API_KEY");

            var response = new OpenAiManager(endpointUrl, apiKey).GenerateSqlQuery(deploymentName, naturalLanguageQuery, template);
            string sqlQuery = response.Item1;
            int tokenCountInp = response.Item2;
            int tokenCountOutp = response.Item3;

            double retailPriceInp = GetRetailPriceTokens("gpt-35-turbo4K-Inp-glbl", tokenCountOutp);
            double retailPriceOutp = GetRetailPriceTokens("gpt-35-turbo4K-Outp-glbl", tokenCountOutp);
            double retailPrice = retailPriceInp + retailPriceOutp;

            return new GenerateSqlQueryDTO
            {
                GeneratedSqlQuery = sqlQuery,
                TokenCountInp = tokenCountInp,
                TokenCountOutp = tokenCountOutp,
                RetailPrice = retailPrice,
            };
        }

        public ExecuteSqlQueryDTO ExecuteSqlQuery(
            string sqlQuery,
            DatabaseType databaseType)
        {
            string[] connectionString = GetDatabaseConecction(databaseType);
            List<string> results = databaseType switch
            {
                DatabaseType.CosmosDB => new CosmosDBManager(connectionString[0], connectionString[1], connectionString[2]).ExecuteQuery(sqlQuery),
                DatabaseType.MSSQL => new MSSQLManager(connectionString[0]).ExecuteQuery(sqlQuery),
                DatabaseType.SQLite => new SQLiteManager(connectionString[0], connectionString[1]).ExecuteQuery(sqlQuery),
                _ => ["Invalid Database Type"],
            };
            return new ExecuteSqlQueryDTO { Results = results };
        }

        private static string[] GetDatabaseConecction(
            DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SQLite =>
                [
                    ConfigurationManager.GetValue("SQLITE_DATABASE_URL"),
                    ConfigurationManager.GetValue("SQLITE_AUTH_TOKEN")
                ],
                DatabaseType.MSSQL =>
                [
                    ConfigurationManager.GetValue("MSSQL_CONNECTION_STRING")
                ],
                DatabaseType.CosmosDB =>
                [
                    ConfigurationManager.GetValue("COSMOS_DATABASE"),
                    ConfigurationManager.GetValue("COSMOS_CONTAINER"),
                    ConfigurationManager.GetValue("COSMOS_CONNECTION_STRING")
                ],
                _ => []
            };
        }

        private static string GetDatabaseTemplate(
            DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SQLite => "sqlite_template",
                DatabaseType.MSSQL => "mssql_template",
                DatabaseType.CosmosDB => "cosmos_template",
                _ => string.Empty
            };
        }

        public ExecutionPlanDTO GetExecutionPlan(
            string sqlQuery)
        {
            string[] connectionString = GetDatabaseConecction(DatabaseType.MSSQL);
            string planXml = new MSSQLManager(connectionString[0])
                .GetExecutionPlan(sqlQuery);
            return ParseExecutionPlan(planXml);
        }

        private static ExecutionPlanDTO ParseExecutionPlan(
            string planXml)
        {
            var plan = new ExecutionPlanDTO();
            try
            {
                var xDoc = XDocument.Parse(planXml);

                // Obtenemos el nodo StmtSimple
                var stmtSimple = xDoc.Descendants("{http://schemas.microsoft.com/sqlserver/2004/07/showplan}StmtSimple").FirstOrDefault();
                if (stmtSimple != null)
                {
                    plan.StatementText = stmtSimple.Attribute("StatementText")?.Value;
                    plan.StatementType = stmtSimple.Attribute("StatementType")?.Value;
                    plan.StatementCost = stmtSimple.Attribute("StatementSubTreeCost")?.Value;
                    plan.EstimatedRows = stmtSimple.Attribute("StatementEstRows")?.Value;

                    // Información del plan de consulta (QueryPlan)
                    var queryPlan = stmtSimple.Descendants("{http://schemas.microsoft.com/sqlserver/2004/07/showplan}QueryPlan").FirstOrDefault();
                    if (queryPlan != null)
                    {
                        plan.CompileTime = queryPlan.Attribute("CompileTime")?.Value;
                        plan.CompileCPU = queryPlan.Attribute("CompileCPU")?.Value;
                        plan.CompileMemory = queryPlan.Attribute("CompileMemory")?.Value;
                    }

                    // Extraemos la operación RelOp
                    var relOp = stmtSimple.Descendants("{http://schemas.microsoft.com/sqlserver/2004/07/showplan}RelOp").FirstOrDefault();
                    if (relOp != null)
                    {
                        plan.PhysicalOp = relOp.Attribute("PhysicalOp")?.Value;
                        plan.LogicalOp = relOp.Attribute("LogicalOp")?.Value;
                        plan.EstimatedRows = relOp.Attribute("EstimateRows")?.Value;
                        plan.EstimatedCost = relOp.Attribute("EstimatedTotalSubtreeCost")?.Value;
                        plan.EstimateCPU = relOp.Attribute("EstimateCPU")?.Value;
                    }
                }
            }
            catch { }

            return plan;
        }
    }
}
