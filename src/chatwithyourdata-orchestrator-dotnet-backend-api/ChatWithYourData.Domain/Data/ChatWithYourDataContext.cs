namespace ChatWithYourData.Domain.Data
{
    using ChatWithYourData.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public class ChatWithYourDataContext : DbContext
    {
        public ChatWithYourDataContext() { }

        public ChatWithYourDataContext(DbContextOptions<ChatWithYourDataContext> options) : base(options) { }

        public DbSet<UserTokens> UserTokens { get; set; }

        public DbSet<UserTokensHistory> UserTokensHistory { get; set; }
    }
}
