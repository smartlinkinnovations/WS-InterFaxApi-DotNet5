using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WS_InterFaxApi_DotNet5.Services;

namespace WS_InterFaxApi_DotNet5
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly string SYNC_FN_URL = "https://smartlink.smartmeds.ca/SmartInterFaxApi/InterFaxInbound/CheckAndSyncFaxes";
        
        private readonly LogService _logService;
        public Worker(ILogger<Worker> logger, LogService logService)
        {
            _logger = logger;
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                // @every 2 minutes
                await Task.Delay(120000, stoppingToken);
                 _logService.WriteToLog("Cron Job Started :" + DateTime.Now, true);
                 
                var downloadString = "-----";
                using (var client = new WebClient())
                {
                    downloadString = client.DownloadString(SYNC_FN_URL);
                    _logService.WriteToLog(SYNC_FN_URL);
                    // _logService.WriteToLog(downloadString);
                }

                if (downloadString == "NODATA") continue;
                {
                    using var client = new SmtpClient();
                    try
                    {
                        var mail = new MailMessage();
                        
                        client.Port = 587;
                        client.Host = "smtp.office365.com";
                        client.Timeout = 10000;
                        client.EnableSsl = true;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new System.Net.NetworkCredential("mailer@carerx.ca", "genPCg~0NY,*9nf1Y9,SKyIZ^f/1v2~k");
                        client.TargetName = "STARTTLS/smtp.office365.com";
                        mail.IsBodyHtml = true;
                        mail.From = new MailAddress("mailer@carerx.ca");
                        mail.Subject = "Worker Service Response: " + DateTimeOffset.Now;
                        mail.Body = "Worker service execution time: " + DateTimeOffset.Now;
                        mail.Body += "\n\n";
                        mail.Body += downloadString;
                        mail.To.Add(new MailAddress("anil.kumar@smartmeds.ca"));
                        client.Send(mail);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

            }
        }
    }
}