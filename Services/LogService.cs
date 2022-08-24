using System;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace WS_InterFaxApi_DotNet5.Services
{
    public class LogService
    {
        private readonly IHostEnvironment _env;
        private const string LogFileName = "log.txt";

        public LogService(IHostEnvironment env)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public void WriteToLog(string txt, bool isBreak = true)
        {
            var filePath = Path.Combine(_env.ContentRootPath, LogFileName);
            if (!File.Exists(filePath))
            {
                using var myFile = File.Create(filePath);
                myFile.Dispose();
            }
            var writer = new StreamWriter(filePath, true);
            writer.WriteLine(txt);
            writer.WriteLine("DateTime: " + DateTimeOffset.Now.ToString("F"));
            if (isBreak)
            {
                writer.WriteLine("*********************************************");
                writer.WriteLine();
            }
            writer.Close();
        }
    }
}