using OnlineShop.Models;
using System.Net;
using System.Net.Mail;

namespace OnlineShop.Services
{
    public interface IEmailService
    {
        Task SendOrderStatusUpdateEmail(Order order);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOrderStatusUpdateEmail(Order order)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpClient = new SmtpClient
            {
                Host = emailSettings["SmtpServer"]!,
                Port = int.Parse(emailSettings["SmtpPort"]!),
                EnableSsl = true,
                Credentials = new NetworkCredential(emailSettings["SenderEmail"], emailSettings["SenderPassword"])
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings["SenderEmail"]!, "Online Shop"),
                Subject = $"Order Status Update - {order.OrderId}",
                Body = GenerateEmailBody(order),
                IsBodyHtml = true,
                To = { order.User.Email }
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

        private string GenerateEmailBody(Order order)
        {
            var statusMessage = order.Status switch
            {
                "Processing" => "Your order is being processed.",
                "Shipping" => "Your order is out for delivery.",
                "Completed" => "Your order has been delivered successfully.",
                "Cancelled" => "Your order has been cancelled.",
                _ => "Your order status has been updated."
            };

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #333;'>Order Status Update</h2>
                    <p>Dear {order.User.UserName},</p>
                    <p><strong>{statusMessage}</strong></p>
                    
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <h3 style='color: #333; margin-top: 0;'>Order Details</h3>
                        <p><strong>Order ID:</strong> {order.OrderId}</p>
                        <p><strong>Status:</strong> {order.Status}</p>
                        <p><strong>Order Date:</strong> {order.CreatedDate:dd/MM/yyyy HH:mm}</p>
                        <p><strong>Total Amount:</strong> {order.Amount:C}</p>
                    </div>";

            if (order.Status == "Cancelled" )
            {
                body += $@"
                    <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='color: #856404; margin: 0;'><strong>Cancellation Reason:</strong></p>
                        <p style='color: #856404; margin: 10px 0 0 0;'>Cancel order</p>
                    </div>";
            }

            body += @"
                    <p>Thank you for shopping with us!</p>
                    <p style='color: #666;'>Best regards,<br>Your Online Shop Team</p>
                </div>";

            return body;
        }
    }
}