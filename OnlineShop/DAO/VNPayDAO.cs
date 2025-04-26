using System.Security.Cryptography;
using System.Text;

namespace OnlineShop.DAO
{
    public class VNPayDAO : IVNPayDAO
    {
        private static readonly string _vnpTmnCode = "P8XP9B1K";
        private static readonly string _vnpHashSecret = "9773CZVDHU81K3SMNBTFXVQVYY4TC73T";
        private static readonly string _vnpUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        private static readonly string _vnpReturnUrl = "https://eee1-42-113-163-139.ngrok-free.app/Cart/Return"; // URL callback từ ngrok


        public string CreatePaymentUrl(decimal amount, string orderId, string orderInfo, string ipAddress)
        {
            DateTime vnTime;
            try
            {
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            }
            catch
            {
                vnTime = DateTime.UtcNow.AddHours(7);
            }

            var vnpParams = new Dictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", _vnpTmnCode },
                { "vnp_Amount", ((int)(amount * 100)).ToString() },
                { "vnp_CreateDate", vnTime.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", ipAddress },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", orderInfo },
                { "vnp_OrderType", "topup" },
                { "vnp_ReturnUrl", _vnpReturnUrl },
                { "vnp_TxnRef", orderId },
            };

            var sortedParams = vnpParams.OrderBy(k => k.Key).ToDictionary(k => k.Key, k => k.Value);
            var signData = string.Join("&", sortedParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var vnpSecureHash = HmacSha512(_vnpHashSecret, signData);

            Console.WriteLine("SignData: " + signData);
            Console.WriteLine("SecureHash: " + vnpSecureHash);

            var paymentUrl = $"{_vnpUrl}?{signData}&vnp_SecureHash={vnpSecureHash}";
            return paymentUrl;
        }

        public bool ValidateResponse(Dictionary<string, string> queryParams)
        {
            if (!queryParams.ContainsKey("vnp_SecureHash"))
                return false;

            var secureHash = queryParams["vnp_SecureHash"];
            queryParams.Remove("vnp_SecureHash");
            queryParams.Remove("vnp_SecureHashType");

            var sortedParams = queryParams.OrderBy(k => k.Key).ToDictionary(k => k.Key, k => k.Value);
            var signData = string.Join("&", sortedParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var computedHash = HmacSha512(_vnpHashSecret, signData);

            return secureHash.Equals(computedHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSha512(string key, string input)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(input);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
