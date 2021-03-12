using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.Services
{
    public class CloudMailService : IMailService
    {
        private readonly IConfiguration _iconfiguration;

        public CloudMailService(IConfiguration configuration)
        {
            // To access email from appsettings.json
            _iconfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void Send(string subject, string message)
        {
            Debug.WriteLine($"Mail from {_iconfiguration["mailSettings:mailFromAddress"]} to {_iconfiguration["mailSettings:mailToAddress"]}, with LocalMailService.");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine($"Message: {message}");
        }
    }
}
