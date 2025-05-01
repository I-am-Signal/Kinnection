using System.Security.Cryptography;
using System.Text;

namespace Kinnection
{
    public static class PassForge
    {
        /// <summary>
        /// Byte encoded key used for hashing.
        /// </summary>
        private static readonly byte[] KEY = GetKey();

        /// <summary>
        /// Gets and returns the byte encoded key string.
        /// </summary>
        /// <returns>byte[]</returns>
        /// <exception cref="Exception"></exception>
        private static byte[] GetKey()
        {
            string RawKey = Environment.GetEnvironmentVariable("KEY")!;
            if (string.IsNullOrEmpty(RawKey))
                throw new Exception("KEY environment variable is null or empty!");
            return Encoding.UTF8.GetBytes(RawKey);
        }

        /// <summary>
        /// Returns the hashed form of the 'Pass' parameter.
        /// </summary>
        /// <param name="Pass"></param>
        /// <returns></returns>
        public static string HashPass(string Pass)
        {
            try
            {
                using var HMAC = new HMACSHA256(KEY);
                return Convert.ToBase64String(
                    HMAC.ComputeHash(
                        Encoding.UTF8.GetBytes(Pass)));
            }
            catch (Exception)
            { throw new ArgumentException($"Invalid string \"{Pass}\" was provided."); }
        }

        /// <summary>
        /// Checks if the password is correct.
        /// </summary>
        /// <param name="Pass"></param>
        /// <param name="UserID"></param>
        /// <returns>true if passed the correct password for the user, false otherwise.</returns>
        public static bool IsPassCorrect(string Pass, int UserID)
        {
            // Get pass hash
            using var Context = DatabaseManager.GetActiveContext();
            string PassHash = Context.Passwords
                .OrderByDescending(p => p.Created)
                .First(p => p.User.ID == UserID).PassString;

            try
            {
                // Check hash equivalence
                return CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(HashPass(Pass)),
                    Encoding.UTF8.GetBytes(PassHash)
                );
            }
            catch (Exception) { return false; }
        }
    }
}