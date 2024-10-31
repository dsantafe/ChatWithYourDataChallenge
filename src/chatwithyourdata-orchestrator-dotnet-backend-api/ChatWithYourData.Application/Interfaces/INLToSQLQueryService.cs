namespace ChatWithYourData.Application.Interfaces
{
    using ChatWithYourData.Application.DTOs;
    using ChatWithYourData.Domain.Enums;

    public interface INLToSQLQueryService
    {
        DatabaseType GetDatabaseType(string database);
        TokensConsumptionDTO GetTokensConsumption(string naturalLanguageQuery, string templatesPath);
        TokensConsumptionDTO GetTokensConsumption(string naturalLanguageQuery, DatabaseType databaseType, string templatesPath);
        GenerateSqlQueryDTO GenerateSqlQuery(string naturalLanguageQuery, DatabaseType databaseType, string templatesPath);
        ExecuteSqlQueryDTO ExecuteSqlQuery(string sqlQuery, DatabaseType databaseType);
        ExecutionPlanDTO GetExecutionPlan(string sqlQuery);
    }
}