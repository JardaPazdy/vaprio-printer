using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Drawing.Printing;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        // Inicializace HttpClient pro stahování dat z URL
        HttpClient httpClient = new HttpClient();

        // Nastavení URL, kterou chcete periodicky kontaktovat
        string targetUrl = "http://vaprio.net/labs/vaprio-printer.php";

        // Nastavení autentizačních údajů pro HTTP Basic Authentication
        string username = "malinka";
        string password = "gamepark";
        string base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

        // Nastavení hlavičky Authorization pro HttpClient
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Credentials);

        // Vytvoření a nastavení Timeru pro periodické provádění akce
        Timer timer = new Timer(async _ =>
        {
            try
            {
                // Získání dat z cílové URL
                HttpResponseMessage response = await httpClient.GetAsync(targetUrl);

                // Kontrola, zda byla odpověď úspěšná (kód 200)
                if (response.IsSuccessStatusCode)
                {
                    // Přečtení obsahu odpovědi jako řetězec
                    string content = await response.Content.ReadAsStringAsync();
                    
                    var headers = response.Headers;

                    var printType = headers.GetValues("Vaprio-Print-Type").First();
                    var printerName = headers.GetValues("Vaprio-Printer-Name").First();

                    // nacetly se nam udaje, tak provedeme co je potreba
                    // pokud je to pdf, nacteme si ten soubor a posleme ho na tiskarnu
                    if(printType == "pdf") {
                        var obsahSouboru = httpClient.GetByteArrayAsync(content).Result;
                        File.WriteAllBytes("temp.pdf", obsahSouboru);

                        PrintPDF("temp.pdf", printerName);
                    }

                    // tisk html
                    if(printType == "html") {}

                    // tisk plain textu 
                    if(printType == "plain") {}

                    Console.WriteLine($"{printType} - {printerName} - {content}");
                }
                else
                {
                    Console.WriteLine($"Chyba: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba: {ex.Message}");
            }
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(20)); // Interval X sekund

        // Udržení hlavního vlákna živého
        Console.ReadLine();

        // Zastavení Timeru před ukončením programu
        timer.Dispose();
    }

    // Metoda pro tisk PDF souboru
    static void PrintPDF(string fileName, string printerName)
    {
        // Definice cesty k PDF souboru, který chcete vytisknout
        string pdfFilePath = fileName;

        // Nastavení parametrů pro tisk na tiskárnu
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            Verb = "printto",
            FileName = pdfFilePath,
            UseShellExecute = true,
            Arguments = printerName
        };

        // Spuštění procesu pro tisk PDF souboru
        try
        {
            using (Process printProcess = new Process())
            {
                printProcess.StartInfo = startInfo;
                printProcess.EnableRaisingEvents = true; // Povolení zvyšování událostí
                printProcess.Exited += PrintProcess_Exited; // Přidání obslužné metody pro událost Exited
                printProcess.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Chyba při tisku PDF souboru: " + ex.Message);
        }
    }   

    // Metoda pro zpracování události Exited
    private static void PrintProcess_Exited(object sender, EventArgs e)
    {
        if (sender != null)
        {
            Console.WriteLine("Tisk byl dokončen.");
            // Zde můžete provést další akce, které chcete vykonat po dokončení tisku
        }
        else
        {
            Console.WriteLine("Chyba: Objekt sender je null.");
        }
    }     
}