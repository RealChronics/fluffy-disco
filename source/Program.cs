using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AntiCaptchaAPI;

namespace effectiveWinner
{
    internal class Program
    {
        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static async Task<string> captcha()
        {
            var captcha = new AntiCaptcha("YOURANTICAPTCHA TOKEN");
            var funCaptcha =
                await captcha.SolveFunCaptcha("A2A14B1D-1AF3-C791-9BBC-EE33CC7A0A6F", "https://www.roblox.com/");

            Console.WriteLine(funCaptcha.Response);

            return funCaptcha.Response;
        }

        private static readonly HttpClient client = new HttpClient();


        public static async Task<HttpResponseMessage> PostAsync(string uri, HttpMethod method,
            IDictionary<string, string> headers = null, FormUrlEncodedContent content = null)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uri),
                Method = method,
                Content = content
            };

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return await client.SendAsync(request);
        }

        private static readonly List<string> proxies =
            new List<string>(File.ReadAllLines(Directory.GetCurrentDirectory() + "\\proxies.txt"));

        public static async Task Main(string[] args)
        {
            Parallel.ForEach(proxies, async (i) =>
            {
                string csrf = "";
                using var wb = new WebClient();

                string htmlcode = wb.DownloadString("https://www.roblox.com");

                Match m = Regex.Match(htmlcode, "<meta name=\"csrf-token\" data-token=\"(.*)\" />");
                csrf = m.Groups[1].Value;

                var variables =
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("username", RandomString(8)),
                        new KeyValuePair<string, string>("password", "izotogambaregambare1F"),
                        new KeyValuePair<string, string>("gender", "2"),
                        new KeyValuePair<string, string>("birthday", "05 Mar 1989"),
                        new KeyValuePair<string, string>("isTosAgreementBoxChecked", "true"),
                        new KeyValuePair<string, string>("captchaToken", captcha().Result)
                    });

                var list = new Dictionary<string, string>
                {
                    {
                        "X-CSRF-TOKEN", csrf
                    },
                    {
                        "User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:93.0) Gecko/20100101 Firefox/93.0"
                    }
                };

                var response = await PostAsync("https://auth.roblox.com/v2/signup", HttpMethod.Post, list, variables);
                Console.WriteLine(await response.Content.ReadAsStringAsync());

                string pattern = Regex.Escape(".ROBLOSECURITY=") + "(.*?);";

                MatchCollection matches = Regex.Matches(response.Headers.ToString(), pattern);

                foreach (Match match in matches)
                    Console.WriteLine(match.Value.Replace(";", "").Replace(".ROBLOSECURITY=", ""));
            });
        }
    }
}
