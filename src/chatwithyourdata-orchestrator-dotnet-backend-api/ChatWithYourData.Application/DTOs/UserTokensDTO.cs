namespace ChatWithYourData.Application.DTOs
{
    public class UserTokensDTO
    {
        public Guid ID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public int TokensAvailable { get; set; }
    }
}
