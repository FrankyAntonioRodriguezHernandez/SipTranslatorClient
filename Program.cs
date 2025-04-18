using System;
using System.Threading;

namespace SIPTranslationClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== CLIENTE SIP PARA ANTISIP.COM ===");
            
            var client = new SIPClient();
            
            Console.WriteLine("\nEsperando registro SIP...");
            for (int i = 0; i < 10; i++)
            {
                if (client.IsRegistered()) break;
                Thread.Sleep(1000);
            }
            
            if (!client.IsRegistered())
            {
                Console.WriteLine("No se pudo registrar. Revisa:");
                Console.WriteLine("1. Tus credenciales en SIPClient.cs");
                Console.WriteLine("2. Tu conexión a internet");
                Console.WriteLine("3. Que el firewall no bloquee el puerto 5060");
                Console.WriteLine("\nPresiona cualquier tecla para salir...");
                Console.ReadKey();
                return;
            }
            
            while (true)
            {
                Console.WriteLine("\nOpciones:");
                Console.WriteLine("1 - Llamar al servicio de eco (prueba audio)");
                Console.WriteLine("2 - Llamar a otro número");
                Console.WriteLine("3 - Ver información de depuración");
                Console.WriteLine("x - Salir");
                Console.Write("\nSelección: ");
                
                var key = Console.ReadKey().KeyChar;
                Console.WriteLine();
                
                switch (key)
                {
                    case '1':
                        Console.WriteLine("Llamando al servicio de eco...");
                        Console.WriteLine("Habla después del tono (deberías escuchar tu voz)");
                        client.MakeCall("echo@conference.sip.antisip.com");
                        Thread.Sleep(10000); // 10 segundos de prueba
                        client.HangUp();
                        break;
                        
                    case '2':
                        Console.Write("Ingresa número a llamar (ej: 100@sip.antisip.com): ");
                        var number = Console.ReadLine();
                        if (!string.IsNullOrEmpty(number))
                        {
                            client.MakeCall(number);
                            Console.WriteLine("Presiona cualquier tecla para colgar...");
                            Console.ReadKey();
                            client.HangUp();
                        }
                        break;
                        
                    case '3':
                        client.PrintDebugInfo();
                        break;
                        
                    case 'x':
                        return;
                        
                    default:
                        Console.WriteLine("Opción no válida");
                        break;
                }
            }
        }
    }
}