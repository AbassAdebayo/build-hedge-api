using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.IDS
{
    public class LoginIntrusionDetector
    {
        private static Dictionary<string, List<DateTime>> attempts = new();
        public bool RecordAttempt(string ip)
        {
            if (!attempts.ContainsKey(ip))
            {
                attempts[ip] = new List<DateTime>();
            }

            attempts[ip].Add(DateTime.Now);

            // Remove attempts older than 2 minutes
            attempts[ip].RemoveAll(a => a < DateTime.Now.AddMinutes(-2));

            if (attempts[ip].Count > 5)
            {
                BlockIP(ip);
                return true;
            }

            return false;
        }

        private void BlockIP(string ip)
        {
            Console.WriteLine($"Intrusion detected: {ip} blocked due to too many attempts.");
        }



    }
}
