using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System.IO;
using System.Web.Script.Serialization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace UnaspBotForm
{
    public partial class Form1 : Form
    {
        static string[] Scopes = { CalendarService.Scope.Calendar };
        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Status: Pronto. ";
            listTask.Items.Add("*********************************** Tarefas em aberto ***********************************");
            listTask.Items.Add("***********************************************************************************************");

            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "parametrosBusca.json");

            dynamic fileParametros = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            string tipo = fileParametros["tipo"];

            toolStripComboBoxBuscar.Text = tipo;

            if (tipo.Equals("Buscar Automaticamente"))
            {
                execBtnBuscar();
            }
        }

        private bool buscarTarefas()
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

                webTeste.Navigate().GoToUrl("https://unasp.mrooms.net/login/index.php");
                toolStripStatusLabel1.Text = "Status: Realizando o login. ";
                webTeste.FindElementById("username").SendKeys(usuario);
                webTeste.FindElementById("password").SendKeys(senha);

                webTeste.FindElementById("loginbtn").Click();

                Thread.Sleep(2000);
                toolStripStatusLabel1.Text = "Status: Login efetuado com sucesso. ";
                webTeste.FindElementById("snap-pm-trigger").Click();
                toolStripStatusLabel1.Text = "Status: Carregando todas as tarefas pendentes. ";
                Thread.Sleep(1500);

                var deadLines = webTeste.FindElementsByXPath("//div[@id='snap-personal-menu-deadlines']/div");

                if (deadLines.Count == 0)
                {
                    toolStripStatusLabel1.Text = "Status: Carregando todas as tarefas pendentes. ";
                    Thread.Sleep(1000);
                    toolStripStatusLabel1.Text = "Status: Carregando 3... ";
                    Thread.Sleep(1000);
                    toolStripStatusLabel1.Text = "Status: Carregando 2... ";
                    Thread.Sleep(1000);
                    toolStripStatusLabel1.Text = "Status: Carregando 1... ";
                    deadLines = webTeste.FindElementsByXPath("//div[@id='snap-personal-menu-deadlines']/div");

                    if (deadLines.Count == 0)
                    {
                        toolStripStatusLabel1.Text = "Status: Por favor, verifique sua conexão e tente novamente. ";
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
                    this.Invoke((MethodInvoker)delegate
                    {
                        listTask.Items.Add("Titulo: " + titulo.Split(delimiter).First());
                    });

                    materia = item.FindElement(By.TagName("small")).GetAttribute("innerText");
                    this.Invoke((MethodInvoker)delegate
                    {
                        listTask.Items.Add("Materia: " + materia.Trim());
                    });


                    time = item.FindElement(By.TagName("time")).GetAttribute("innerText");
                    this.Invoke((MethodInvoker)delegate
                    {
                        listTask.Items.Add("Data de entrega: " + time);
                    });
                    time = item.FindElement(By.TagName("time")).GetAttribute("datetime");

                    status = item.FindElement(By.ClassName("snap-completion-meta")).GetAttribute("innerText");
                    this.Invoke((MethodInvoker)delegate
                    {
                        listTask.Items.Add("Status: " + status + "\n");
                    });

                    createEventCalendar(calendar(), titulo, materia, time, status);

                    this.Invoke((MethodInvoker)delegate
                    {
                        listTask.Items.Add("******************************************************************************************");
                    });
                }

                webTeste.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(null, "Ocorreu um erro ao buscar as tarefas pendentes. Provável problema de conexão com o servidor ou não há tarefas para exibir, por favor tente novamente mais tarde.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void execBtnBuscar()
        {
            pictureBox1.BringToFront();
            pictureBox1.Visible = true;
            var qtd = listTask.Items.Count;
            if (qtd == 2)
            {
                backgroundWorker1.RunWorkerAsync();
            }
            else
            {
                listTask.Items.Clear();
                listTask.Items.Add("****************************** Tarefas em aberto ******************************");
                listTask.Items.Add("***********************************************************************************");
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void btnBusca_Click(object sender, EventArgs e)
        {
            execBtnBuscar();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            toolStripStatusLabel1.Text = "Status: Loading ... " + "Thanks for your patience";
            buscarTarefas();
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripStatusLabel1.Text = "Status: Pronto. ";
            pictureBox1.Visible = false;
        }

        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void usuárioSenhaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormParametros formParametros = new FormParametros();
            formParametros.Show();
        }

        private void toolStripComboBoxBuscar_TextChanged(object sender, EventArgs e)
        {
            var selectedValue = toolStripComboBoxBuscar.Text.ToString();

            TipoBusca busca = new TipoBusca();

            busca.tipo = selectedValue;

            string json = new JavaScriptSerializer().Serialize(busca);

            //write string to file
            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "parametrosBusca.json", json);
        }

        private void toolStripComboBoxBuscar_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        public static CalendarService calendar()
        {
            // GOOGLE CALENDAR

            UserCredential credential;

            using (var stream =
                new FileStream(AppDomain.CurrentDomain.BaseDirectory + "//credentials.json", FileMode.Open, FileAccess.Read))
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
                    if (eventItem.Description.Equals(titulo + "\n" + status))
                    {
                        calendar.Events.Delete("primary", eventItem.Id).Execute();
                        break;
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
            Console.WriteLine("Event created: {0}", createdEvent.HtmlLink);
        }
    }
}
