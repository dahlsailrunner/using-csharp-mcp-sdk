using System.Net.Mail;
using MailKit.Client;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace CarvedRock.WebApp;

public class EmailService(MailKitClientFactory factory): IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var client = await factory.GetSmtpClientAsync(); 

        using var message = new MailMessage
        {
            Body = htmlMessage,
            Subject = subject,
            IsBodyHtml = true,
            From = new MailAddress("e-commerce@carvedrock.com", "Carved Rock Shop"),
            To = { email }
        };

        await client.SendAsync(MimeMessage.CreateFromMailMessage(message));
    }
}
