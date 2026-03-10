using System.Security.Cryptography;
using System.Text;

namespace QLTaiChinh.Helper
{
    public class HashHelper
    {
        public static string GetMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                // Chuyển chuỗi thành mảng byte
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);

                // Tính hash MD5
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Chuyển sang chuỗi hex
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
