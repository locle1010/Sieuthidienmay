namespace WebsiteBanHang.Services
{
    public class EmailTemplateService
    {
        private readonly string _websiteName = "Siêu thị điện máy";
        private readonly string _websiteUrl = "http://localhost:25576/";

        public string GetEmailTemplate(string title, string bodyContent, string buttonText = null, string buttonUrl = null)
        {
            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #e0f2fe 0%, #f0f9ff 100%);
            padding: 20px;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background: #ffffff;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 10px 30px rgba(131, 164, 212, 0.2);
        }}
        .email-header {{
            background: linear-gradient(135deg, #83a4d4 0%, #b6fbff 100%);
            padding: 40px 30px;
            text-align: center;
        }}
        .logo {{
            width: 80px;
            height: 80px;
            background: white;
            border-radius: 50%;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 20px;
            box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
        }}
        .logo-text {{
            font-size: 32px;
            color: #83a4d4;
            font-weight: bold;
        }}
        .email-header h1 {{
            color: white;
            font-size: 28px;
            margin: 0;
            font-weight: 700;
            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }}
        .email-body {{
            padding: 40px 30px;
        }}
        .email-body h2 {{
            color: #1e293b;
            font-size: 24px;
            margin-bottom: 20px;
            font-weight: 700;
        }}
        .email-body p {{
            color: #64748b;
            font-size: 16px;
            line-height: 1.6;
            margin-bottom: 20px;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .email-button {{
            display: inline-block;
            padding: 16px 40px;
            background: linear-gradient(135deg, #83a4d4 0%, #b6fbff 100%);
            color: white !important;
            text-decoration: none;
            border-radius: 50px;
            font-weight: 700;
            font-size: 16px;
            box-shadow: 0 8px 20px rgba(131, 164, 212, 0.3);
            transition: all 0.3s ease;
        }}
        .email-button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 12px 30px rgba(131, 164, 212, 0.4);
        }}
        .info-box {{
            background: linear-gradient(135deg, #e0f2fe 0%, #f0f9ff 100%);
            border-left: 4px solid #83a4d4;
            padding: 20px;
            border-radius: 12px;
            margin: 20px 0;
        }}
        .info-box p {{
            margin: 0;
            color: #1e3a5f;
        }}
        .warning-box {{
            background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
            border-left: 4px solid #fbbf24;
            padding: 20px;
            border-radius: 12px;
            margin: 20px 0;
        }}
        .warning-box p {{
            margin: 0;
            color: #92400e;
        }}
        .email-footer {{
            background: linear-gradient(135deg, #1e3a5f 0%, #2c5282 100%);
            padding: 30px;
            text-align: center;
            color: rgba(255, 255, 255, 0.8);
        }}
        .email-footer p {{
            margin: 5px 0;
            font-size: 14px;
            color: rgba(255, 255, 255, 0.8);
        }}
        .social-links {{
            margin: 20px 0;
        }}
        .social-links a {{
            display: inline-block;
            width: 36px;
            height: 36px;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 50%;
            margin: 0 5px;
            text-decoration: none;
            color: white;
            line-height: 36px;
        }}
        .divider {{
            height: 1px;
            background: linear-gradient(90deg, transparent, #e0f2fe, transparent);
            margin: 30px 0;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <h1>{_websiteName}</h1>
        </div>
        
        <div class='email-body'>
            <h2>{title}</h2>
            {bodyContent}
            {(buttonText != null && buttonUrl != null ? $@"
            <div class='button-container'>
                <a href='{buttonUrl}' class='email-button'>{buttonText}</a>
            </div>" : "")}
        </div>
        
        <div class='email-footer'>
            <p><strong>{_websiteName}</strong></p>
            <p>Địa chỉ: 123 Đường ABC, Quận XYZ, TP. Hồ Chí Minh</p>
            <p>Hotline: 1900-xxxx | Email: support@sieuthidienmay.com</p>
            <div class='divider'></div>
            <p style='font-size: 12px;'>
                Email này được gửi tự động, vui lòng không trả lời trực tiếp.<br/>
                © 2025 {_websiteName}. All rights reserved.
            </p>
        </div>
    </div>
</body>
</html>";
        }

        public string GetConfirmEmailTemplate(string userName, string confirmationUrl)
        {
            var bodyContent = $@"
                <p>Xin chào <strong>{userName}</strong>,</p>
                <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>{_websiteName}</strong>! Chúng tôi rất vui mừng chào đón bạn.</p>
                
                <div class='info-box'>
                    <p><strong>Để Chào mừng đến với cộng đồng của chúng tôi!</strong></p>
                    <p>Để hoàn tất quá trình đăng ký và bắt đầu mua sắm, vui lòng xác nhận địa chỉ email của bạn bằng cách nhấn vào nút bên dưới.</p>
                </div>
                
                <p>Nếu bạn không thực hiện đăng ký này, vui lòng bỏ qua email này.</p>
                
                <div class='warning-box'>
                    <p><strong>Để Lưu ý bảo mật:</strong></p>
                    <p>Link xác nhận này chỉ có hiệu lực trong 24 giờ. Không chia sẻ link này với bất kỳ ai.</p>
                </div>";

            return GetEmailTemplate(
                "Xác nhận địa chỉ Email",
                bodyContent,
                "Để Xác nhận Email ngay",
                confirmationUrl
            );
        }

        public string GetResetPasswordTemplate(string userName, string resetUrl)
        {
            var bodyContent = $@"
                <p>Xin chào <strong>{userName}</strong>,</p>
                <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn tại <strong>{_websiteName}</strong>.</p>
                
                <div class='info-box'>
                    <p><strong>Để đặt lại mật khẩu</strong></p>
                    <p>Nhấn vào nút bên dưới để tạo mật khẩu mới cho tài khoản của bạn. Link này chỉ có hiệu lực trong <strong>1 giờ</strong>.</p>
                </div>
                
                <div class='warning-box'>
                    <p><strong>Để Bạn không yêu cầu đặt lại mật khẩu?</strong></p>
                    <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email và liên hệ với chúng tôi ngay để bảo vệ tài khoản.</p>
                </div>
                
                <p style='color: #64748b; font-size: 14px;'>Để bảo mật tài khoản, không chia sẻ link này với bất kỳ ai.</p>";

            return GetEmailTemplate(
                "Đặt lại mật khẩu",
                bodyContent,
                "Đặt lại mật khẩu",
                resetUrl
            );
        }

        public string GetChangeEmailConfirmationTemplate(string userName, string newEmail, string confirmationUrl)
        {
            var bodyContent = $@"
                <p>Xin chào <strong>{userName}</strong>,</p>
                <p>Bạn đã yêu cầu thay đổi địa chỉ email của tài khoản tại <strong>{_websiteName}</strong>.</p>
                
                <div class='info-box'>
                    <p><strong>Để Email mới:</strong> {newEmail}</p>
                    <p>Để hoàn tất việc thay đổi email, vui lòng xác nhận bằng cách nhấn vào nút bên dưới.</p>
                </div>
                
                <div class='warning-box'>
                    <p><strong>Để Bạn không yêu cầu thay đổi email?</strong></p>
                    <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email và đổi mật khẩu ngay để bảo vệ tài khoản.</p>
                </div>";

            return GetEmailTemplate(
                "Xác nhận thay đổi Email",
                bodyContent,
                "Xác nhận Email mới",
                confirmationUrl
            );
        }

        public string GetWelcomeEmailTemplate(string userName)
        {
            var bodyContent = $@"
                <p>Xin chào <strong>{userName}</strong>,</p>
                <p>Để <strong>Chào mừng bạn đến với {_websiteName}!</strong> Để</p>
                
                <p>Tài khoản của bạn đã được kích hoạt thành công. Bây giờ bạn có thể:</p>
                
                <div class='info-box'>
                    <p><strong>Để Những gì bạn có thể làm:</strong></p>
                    <ul style='margin: 10px 0; padding-left: 20px;'>
                        <li style='margin: 8px 0;'>Mua sắm hàng nghìn sản phẩm điện máy chính hãng</li>
                        <li style='margin: 8px 0;'>Nhận thông báo về các chương trình khuyến mãi đặc biệt</li>
                        <li style='margin: 8px 0;'>Theo dõi đơn hàng và lịch sử mua hàng</li>
                        <li style='margin: 8px 0;'>Đánh giá sản phẩm và chia sẻ trải nghiệm</li>
                    </ul>
                </div>
                
                <div style='background: linear-gradient(135deg, #e0f2fe 0%, #f0f9ff 100%); padding: 20px; border-radius: 12px; margin: 20px 0; text-align: center;'>
                    <p style='margin: 0; color: #1e293b; font-size: 18px; font-weight: bold;'>Để Ưu đãi đặc biệt cho thành viên mới!</p>
                    <p style='margin: 10px 0; color: #64748b;'>Giảm ngay <strong style='color: #83a4d4; font-size: 24px;'>10%</strong> cho đơn hàng đầu tiên</p>
                    <p style='margin: 0; color: #64748b; font-size: 14px;'>Mã giảm giá: <strong style='color: #4fc3f7;'>WELCOME10</strong></p>
                </div>
                
                <p>Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi qua hotline <strong>1900-xxxx</strong> hoặc email <strong>support@dienmayshop.com</strong>.</p>
                
                <p style='margin-top: 30px;'>Chúc bạn có trải nghiệm mua sắm tuyệt vời! Để</p>";

            return GetEmailTemplate(
                "Chào mừng bạn đến với " + _websiteName,
                bodyContent,
                "Để Bắt đầu mua sắm",
                _websiteUrl
            );
        }

        public string GetOrderConfirmationTemplate(string userName, string orderNumber, decimal totalAmount)
        {
            var bodyContent = $@"
                <p>Xin chào <strong>{userName}</strong>,</p>
                <p>Cảm ơn bạn đã đặt hàng tại <strong>{_websiteName}</strong>!</p>
                
                <div class='info-box'>
                    <p><strong>Để Thông tin đơn hàng:</strong></p>
                    <p>Mã đơn hàng: <strong>{orderNumber}</strong></p>
                    <p>Tổng tiền: <strong style='color: #83a4d4; font-size: 20px;'>{totalAmount:N0} đ</strong></p>
                </div>
                
                <p>Đơn hàng của bạn đang được xử lý. Chúng tôi sẽ thông báo cho bạn ngay khi đơn hàng được giao cho đơn vị vận chuyển.</p>
                
                <p>Bạn có thể theo dõi tình trạng đơn hàng bằng cách đăng nhập vào tài khoản và truy cập mục <strong>Đơn hàng của tôi</strong>.</p>";

            return GetEmailTemplate(
                "Xác nhận đơn hàng #" + orderNumber,
                bodyContent,
                "Để Xem chi tiết đơn hàng",
                _websiteUrl + "/Order"
            );
        }
    }
}