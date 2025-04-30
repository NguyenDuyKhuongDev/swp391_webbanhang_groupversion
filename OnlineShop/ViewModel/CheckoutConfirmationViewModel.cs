namespace OnlineShop.ViewModel
{
    public class CheckoutConfirmationViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
        public List<UserAddressViewModel> Addresses { get; set; }
        public int? SelectedAddressId { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}
