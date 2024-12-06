namespace Cursus.Data.DTO
{
    public class WalletDTO
    {
        public string UserName { get; set; } = string.Empty;
        public double Balance { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
