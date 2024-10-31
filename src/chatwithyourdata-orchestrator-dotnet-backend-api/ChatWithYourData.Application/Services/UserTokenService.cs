namespace ChatWithYourData.Application.Services
{
    using ChatWithYourData.Application.DTOs;
    using ChatWithYourData.Application.Interfaces;
    using ChatWithYourData.Application.Utils;
    using ChatWithYourData.Domain.Data;
    using ChatWithYourData.Domain.Entities;
    using ChatWithYourData.Infrastructure.Managers;

    public class UserTokenService(
        ChatWithYourDataContext context) : IUserTokenService
    {
        private readonly UnitOfWork unitOfWork = new(context);

        public UserTokensDTO GetUserTokens(
            string username,
            string password)
        {
            string passwordHash = password.EncodeToBase64();
            UserTokens userToken = unitOfWork.Repository<UserTokens>()
                .Get(x => x.Username == username && x.PasswordHash == passwordHash)
                .FirstOrDefault();

            if (userToken != null)
            {
                ResetUserTokensAvailable(userToken);
                return ToUserTokensDTO(userToken);
            }

            return null;
        }

        public UserTokensDTO GetUserTokensById(
            Guid userId)
        {
            UserTokens userToken = unitOfWork.Repository<UserTokens>().GetByID(userId);
            return ToUserTokensDTO(userToken);
        }

        public List<UserTokensHistoryDTO> GetTokensHistoryByUserId(
            Guid userId)
        {
            List<UserTokensHistory> userTokensHistory = unitOfWork.Repository<UserTokensHistory>()
                .Get(x => x.UserTokensID == userId)
                .ToList();
            return userTokensHistory
                .Select(x => ToUserTokensHistoryDTO(x))
                .ToList();
        }

        private void ResetUserTokensAvailable(
            UserTokens userToken)
        {
            // Reset tokens available to user on the first day of the month
            if (DateTime.UtcNow.Day == 1)
            {
                int tokensAvailable = int.Parse(ConfigurationManager.GetValue("USER_TOKENS_AVAILABLE"));
                userToken.TokensAvailable = tokensAvailable;
                unitOfWork.Repository<UserTokens>().Update(userToken);
                unitOfWork.Save();
            }
        }

        public void UpdateTokenConsumption(
            Guid userId,
            int tokensConsumed)
        {
            DateTime currentDate = DateTime.UtcNow;
            UserTokensHistory userTokensHistory = unitOfWork.Repository<UserTokensHistory>()
                .Get(x => x.Year == currentDate.Year && x.Month == currentDate.Month && x.UserTokensID == userId)
                .FirstOrDefault();

            if (userTokensHistory != null)
            {
                userTokensHistory.TokensConsumed += tokensConsumed;
                userTokensHistory.RecordedAt = currentDate;
                unitOfWork.Repository<UserTokensHistory>().Update(userTokensHistory);
            }
            else
            {
                unitOfWork.Repository<UserTokensHistory>().Insert(new UserTokensHistory
                {
                    ID = Guid.NewGuid(),
                    UserTokensID = userId,
                    Year = currentDate.Year,
                    Month = currentDate.Month,
                    TokensConsumed = tokensConsumed,
                    RecordedAt = currentDate
                });
            }

            UserTokens userToken = unitOfWork.Repository<UserTokens>().GetByID(userId);
            userToken.TokensAvailable -= tokensConsumed;
            unitOfWork.Repository<UserTokens>().Update(userToken);
            unitOfWork.Save();
        }

        public static UserTokensDTO ToUserTokensDTO(UserTokens userToken) => new()
        {
            ID = userToken.ID,
            Username = userToken.Username,
            PasswordHash = userToken.PasswordHash,
            TokensAvailable = userToken.TokensAvailable
        };

        public static UserTokensHistoryDTO ToUserTokensHistoryDTO(UserTokensHistory userTokensHistory) => new()
        {
            ID = userTokensHistory.ID,
            Month = userTokensHistory.Month,
            Year = userTokensHistory.Year,
            TokensConsumed = userTokensHistory.TokensConsumed,
            RecordedAt = userTokensHistory.RecordedAt
        };
    }
}
