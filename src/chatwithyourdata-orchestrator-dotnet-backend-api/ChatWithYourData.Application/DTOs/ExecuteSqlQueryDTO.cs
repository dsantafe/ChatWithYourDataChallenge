namespace ChatWithYourData.Application.DTOs
{
    public class ExecuteSqlQueryReqDTO
    {
        public string GeneratedSqlQuery { get; set; }

        public string SelectedDatabaseType { get; set; }
    }

    public class ExecuteSqlQueryDTO
    {
        public List<string> Results { get; set; }
    }
}
