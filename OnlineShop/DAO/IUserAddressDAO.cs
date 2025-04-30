using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public interface IUserAddressDAO
    {
        Task<List<UserAddress>> GetAddressesByUserIdAsync(string userId);
        Task<UserAddress> GetAddressByIdAsync(int addressId, string userId);
        Task AddAddressAsync(UserAddress address);
        Task UpdateAddressAsync(UserAddress address);
        Task DeleteAddressAsync(int addressId, string userId);
        Task ClearDefaultAddressAsync(string userId);
    }
}
