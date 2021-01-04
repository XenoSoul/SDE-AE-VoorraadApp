namespace SDE_AE_VoorraadApp.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }

        public ProductStock ProductStock { get; set; }
        public OrderList OrderList { get; set; }
    }
}