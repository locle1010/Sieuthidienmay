// Tạo file mới WebsiteBanHang/Services/InvoiceService.cs
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.EntityFrameworkCore;
using SelectPdf;
using System.Text;
using Web_dienmay.Models;

namespace Web_dienmay.Services
{
    public class InvoiceService
    {
        private readonly IConverter _converter;
        private readonly ApplicationDbContext _context;
        private readonly string _baseUrl;

        public InvoiceService(IConverter converter, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _converter = converter;
            _context = context;

            var request = httpContextAccessor.HttpContext.Request;
            _baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
        }

        public async Task<Order> GetOrderDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public string GenerateInvoiceHtml(Order order)
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8' />
                    <title>Hóa đơn - #{order.Id}</title>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            font-size: 14px;
                            line-height: 1.6;
                            color: #333;
                        }}
                        .invoice-box {{
                            max-width: 800px;
                            margin: auto;
                            padding: 30px;
                            border: 1px solid #eee;
                            box-shadow: 0 0 10px rgba(0, 0, 0, 0.15);
                        }}
                        .invoice-header {{
                            display: flex;
                            justify-content: space-between;
                            margin-bottom: 20px;
                            padding-bottom: 20px;
                            border-bottom: 1px solid #eee;
                        }}
                        .invoice-title {{
                            font-size: 28px;
                            font-weight: bold;
                            color: black;
                        }}
                        table {{
                            width: 100%;
                            border-collapse: collapse;
                        }}
                        table th, table td {{
                            padding: 10px;
                            border-bottom: 1px solid #eee;
                            text-align: left;
                        }}
                        table th {{
                            background-color: #f8f8f8;
                        }}
                        .text-right {{
                            text-align: right;
                        }}
                        .total-row {{
                            font-weight: bold;
                            font-size: 16px;
                        }}
                        .footers {{
                            margin-top: 30px;
                            padding-top: 20px;
                            border-top: 1px solid #eee;
                            text-align: center;
                            background-color: white;
                            color: black;
                        }}
                    </style>
                </head>
                <body>
                    <div class='invoice-box'>
                        <div class='invoice-header'>
                            <div>
                                <div class='invoice-title'>HÓA ĐƠN</div>
                                <div>Mã đơn hàng: #{order.Id}</div>
                                <div>Ngày đặt: {order.OrderDate.ToString("dd/MM/yyyy HH:mm")}</div>
                            </div>
                            <div>
                                <div>SIÊU THỊ ĐIỆN MÁY</div>
                                <div>Trụ ngoài, đường giữa, Quận Bình Nguyên Vô Tận, Thành phố Liên Quân</div>
                                <div>Email: letanlochhgg@gmail.com</div>
                                <div>Hotline: 1234 1234</div>
                            </div>
                        </div>
                        
                        <div>
                            <h3>Thông tin khách hàng:</h3>
                            <div>Họ tên: {order.ApplicationUser.FullName}</div>
                            <div>Email: {order.ApplicationUser.Email}</div>
                            <div>Địa chỉ giao hàng: {order.ShippingAddress}</div>
                        </div>
                        
                        <h3>Chi tiết đơn hàng:</h3>
                        <table>
                            <thead>
                                <tr>
                                    <th>Sản phẩm</th>
                                    <th class='text-right'>Đơn giá</th>
                                    <th class='text-right'>Số lượng</th>
                                    <th class='text-right'>Thành tiền</th>
                                </tr>
                            </thead>
                            <tbody>");

            foreach (var item in order.OrderDetails)
            {
                sb.AppendLine($@"
                                <tr>
                                    <td>{item.Product?.Name ?? $"Sản phẩm #{item.ProductId}"}</td>
                                    <td class='text-right'>{item.Price.ToString("N0")} ₫</td>
                                    <td class='text-right'>{item.Quantity}</td>
                                    <td class='text-right'>{(item.Price * item.Quantity).ToString("N0")} ₫</td>
                                </tr>");
            }

            sb.AppendLine($@"
                                <tr class='total-row'>
                                    <td colspan='3' class='text-right'>Tổng cộng:</td>
                                    <td class='text-right'>{order.TotalPrice.ToString("N0")} ₫</td>
                                </tr>
                            </tbody>
                        </table>
                        
                        <div>
                            <h3>Ghi chú:</h3>
                            <div>{(string.IsNullOrEmpty(order.Notes) ? "Không có ghi chú" : order.Notes)}</div>
                        </div>
                        
                        <div class='footers'>
                            <p>Cảm ơn quý khách đã mua hàng tại Siêu Thị Điện Máy!</p>
                            <p>Mọi thắc mắc xin vui lòng liên hệ hotline: 1234 1234</p>
                        </div>
                    </div>
                </body>
                </html>");

            return sb.ToString();
        }

        public byte[] GenerateInvoicePdf(Order order)
        {
            var htmlContent = GenerateInvoiceHtml(order);

            //Tạo HtmlToPdf
            HtmlToPdf converter = new HtmlToPdf();

            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            converter.Options.MarginLeft = 10;
            converter.Options.MarginRight = 10;
            converter.Options.MarginTop = 10;
            converter.Options.MarginBottom = 10;

            //đổi HTML sang PDF
            PdfDocument doc = converter.ConvertHtmlString(htmlContent);

            using (MemoryStream ms = new MemoryStream())
            {
                doc.Save(ms);
                doc.Close();
                return ms.ToArray();
            }
        }
    }
}
