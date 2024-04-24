
using System.Threading.Tasks;
using System.Threading;
using Volo.Abp.BackgroundWorkers.Hangfire;
using Microsoft.Extensions.Logging;
using Volo.Abp.Identity;
using Acme.BookStore.BackgroundJob;
using Hangfire;
using System;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Emailing.Templates;
using Volo.Abp.TextTemplating;

namespace Acme.BookStore.BackgroundWorker
{
    public class MyLogWorker : HangfireBackgroundWorkerBase
    {
        private readonly IdentityUserManager _userManager;
        private readonly IdentityRoleManager _roleManager;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly ITemplateRenderer _templateRenderer;

        public MyLogWorker(IdentityUserManager userManager,
        IdentityRoleManager roleManager, IBackgroundJobManager backgroundJobManager, ITemplateRenderer templateRenderer)
        {
            RecurringJobId = nameof(MyLogWorker);
            CronExpression = Cron.Monthly();
            _userManager = userManager;
            _roleManager = roleManager;
            _backgroundJobManager = backgroundJobManager;
            _templateRenderer = templateRenderer;
        }

        public override async Task<Task> DoWorkAsync(CancellationToken cancellationToken = default)
        {
            var emailBody = await _templateRenderer.RenderAsync(StandardEmailTemplates.Message, new { message = "ABP Framework provides IEmailSender service that is used to send emails." });
            var customerRole = await _roleManager.FindByNameAsync("customer");
            if (customerRole == null)
            {
                Logger.LogInformation("không tìm thấy");
                return Task.CompletedTask;

            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(customerRole.Name);
           
            foreach (var item in usersInRole)
            {
                await _backgroundJobManager.EnqueueAsync(
                    new EmailSendingArgs
                    {
                        EmailAddress = item.Email,
                        Subject = "Xin chào",
                        Body = emailBody


                    },
                     delay: TimeSpan.FromSeconds(30)
                ); ;
            }
            return Task.CompletedTask;
        }
    }
}
