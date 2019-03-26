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
        static void Main(string[] args)
        {
            buscarTarefas();

            Console.ReadKey();
        }

        private static void buscarTarefas()
        {
            try
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArguments("--headless");

                var chromeDriverService = ChromeDriverService.CreateDefaultService();
                chromeDriverService.HideCommandPromptWindow = true;

                ChromeDriver webTeste = new ChromeDriver(chromeDriverService, options);
                var materia = "";
                var titulo = "";
                var time = "";
                var status = "";

                string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "//parametros.json");

                dynamic fileParametros = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                string usuario = fileParametros["user"];

                string senha = fileParametros["password"];

                Console.WriteLine("\n--------- Redirecionando para https://unasp.mrooms.net/login/index.php ---------");
                webTeste.Navigate().GoToUrl("https://unasp.mrooms.net/login/index.php");
                Console.WriteLine("------------------------------- Realizando o login -----------------------------");
                webTeste.FindElementById("username").SendKeys(usuario);
                webTeste.FindElementById("password").SendKeys(senha);

                webTeste.FindElementById("loginbtn").Click();
                Console.WriteLine("-------------------------- Login efetuado com sucesso --------------------------");
                Thread.Sleep(2000);
                webTeste.FindElementById("snap-pm-trigger").Click();
                Console.WriteLine("----------------------- Exibindo todas as tarefas pendentes --------------------");
                Thread.Sleep(1500);

                var deadLines = webTeste.FindElementsByXPath("//div[@id='snap-personal-menu-deadlines']/div");

                if (deadLines.Count == 0)
                {
                    Console.WriteLine("\nA conexão está demorando mais do que o normal. Reconectando com o servidor em...");
                    Console.Write("                                   3... ");
                    Thread.Sleep(1000);
                    Console.Write("2... ");
                    Thread.Sleep(1000);
                    Console.Write("1");
                    Thread.Sleep(1000);
                    deadLines = webTeste.FindElementsByXPath("//div[@id='snap-personal-menu-deadlines']/div");

                    if (deadLines.Count == 0)
                    {
                        Console.WriteLine("\nO sistema não conseguiu se conectar com o servidor. Por favor, verifique sua conexão e tente novamente...");
                        Console.ReadKey();
                        webTeste.Dispose();
                        Environment.Exit(0);
                    }
                    Console.WriteLine("");
                }

                Char delimiter = '\r';

                Console.WriteLine("\n****************************** Tarefas em aberto ***********************************");
                Console.WriteLine("*************************************************************************************");

                foreach (var item in deadLines)
                {
                    titulo = item.FindElement(By.TagName("h3")).GetAttribute("innerText");

                    Console.WriteLine("Titulo: " + titulo.Split(delimiter).First());

                    materia = item.FindElement(By.TagName("small")).GetAttribute("innerText");
                    Console.WriteLine("Materia: " + materia.Trim());

                    time = item.FindElement(By.TagName("time")).GetAttribute("datetime");
                    Console.WriteLine("Data de entrega: " + time);

                    status = item.FindElement(By.ClassName("snap-completion-meta")).GetAttribute("innerText");
                    Console.WriteLine("Status: " + status + "\n");

                    //createEventCalendar(calendar(), titulo + " - " + materia, time);
                    createEventCalendar(calendar(), titulo, materia, time, status);

                    Console.WriteLine("********************************************************************************");
                }

                webTeste.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro: " + ex.Message);
            }
        }

        public static CalendarService calendar()
        {
            // GOOGLE CALENDAR

            UserCredential credential;

            using (var stream =
                new FileStream(AppDomain.CurrentDomain.BaseDirectory + "//credentials_daniel.json", FileMode.Open, FileAccess.Read))
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

        public static void createEventCalendar(CalendarService calendar, string titulo, string materia, string dataFim, string status)
        {
            // Define parameters of request.
            EventsResource.ListRequest request = calendar.Events.List("primary");
            request.TimeMin = Convert.ToDateTime(dataFim).AddHours(-24);
            request.TimeMax = Convert.ToDateTime(dataFim);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            Events eventsList = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (eventsList.Items != null && eventsList.Items.Count > 0)
            {
                foreach (var eventItem in eventsList.Items)
                {
                    if (eventItem.Description != null)
                    {
                        if (eventItem.Description.Equals(titulo + "\n" + status))
                        {
                            calendar.Events.Delete("primary", eventItem.Id).Execute();
                            break;
                        }
                    }
                }
            }
            Event newEvent = new Event()
            {
                Summary = materia.Split('-')[0].Trim(),
                Description = titulo + "\n" + status,
                Start = new EventDateTime()
                {
                    DateTime = Convert.ToDateTime(dataFim).AddHours(-1),
                },
                End = new EventDateTime()
                {
                    DateTime = Convert.ToDateTime(dataFim),
                },
            };

            String calendarId = "primary";
            EventsResource.InsertRequest requestCreate = calendar.Events.Insert(newEvent, calendarId);
            Event createdEvent = requestCreate.Execute();
            //Console.WriteLine("Event created: {0}", createdEvent.HtmlLink);
        }
    }
}
