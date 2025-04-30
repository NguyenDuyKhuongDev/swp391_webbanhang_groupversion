using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public class UserAddressDAO : IUserAddressDAO
    {
        private readonly ApplicationDbContext _dbContext;

        public UserAddressDAO(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<UserAddress>> GetAddressesByUserIdAsync(string userId)
        {
            return await _dbContext.UserAddresses
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<UserAddress> GetAddressByIdAsync(int addressId, string userId)
        {
            return await _dbContext.UserAddresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
        }

        public async Task ClearDefaultAddressAsync(string userId)
        {
            var addresses = await _dbContext.UserAddresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();
            foreach (var address in addresses)
            {
                address.IsDefault = false;
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddAddressAsync(UserAddress address)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (address.IsDefault)
                {
                    await ResetDefaultAddress(address.UserId);
                }

                _dbContext.UserAddresses.Add(address);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAddressAsync(UserAddress address)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var existingAddress = await _dbContext.UserAddresses
                    .FirstOrDefaultAsync(a => a.Id == address.Id && a.UserId == address.UserId);

                if (existingAddress == null)
                {
                    throw new KeyNotFoundException("Address not found.");
                }

                if (address.IsDefault)
                {
                    await ResetDefaultAddress(address.UserId, address.Id);
                }

                existingAddress.Address = address.Address;
                existingAddress.AddressDetail = address.AddressDetail;
                existingAddress.IsDefault = address.IsDefault;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAddressAsync(int addressId, string userId)
        {
            var address = await GetAddressByIdAsync(addressId, userId);
            if (address != null)
            {
                _dbContext.UserAddresses.Remove(address);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task ResetDefaultAddress(string userId, int? excludeId = null)
        {
            var query = _dbContext.UserAddresses
                .Where(a => a.UserId == userId && a.IsDefault);

            if (excludeId.HasValue)
            {
                query = query.Where(a => a.Id != excludeId.Value);
            }

            var defaultAddresses = await query.ToListAsync();
            foreach (var addr in defaultAddresses)
            {
                addr.IsDefault = false;
            }
        }


    }
}
