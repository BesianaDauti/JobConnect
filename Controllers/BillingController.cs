using JobConnect.Repositories;
using JobConnect.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

public class BillingController : Controller
{
    private readonly IConfiguration _config;
    private readonly IEmployerRepository _employerRepo;
    private readonly IEmailService _emailService;

    public BillingController(
        IConfiguration config,
        IEmployerRepository employerRepo, IEmailService emailService)
    {
        _config = config;
        _employerRepo = employerRepo;
        _emailService = emailService;
    }
    private IActionResult EnsureEmployer()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("Role");

        if (userId == null || role != "Employer")
            return RedirectToAction("Login", "Accounts");

        return null;
    }
    public async Task<IActionResult> Plans()
    {
        var auth = EnsureEmployer();
        if (auth != null) return auth;

        var userId = HttpContext.Session.GetInt32("UserId").Value;
        var employer = await _employerRepo.GetEmployerByUserIdAsync(userId);
        return View(employer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCheckoutSession()
    {
        var auth = EnsureEmployer();
        if (auth != null) return auth;

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Accounts");

        var employer = await _employerRepo.GetEmployerByUserIdAsync(userId.Value);

        var domain = $"{Request.Scheme}://{Request.Host}";

        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            SuccessUrl = domain + "/Billing/Success?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = domain + "/Billing/Plans",
            LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                Price = _config["Stripe:PaidPlanPriceId"],
                Quantity = 1
            }
        },
            CustomerEmail = employer.User.Email
        };

        var service = new SessionService();
        var session = service.Create(options);

        return Redirect(session.Url);
    }
    public async Task<IActionResult> Success(string session_id)
    {
        var service = new SessionService();
        var session = await service.GetAsync(session_id);

        if (session.SubscriptionId == null)
            return BadRequest("Subscription was not created.");

        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var employer = await _employerRepo.GetEmployerByUserIdAsync(userId);

        employer.Plan = "Paid";
        employer.IsSubscriptionActive = true;
        employer.PaidUntil = DateTime.UtcNow.AddMonths(1);
        employer.StripeSubscriptionId = session.SubscriptionId;
        employer.SubscriptionCanceledAt = null;

        await _employerRepo.UpdateAsync(employer);

        var cancelLink = Url.Action("Plans","Billing",null,Request.Scheme);

        string body = $@"
        <h2>Paid Plan Activated</h2>
        <p>Your subscription is active until 
           <strong>{employer.PaidUntil:dd MMM yyyy}</strong></p>

        <p>
            You can manage or cancel your subscription here:<br />
            <a href='{cancelLink}'>Manage Subscription</a>
        </p>

        <small>
            If you cancel, your plan will downgrade to Basic after the paid period ends.
        </small>";

        await _emailService.SendSimpleEmailAsync(
            employer.User.Email,
            "Your JobConnect Paid Plan is Active",
            body
        );

        return RedirectToAction("Plans", "Billing");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelSubscription()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var employer = await _employerRepo.GetEmployerByUserIdAsync(userId);

        if (string.IsNullOrEmpty(employer.StripeSubscriptionId))
            return RedirectToAction("Plans");

        var service = new SubscriptionService();
        await service.UpdateAsync(employer.StripeSubscriptionId, new SubscriptionUpdateOptions
        {
            CancelAtPeriodEnd = true
        });

        employer.SubscriptionCanceledAt = DateTime.UtcNow;
        await _employerRepo.UpdateAsync(employer);

        return RedirectToAction("Plans");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DowngradeToBasic()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var employer = await _employerRepo.GetEmployerByUserIdAsync(userId);

        if (!string.IsNullOrEmpty(employer.StripeSubscriptionId))
        {
            var service = new SubscriptionService();
            await service.CancelAsync(employer.StripeSubscriptionId);
        }

        employer.Plan = "Basic";
        employer.IsSubscriptionActive = false;
        employer.PaidUntil = null;
        employer.StripeSubscriptionId = null;
        employer.SubscriptionCanceledAt = DateTime.UtcNow;

        await _employerRepo.UpdateAsync(employer);

        return RedirectToAction("Plans");
    }
}
