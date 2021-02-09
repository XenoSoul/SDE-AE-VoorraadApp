namespace SDE_AE_VoorraadApp.Models
{
    public class Order
    {
        public int ID { get; set; }
        public int OrderListID { get; set; }
        public int ProductStockID { get; set; }

        public string ProductName { get; set; }
        public int SnapRefillAdviceCount { get; set; }
        public int SnapAvailableCount { get; set; }
        public int SnapMaxCount { get; set; }
        public float Priority { get; set; }

        public ProductStock ProductStock { get; set; }
        public OrderList OrderList { get; set; }
    }
}