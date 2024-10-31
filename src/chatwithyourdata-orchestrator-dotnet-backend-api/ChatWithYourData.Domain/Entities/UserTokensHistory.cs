namespace ChatWithYourData.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class UserTokensHistory
    {
        public Guid ID { get; set; }

        [ForeignKey(nameof(UserTokens))]
        public Guid UserTokensID { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public int TokensConsumed { get; set; }

        public DateTime RecordedAt { get; set; }

        public UserTokens UserTokens { get; set; }
    }
}
