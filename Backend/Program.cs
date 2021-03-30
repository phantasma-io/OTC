using System;
using System.Threading;

using LunarLabs.WebServer.Core;
using LunarLabs.WebServer.HTTP;
using System.Collections.Generic;
using LunarLabs.WebServer.Templates;
using System.IO;
using Phantasma.Blockchain.Contracts;
using Phantasma.Core.Types;
using Phantasma.Numerics;
using Phantasma.Cryptography;

namespace Phantasma.Docs
{

    public struct Offer
    {
        public string ID;
        public Address seller;
        public string sellSymbol;
        public string sellAmount;
        public string buySymbol;
        public string buyAmount;

        public Offer(string iD, Address seller, string sellSymbol, string sellAmount, string buySymbol, string buyAmount)
        {
            ID = iD;
            this.seller = seller;
            this.sellSymbol = sellSymbol;
            this.sellAmount = sellAmount;
            this.buySymbol = buySymbol;
            this.buyAmount = buyAmount;
        }
    }

    class Program
    {
        const string LanguageHeader = "Accept-Language";

        static string DetectLanguage(HTTPRequest request)
        {
            if (request.headers.ContainsKey(LanguageHeader))
            {
                var languages = request.headers[LanguageHeader].Split(new char[] { ',', ';' });
                foreach (var lang in languages)
                {
                    string code;
                    if (lang.Contains("-"))
                    {
                        code = lang.Split('-')[0];
                    }
                    else
                    {
                        code = lang;
                    }

                    if (LocalizationManager.HasLanguage(code))
                    {
                        return code;
                    }
                }
            }

            return "en";
        }

        static List<Offer> offers = new List<Offer>();

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            if (args.Length == 0)
            {
                args = new string[] { "--path=" + Path.GetFullPath("../Frontend")};
            }

            var settings = ServerSettings.Parse(args);

            var server = new HTTPServer(settings, ConsoleLogger.Write);

            var templateEngine = new TemplateEngine(server, "views");

            Console.WriteLine("Frontend path: " + settings.Path);

            /*var locFiles = Directory.GetFiles("Localization", "*.csv");
            foreach (var fileName in locFiles)
            {
                var language = Path.GetFileNameWithoutExtension(fileName).Split("_")[1];
                LocalizationManager.LoadLocalization(language, fileName);
            }*/


            for (int i=1; i<10; i++)
            {
                offers.Add(new Offer(i.ToString(), PhantasmaKeys.Generate().Address, "KCAL", (100*i).ToString(), "SOUL", (20 * i).ToString()));
            }

            Func<HTTPRequest, Dictionary<string, object>> GetContext = (request) =>
            {

                var context = new Dictionary<string, object>();

                context["available_languages"] = LocalizationManager.Languages;

                var langCode = request.session.GetString("language", "auto");

                if (langCode == "auto")
                {
                    langCode = DetectLanguage(request);
                    request.session.SetString("language", langCode);
                }

                context["current_language"] = LocalizationManager.GetLanguage(langCode);

                context["offers"] = offers;

                return context;
            };

            server.Get("/language/{code}", (request) =>
            {
                var code = request.GetVariable("code");

                if (LocalizationManager.GetLanguage(code) != null)
                {
                    request.session.SetString("language", code);
                }

                return HTTPResponse.Redirect("/");
            });

            server.Get("/", (request) =>
            {
                var context = GetContext(request);
                return templateEngine.Render(context, "main");
            });

            server.Run();

            bool running = true;

            Console.CancelKeyPress += delegate {
                server.Stop();
                running = false;
            };

            while (running)
            {
                Thread.Sleep(500);
            }
        }
    }
}
