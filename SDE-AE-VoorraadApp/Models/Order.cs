namespace SDE_AE_VoorraadApp.Models
{
    public class Order
    {
        public int ID { get; set; }
        public int OrderListID { get; set; }
        public int ProductStockID { get; set; }
        public float Priority { get; set; }
        public int Quantity { get; set; }

        public ProductStock ProductStock { get; set; }
        public OrderList OrderList { get; set; }
    }
}