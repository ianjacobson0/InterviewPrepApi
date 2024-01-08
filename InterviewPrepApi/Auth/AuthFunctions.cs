using CryptSharp;
using CryptSharp.Utility;
using System.Text;

namespace InterviewPrepApi.Auth
{
	public static class AuthFunctions
	{
		public static string EncryptPassword(string password, string salt, int cost = 16384, int blockSize = 8, int parallel = 1, int size = 32)
		{
			byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
			byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
			byte[] hashedBytes = SCrypt.ComputeDerivedKey(passwordBytes, saltBytes, cost, blockSize, parallel, null, size);
			return (Convert.ToHexString(hashedBytes) + $"|{cost}|{blockSize}|{parallel}").ToLower();
		}
	}
}
