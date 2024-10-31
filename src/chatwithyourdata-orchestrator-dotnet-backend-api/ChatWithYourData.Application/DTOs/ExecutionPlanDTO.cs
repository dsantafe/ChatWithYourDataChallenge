namespace ChatWithYourData.Application.DTOs
{
    public class ExecutionPlanDTO
    {
        // Propiedades relacionadas con la instrucción SQL
        public string StatementText { get; set; }  // Texto de la instrucción SQL
        public string StatementType { get; set; }  // Tipo de instrucción (SELECT, INSERT, etc.)
        public string StatementCost { get; set; }  // Costo estimado del subárbol de la instrucción
        public string EstimatedRows { get; set; }  // Filas estimadas de la instrucción

        // Propiedades relacionadas con el plan de consulta
        public string CompileTime { get; set; }    // Tiempo de compilación del plan
        public string CompileCPU { get; set; }     // Uso de CPU en la compilación
        public string CompileMemory { get; set; }  // Memoria usada en la compilación

        // Propiedades relacionadas con las operaciones en el plan de ejecución
        public string PhysicalOp { get; set; }     // Operación física (ej. Index Seek, Scan)
        public string LogicalOp { get; set; }      // Operación lógica (ej. Join, Filter)
        public string EstimatedCost { get; set; }  // Costo estimado total del subárbol
        public string EstimateCPU { get; set; }    // CPU estimado para la operación
    }
}
