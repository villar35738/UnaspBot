using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace TesteRobo
{
    class Program
    {
        static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
       //static string ApplicationName = "Google Calendar API .NET Quickstart";

        static void Main(string[] args)
        {
            var serviceDriver = new ChromeOptions();
            serviceDriver.AddArgument("--headless");

            ChromeDriver driver = new ChromeDriver(serviceDriver);
            string nomeLogin = "", senha = "";
            var materia = "";
            var titulo = "";
            var time = "";
            var status = "";

            string[] lines = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + "//Dados.txt");

            bool aux = true;
            foreach (string dado in lines)
            {
                if (aux)
                {
                    nomeLogin = dado.ToString();
                    aux = false;
                }
                if (!aux)
                {
                    senha = dado.ToString();
                }
            }
            Console.WriteLine("\n--------- Redirecionando para https://unasp.mrooms.net/login/index.php ---------");
            driver.Navigate().GoToUrl("https://unasp.mrooms.net/login/index.php");
            Console.WriteLine("------------------------------- Realizando o login -----------------------------");
            driver.FindElementById("username").SendKeys(nomeLogin);
            driver.FindElementById("password").SendKeys(senha);

            driver.FindElementById("loginbtn").Click();

            Thread.Sleep(2000);
            Console.WriteLine("-------------------------- Login efetuado com sucesso --------------------------");
            driver.FindElementById("snap-pm-trigger").Click();
            Console.WriteLine("----------------------- Exibindo todas as tarefas pendentes --------------------");

            Thread.Sleep(1500);

            var deadLines = driver.FindElementsByXPath("//div[@id='snap-personal-menu-deadlines']/div");

            if (deadLines.Count == 0)
            {
                Console.WriteLine("\nA conexão está demorando mais do que o normal. Reconectando com o servidor em...");
                Console.Write("                                   3... ");
                Thread.Sleep(1000);
                Console.Write("2... ");
                Thread.Sleep(1000);
                Console.Write("1");
                Thread.Sleep(1000);
                deadLines = driver.FindElementsByXPath("//div[@id='snap-personal-menu-deadlines']/div");

                if (deadLines.Count == 0)
                {
                    Console.WriteLine("\nO sistema não conseguiu se conectar com o servidor. Por favor, verifique sua conexão e tente novamente...");
                    Console.ReadKey();
                    driver.Dispose();
                    Environment.Exit(0);
                }
                Console.WriteLine("");
            }
            
            Char delimiter = '\r';
            
            Console.WriteLine("\n******************************* Tarefas em aberto ******************************");
            Console.WriteLine("********************************************************************************");
            foreach (var item in deadLines)
            {
                titulo = item.FindElement(By.TagName("h3")).GetAttribute("innerText");
                
                Console.WriteLine("Titulo: " + titulo.Split(delimiter).First());

                materia = item.FindElement(By.TagName("small")).GetAttribute("innerText");
                Console.WriteLine("Materia: " + materia.Trim());

                time = item.FindElement(By.TagName("time")).GetAttribute("innerText");
                Console.WriteLine("Data de entrega: " + time);

                status = item.FindElement(By.ClassName("snap-completion-meta")).GetAttribute("innerText");
                Console.WriteLine("Status: " + status + "\n");

                createEventCalendar(calendar(), titulo + " - " + materia, time);

                Console.WriteLine("********************************************************************************");
            }

            Console.ReadKey();
            driver.Dispose();
        }

        public static CalendarService calendar()
        {
            // GOOGLE CALENDAR

            UserCredential credential;

            using (var stream =
                new FileStream(@"D:\GIT Projects\UnaspBot\UnaspBot_Solution\UnaspBot\credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var googleCalendar = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "UnaspBot",
            });

            // Define parameters of request.
            EventsResource.ListRequest request = googleCalendar.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // END GOOGLE CALENDAR

            return googleCalendar;
        }

        public static void createEventCalendar(CalendarService calendar, string titulo, string dataFim)
        {
            Event newEvent = new Event()
            {
                Summary = "Google I/O 2015",
                Location = "800 Howard St., San Francisco, CA 94103",
                Description = "A chance to hear more about Google's developer products.",
                Start = new EventDateTime()
                {
                    DateTime = DateTime.Parse("2019-20-02T09:00:00-07:00"),
                    TimeZone = "America/Los_Angeles",
                },
                End = new EventDateTime()
                {
                    DateTime = DateTime.Parse("2015-05-28T17:30:00-07:00"),
                    TimeZone = "America/Los_Angeles",
                },
            };

            String calendarId = "primary";
            EventsResource.InsertRequest request = calendar.Events.Insert(newEvent, calendarId);
            Event createdEvent = request.Execute();
            Console.WriteLine("Event created: {0}", createdEvent.HtmlLink);
        }
    }
}
