namespace ChatWithYourData.Application.Interfaces
{
    using ChatWithYourData.Application.DTOs;

    public interface IUserTokenService
    {
        UserTokensDTO GetUserTokens(string username, string password);
        UserTokensDTO GetUserTokensById(Guid userId);
        void UpdateTokenConsumption(Guid userId, int tokensAvailable);
        List<UserTokensHistoryDTO> GetTokensHistoryByUserId(Guid userId);
    }
}