using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace TesteRobo
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = PhantomJSDriverService.CreateDefaultService();
            service.AddArgument("--webdriver-loglevel=NONE");

            PhantomJSDriver webTeste = new PhantomJSDriver(service);
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
            webTeste.Navigate().GoToUrl("https://unasp.mrooms.net/login/index.php");
            Console.WriteLine("------------------------------- Realizando o login -----------------------------");
            webTeste.FindElementById("username").SendKeys(nomeLogin);
            webTeste.FindElementById("password").SendKeys(senha);

            webTeste.FindElementById("loginbtn").Click();

            Thread.Sleep(2000);
            Console.WriteLine("-------------------------- Login efetuado com sucesso --------------------------");
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

                Console.WriteLine("********************************************************************************");
            }

            Console.ReadKey();
            webTeste.Dispose();
        }
    }
}
