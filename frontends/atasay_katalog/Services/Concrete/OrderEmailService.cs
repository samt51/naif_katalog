using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using atasay_katalog.Models;
using atasay_katalog.Services.Abstract;

namespace atasay_katalog.Services.Concrete
{
    public class OrderEmailService : IOrderEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderEmailService> _logger;

        public OrderEmailService(IConfiguration configuration, ILogger<OrderEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendNewOrderAsync(OrderRecord order, CancellationToken cancellationToken = default)
        {
            var to = _configuration["Email:OrderNotifyTo"];
            var host = _configuration["Email:Host"];
            var user = _configuration["Email:User"];
            var password = (_configuration["Email:Password"] ?? "").Replace(" ", "");
            var from = _configuration["Email:From"] ?? user;
            var fromName = _configuration["Email:FromName"] ?? "ATASAY";

            if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Sipariş maili gönderilemedi: Email ayarları eksik.");
                return false;
            }

            var port = _configuration.GetValue("Email:Port", 587);
            var orderNo = order.OrderNumber;
            var html = BuildHtml(order, orderNo);

            using var message = new MailMessage
            {
                From = new MailAddress(from, fromName, Encoding.UTF8),
                Subject = $"Yeni Sipariş • {orderNo}",
                SubjectEncoding = Encoding.UTF8,
                Body = html,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };
            message.To.Add(to);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = _configuration.GetValue("Email:EnableSsl", true),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(user, password),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            try
            {
                await client.SendMailAsync(message, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş maili gönderilemedi.");
                return false;
            }
        }

        private static string BuildHtml(OrderRecord order, string orderNo)
        {
            var enc = HtmlEncoder.Default;
            var company = Display(order.CompanyName);
            var customer = string.Join(" ", new[] { order.FirstName, order.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
            var phone = Display(order.PhoneNumber);
            var account = Display(string.Join(" ", new[] { order.AccountCompany, order.AccountName }.Where(x => !string.IsNullOrWhiteSpace(x))));
            var accountMail = Display(order.AccountEmail);
            var when = DateTime.Now.ToString("dd.MM.yyyy HH:mm", new CultureInfo("tr-TR"));
            var items = order.Items ?? new List<ConfirmOrderItem>();
            var totalQty = items.Sum(i => i.Quantity > 0 ? i.Quantity : 1);

            var rows = new StringBuilder();
            if (items.Count == 0)
            {
                rows.Append("<tr><td colspan='5' style='padding:16px;color:#64748b;text-align:center;'>Ürün bulunamadı.</td></tr>");
            }
            else
            {
                foreach (var item in items)
                {
                    var qty = item.Quantity > 0 ? item.Quantity : 1;
                    var specs = string.Join(" · ", new[] { item.Ayar, item.Renk, item.Gram }.Where(x => !string.IsNullOrWhiteSpace(x)).Select(Display));
                    var stones = BuildStones(item.Stones);
                    var note = string.IsNullOrWhiteSpace(item.Note) ? "" : $"<div style='margin-top:6px;color:#92400e;font-size:12px;'>Not: {enc.Encode(item.Note.Trim())}</div>";
                    rows.Append($@"
<tr>
  <td style='padding:14px 12px;border-bottom:1px solid #edf2f7;vertical-align:top;'>
    <div style='font-weight:800;color:#0f172a;letter-spacing:.3px;'>{enc.Encode(item.Code ?? "-")}</div>
    <div style='color:#64748b;font-size:12px;margin-top:3px;'>{enc.Encode(item.Category ?? "-")}</div>
    {(string.IsNullOrWhiteSpace(specs) ? "" : $"<div style='color:#475569;font-size:12px;margin-top:4px;'>{specs}</div>")}
    {stones}{note}
  </td>
  <td style='padding:14px 12px;border-bottom:1px solid #edf2f7;text-align:center;vertical-align:top;font-weight:700;color:#0f172a;'>{qty}</td>
  <td style='padding:14px 12px;border-bottom:1px solid #edf2f7;text-align:right;vertical-align:top;font-weight:800;color:#047857;white-space:nowrap;'>{enc.Encode(item.Price ?? "-")}</td>
</tr>");
                }
            }

            return $@"<!DOCTYPE html>
<html lang='tr'>
<head><meta charset='utf-8'><meta name='viewport' content='width=device-width, initial-scale=1'></head>
<body style='margin:0;padding:0;background:#f4f1ec;font-family:Arial,Helvetica,sans-serif;color:#0f172a;'>
  <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='background:#f4f1ec;padding:24px 12px;'>
    <tr><td align='center'>
      <table role='presentation' width='640' cellpadding='0' cellspacing='0' style='max-width:640px;width:100%;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 12px 40px rgba(15,23,42,.08);'>
        <tr>
          <td style='background:#0f172a;padding:28px 28px 24px;'>
            <div style='color:#c5a572;font-size:12px;letter-spacing:3px;font-weight:700;text-transform:uppercase;'>ATASAY</div>
            <div style='color:#ffffff;font-size:26px;font-weight:800;margin-top:8px;'>Yeni Sipariş</div>
            <div style='color:#94a3b8;font-size:13px;margin-top:6px;'>{enc.Encode(orderNo)} · {enc.Encode(when)}</div>
          </td>
        </tr>
        <tr>
          <td style='padding:24px 28px 8px;'>
            <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='background:#f8fafc;border:1px solid #eef2f6;border-radius:14px;'>
              <tr>
                <td style='padding:16px 18px;'>
                  <div style='font-size:11px;letter-spacing:1.2px;text-transform:uppercase;color:#64748b;font-weight:800;margin-bottom:10px;'>Müşteri Bilgileri</div>
                  <table role='presentation' width='100%' cellpadding='0' cellspacing='0'>
                    <tr>
                      <td style='padding:4px 0;color:#64748b;font-size:13px;width:130px;'>Firma</td>
                      <td style='padding:4px 0;color:#0f172a;font-size:13px;font-weight:700;'>{company}</td>
                    </tr>
                    <tr>
                      <td style='padding:4px 0;color:#64748b;font-size:13px;'>Ad Soyad</td>
                      <td style='padding:4px 0;color:#0f172a;font-size:13px;font-weight:700;'>{enc.Encode(string.IsNullOrWhiteSpace(customer) ? "-" : customer)}</td>
                    </tr>
                    <tr>
                      <td style='padding:4px 0;color:#64748b;font-size:13px;'>Telefon</td>
                      <td style='padding:4px 0;color:#0f172a;font-size:13px;font-weight:700;'>{phone}</td>
                    </tr>
                    <tr>
                      <td style='padding:4px 0;color:#64748b;font-size:13px;'>Siparişi veren hesap</td>
                      <td style='padding:4px 0;color:#0f172a;font-size:13px;font-weight:700;'>{account} {(accountMail == "-" ? "" : $"({accountMail})")}</td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
          </td>
        </tr>
        <tr>
          <td style='padding:16px 28px 8px;'>
            <div style='font-size:11px;letter-spacing:1.2px;text-transform:uppercase;color:#64748b;font-weight:800;margin-bottom:10px;'>Sipariş Kalemleri · {totalQty} adet</div>
            <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='border:1px solid #eef2f6;border-radius:14px;overflow:hidden;'>
              <tr style='background:#0f172a;'>
                <th align='left' style='padding:10px 12px;color:#e2e8f0;font-size:11px;letter-spacing:.8px;text-transform:uppercase;'>Ürün</th>
                <th align='center' style='padding:10px 12px;color:#e2e8f0;font-size:11px;letter-spacing:.8px;text-transform:uppercase;'>Adet</th>
                <th align='right' style='padding:10px 12px;color:#e2e8f0;font-size:11px;letter-spacing:.8px;text-transform:uppercase;'>Fiyat</th>
              </tr>
              {rows}
            </table>
          </td>
        </tr>
        <tr>
          <td style='padding:18px 28px 28px;color:#94a3b8;font-size:12px;line-height:1.5;'>
            Bu mail, B2B katalog üzerinden onaylanan yeni bir sipariş için otomatik gönderilmiştir.
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";
        }

        private static string BuildStones(List<ConfirmOrderStone>? stones)
        {
            if (stones == null || stones.Count == 0) return "";
            var enc = HtmlEncoder.Default;
            var sb = new StringBuilder("<div style='margin-top:8px;'>");
            foreach (var stone in stones)
            {
                var title = string.Join(" · ", new[] { stone.Type, stone.Clarity }.Where(x => !string.IsNullOrWhiteSpace(x)));
                var metaParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(stone.Color)) metaParts.Add("Renk: " + stone.Color.Trim());
                if (!string.IsNullOrWhiteSpace(stone.Quantity)) metaParts.Add("Adet: " + stone.Quantity.Trim());
                if (!string.IsNullOrWhiteSpace(stone.TotalCarat)) metaParts.Add(stone.TotalCarat.Trim());
                var meta = string.Join(" · ", metaParts);
                sb.Append("<div style='font-size:12px;color:#334155;background:#fff;border:1px solid #edf2f7;border-radius:8px;padding:6px 8px;margin-top:4px;'><strong>"
                    + enc.Encode(string.IsNullOrWhiteSpace(title) ? "-" : title)
                    + "</strong>"
                    + (string.IsNullOrWhiteSpace(meta) ? "" : "<span style='color:#64748b;'> — " + enc.Encode(meta) + "</span>")
                    + "</div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        private static string Display(string? value)
        {
            var text = string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
            return HtmlEncoder.Default.Encode(text);
        }
    }
}
