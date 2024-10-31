namespace ChatWithYourData.Application.DTOs
{
    public class UserTokensHistoryDTO
    {
        public Guid ID { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public int TokensConsumed { get; set; }

        public DateTime RecordedAt { get; set; }

        public string HistoryDescription => $"Tokens consumed: {TokensConsumed} on {Year}-{Month} last record {RecordedAt:yyyy-MM-dd}";
    }
}
