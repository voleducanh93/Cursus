namespace Cursus.Data.DTO
{
    public class CartDTO
    {
        public string UserId { get; set; }

        public double Total { get; set; }
        public List<CartItemsDTO> CartItems { get; set; }
    }
}