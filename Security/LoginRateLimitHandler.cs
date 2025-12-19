using JobConnect.Interfaces;
using JobConnect.Repositories;
using JobConnect.Services;
using Microsoft.AspNetCore.RateLimiting;

public static class LoginRateLimitHandler
{
    public static async Task Handle(OnRejectedContext context)
    {
        var http = context.HttpContext;
        var services = http.RequestServices;

        if (!http.Request.HasFormContentType)
            return;

        var email = http.Request.Form["Email"].ToString();
        if (string.IsNullOrEmpty(email))
            return;

        var userRepo = services.GetRequiredService<IUserRepository>();
        var emailService = services.GetRequiredService<IEmailService>();
        var config = services.GetRequiredService<IConfiguration>();

        var user = await userRepo.GetByEmailAsync(email);
        if (user == null || user.Role == "Admin")  
            return;

        user.ResetToken = Guid.NewGuid().ToString();
        user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);  

        await userRepo.UpdateAsync(user);

        var resetLink = $"{config["AppUrl"]}/Accounts/ResetPassword?token={user.ResetToken}";

        string body = $@"
            <h3>Security Alert</h3>
            <p>Multiple failed login attempts detected on your account.</p>
            <p>For security reasons, we recommend changing your password.</p>
            <p>
                <a href='{resetLink}'
                   style='padding:10px 16px;
                   background:#092635;
                   color:white;
                   border-radius:6px;
                   text-decoration:none;
                   font-weight:600;'>
                   Change Password
                </a>
            </p>
            <small>If this was you, you can safely ignore this email.</small>
        ";

        await emailService.SendSimpleEmailAsync(
            user.Email,
            "Security Alert – Reset Your Password",
            body
        );
    }
}

