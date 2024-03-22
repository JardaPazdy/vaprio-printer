using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Drawing.Printing;

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
                    string responseData = await response.Content.ReadAsStringAsync();
                    
                    // Zde můžete provést další zpracování dat, např. jejich zobrazení
                    Console.WriteLine(responseData);
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
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(3)); // Interval X sekund

        // Udržení hlavního vlákna živého
        Console.ReadLine();

        // Zastavení Timeru před ukončením programu
        timer.Dispose();
    }
}