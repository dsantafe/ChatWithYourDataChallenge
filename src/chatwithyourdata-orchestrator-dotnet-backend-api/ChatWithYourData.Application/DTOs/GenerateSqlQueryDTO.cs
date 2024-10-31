namespace ChatWithYourData.Application.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class GenerateSqlQueryReqDTO
    {
        [Required]
        public string NaturalLanguageQuery { get; set; }

        [Required]
        public string SelectedDatabaseType { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }

    public class GenerateSqlQueryDTO
    {
        public string GeneratedSqlQuery { get; set; }

        public int TokenCountInp { get; set; }

        public int TokenCountOutp { get; set; }

        public double RetailPrice { get; set; }
    }
}
