namespace Web_dienmay.Models
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Company = "Company";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";
        // Payment Methods
        public const string PaymentMethod_COD = "COD";
        public const string PaymentMethod_Banking = "Banking";
        public const string PaymentMethod_MoMo = "MoMo";

        // Payment Status
        public const string PaymentStatus_Pending = "Chưa thanh toán";
        public const string PaymentStatus_Approved = "Đã thanh toán";
        public const string PaymentStatus_Rejected = "Từ chối";
    }
}
