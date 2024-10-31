namespace ChatWithYourData.Infrastructure.Managers
{
    using Libsql.Client;

    public class SQLiteManager(
        string connectionString,
        string token)
    {
        private readonly IDatabaseClient _connection = DatabaseClient.Create(opts =>
        {
            opts.Url = connectionString;
            opts.AuthToken = token;
        }).GetAwaiter().GetResult();

        public List<string> ExecuteQuery(
             string sqlQuery)
        {
            List<string> results = [];

            IResultSet result = _connection.Execute(sqlQuery).GetAwaiter().GetResult();
            results.Add(string.Join(" | ", result.Columns));
            foreach (var rowValues in result.Rows)
                results.Add(string.Join(" | ", rowValues));

            return results;
        }
    }
}
