namespace ChatWithYourData.Infrastructure.Managers
{
    using Microsoft.Data.SqlClient;

    public class MSSQLManager
    {
        private readonly SqlConnection _connection;

        public MSSQLManager(
            string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();
        }

        public List<string> ExecuteQuery(
            string sqlQuery)
        {
            List<string> results = [];

            try
            {
                EnsureConnectionOpen();

                using SqlCommand command = new(sqlQuery, _connection);
                using SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    results.Add(string.Join(" | ", GetColumnNames(reader)));

                    while (reader.Read())
                        results.Add(string.Join(" | ", GetRowValues(reader)));
                }
                else
                    results.Add("No Result Found");
            }
            finally
            {
                CloseConnectionIfOpen();
            }

            return results;
        }

        public string GetExecutionPlan(
            string sqlQuery)
        {
            // Ejecutar 'SET SHOWPLAN_XML ON' como un comando independiente
            using SqlCommand setShowPlanOn = new("SET SHOWPLAN_XML ON;", _connection);
            setShowPlanOn.ExecuteNonQuery();

            // Ejecutar la consulta y obtener el plan de ejecución
            using SqlCommand planCommand = new(sqlQuery, _connection);
            using SqlDataReader planReader = planCommand.ExecuteReader();
            string executionPlanXml = string.Empty;

            if (planReader.HasRows && planReader.Read())
                executionPlanXml = planReader.GetString(0); // El plan de ejecución en formato XML

            // Cerrar el reader
            planReader.Close();

            // Ejecutar 'SET SHOWPLAN_XML OFF' para desactivar la obtención de planes de ejecución
            using SqlCommand setShowPlanOff = new("SET SHOWPLAN_XML OFF;", _connection);
            setShowPlanOff.ExecuteNonQuery();

            return executionPlanXml;
        }

        private void EnsureConnectionOpen()
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();
        }

        private void CloseConnectionIfOpen()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
                _connection.Close();
        }

        private static List<string> GetColumnNames(
            SqlDataReader reader)
        {
            List<string> columnNames = [];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                columnNames.Add(!string.IsNullOrEmpty(columnName) ? columnName : $"Column{i + 1}");
            }
            return columnNames;
        }

        private static List<string> GetRowValues(
            SqlDataReader reader)
        {
            List<string> rowValues = [];
            for (int i = 0; i < reader.FieldCount; i++)
                rowValues.Add(reader.GetValue(i)?.ToString() ?? "NULL");
            return rowValues;
        }
    }
}
