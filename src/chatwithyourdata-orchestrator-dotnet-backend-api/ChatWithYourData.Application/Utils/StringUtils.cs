namespace ChatWithYourData.Application.Utils
{
    using System.Security.Cryptography;
    using System.Text;

    public static class StringUtils
    {
        public static string EncodeToBase64(this string toEncode, Encoding encoding = null)
        {
            return encoding == null
                ? Convert.ToBase64String(Encoding.UTF8.GetBytes(toEncode))
                : Convert.ToBase64String(encoding.GetBytes(toEncode));
        }

        public static Guid ToGuid(this string code)
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(code);
            byte[] hashBytes = MD5.HashData(inputBytes);
            return new Guid(hashBytes);
        }
    }
}