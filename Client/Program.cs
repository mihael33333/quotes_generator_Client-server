using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main()
    {
        string serverIp = "127.0.0.1";
        int port = 1024;

        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            s.Connect(new IPEndPoint(IPAddress.Parse(serverIp), port));

            byte[] buf = new byte[2048];
            int l = s.Receive(buf);
            string ans = Encoding.Unicode.GetString(buf, 0, l);

            bool work = true;
            if (ans.StartsWith("NO|"))
            {
                Console.WriteLine(ans.Substring(3));
                work = false;
                
            }
            else
            {
                Console.Write("Подключение успешно! \nВведи логин: ");
                string login = Console.ReadLine();
                Console.Write("\nВведи пароль: ");
                string password = Console.ReadLine();

                s.Send(Encoding.Unicode.GetBytes($"{login}\n{password}"));


                byte[] buf2 = new byte[2048];
                int l2 = s.Receive(buf);
                string ans2 = Encoding.Unicode.GetString(buf, 0, l);

                if (ans2.StartsWith("OK"))
                {
                    Console.WriteLine("Введи 'q' - получить цитату, 'exit' - выйти.");
                }
                else
                {
                    Console.WriteLine("Неверный логин или пароль!");
                    work = false;
                }
            }


            while (work)
            {
                string input = Console.ReadLine();

                if (input == "exit")
                {
                    s.Send(Encoding.Unicode.GetBytes("EXIT"));
                    break;
                }

                if (input == "q")
                {
                    s.Send(Encoding.Unicode.GetBytes("Q"));

                    byte[] buffer = new byte[2048];
                    int len = s.Receive(buffer);

                    string quote = Encoding.Unicode.GetString(buffer, 0, len);
                    if (quote.StartsWith("EXIT"))
                    {
                        Console.WriteLine("Цитата: " + quote.Substring(4));

                        break;
                    }
                    Console.WriteLine("Цитата: " + quote);
                }
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            s.Shutdown(SocketShutdown.Both);
            s.Close();
        }
    }
}
