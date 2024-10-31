namespace ChatWithYourData.Infrastructure.Managers
{
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;

    public class CosmosDBManager
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public CosmosDBManager(
            string databaseName,
            string containerName,
            string connectionString)
        {
            _cosmosClient = new CosmosClient(connectionString);
            _container = _cosmosClient.GetContainer(databaseName, containerName);
        }

        public List<string> ExecuteQuery(
            string queryString)
        {
            List<string> results = [];

            FeedIterator<dynamic> iterator = GetFeedIterator(queryString);
            List<dynamic> resultsJson = FetchResultsAsync(iterator);

            if (resultsJson == null || resultsJson.Count == 0)
            {
                results.Add("No Result Found");
                return results;
            }

            List<string> firstRow = GetPropertyNames(resultsJson);
            results.Add(string.Join(" | ", firstRow));

            foreach (var item in resultsJson)
                results.Add(string.Join(" | ", GetPropertyValues(item, firstRow)));

            return results;
        }

        private FeedIterator<dynamic> GetFeedIterator(
            string queryString)
        {
            return !string.IsNullOrEmpty(queryString)
                ? _container.GetItemQueryIterator<dynamic>(new QueryDefinition(queryString))
                : _container.GetItemLinqQueryable<dynamic>().ToFeedIterator();
        }

        private static List<dynamic> FetchResultsAsync(
            FeedIterator<dynamic> iterator)
        {
            List<dynamic> resultsJson = [];
            while (iterator.HasMoreResults)
            {
                FeedResponse<dynamic> response = iterator.ReadNextAsync().GetAwaiter().GetResult();
                resultsJson.AddRange(response);
            }
            return resultsJson;
        }

        private static List<string> GetPropertyNames(
            List<dynamic> resultsJson)
        {
            var firstRow = new HashSet<string>();
            foreach (var item in resultsJson)
            {
                var jsonObject = JObject.FromObject(item);
                foreach (var prop in jsonObject.Properties())
                    firstRow.Add(prop.Name);
            }
            return firstRow.ToList();
        }

        private static List<string> GetPropertyValues(
            dynamic item,
            List<string> propertyNames)
        {
            var jsonObject = JObject.FromObject(item);
            var values = new List<string>();

            foreach (var prop in propertyNames)
            {
                if (jsonObject[prop] != null)
                {
                    if (jsonObject[prop] is JArray array)
                    {
                        // Para cada objeto en el array, combina todas sus propiedades en un solo string
                        var arrayValues = array.Select(arrItem =>
                        {
                            var obj = arrItem as JObject;
                            // Concatena todas las propiedades clave-valor del objeto en un solo string
                            var propertyPairs = obj?.Properties()
                                .Select(p => $"{p.Name}: {p.Value}")
                                .ToList();

                            return propertyPairs != null ? string.Join(", ", propertyPairs) : "NULL";
                        });

                        values.Add(string.Join(", ", arrayValues));
                    }
                    else
                        values.Add(jsonObject[prop].ToString());
                }
                else
                    values.Add("NULL");
            }
            return values;
        }
    }
}
