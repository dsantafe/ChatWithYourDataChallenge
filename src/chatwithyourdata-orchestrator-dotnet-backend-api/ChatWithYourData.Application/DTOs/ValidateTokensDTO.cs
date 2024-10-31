namespace ChatWithYourData.Application.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class TokensConsumptionReqDTO
    {
        [Required]
        public string NaturalLanguageQuery { get; set; }
    }

    public class TokensConsumptionDTO
    {
        public string SelectedDatabaseType { get; set; }

        public int TokenCount { get; set; }

        public double RetailPrice { get; set; }
    }
}
