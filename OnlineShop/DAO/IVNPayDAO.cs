namespace OnlineShop.DAO
{
    public interface IVNPayDAO
    {
        string CreatePaymentUrl(decimal amount, string orderId, string orderInfo, string ipAddress);
        bool ValidateResponse(Dictionary<string, string> queryParams);
    }
}
