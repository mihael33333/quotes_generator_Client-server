using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static string[] quotes =
    {
        "Жизнь коротка, искусство вечно, случайные обстоятельства скоропреходящи, опыт обманчив, суждения трудны.",
        "Бесполезные люди живут только для того, чтобы есть и пить. Достойные люди едят и пьют только для того, чтобы жить.",
        "Благо везде и повсюду зависит от соблюдения двух условий: правильного установления конечных целей и отыскания соответствующих средств, ведущих к конечной цели.",
        "То, что ты не хочешь иметь завтра, отбрось сегодня, а то, что хочешь иметь завтра, приобретай сегодня.",
        "Честь наша состоит в том, чтобы следовать лучшему и улучшать худшее, если оно ещё может стать совершеннее."
    };

    static int MAX_Q = 5;
    static int AVAILABLE_THREADS = 1;
    static string LOGIN = "admin";
    static string PASSWORD = "12345";


    static void Main()
    {
        Console.WriteLine("Сервер запущен: " + DateTime.Now.ToString("HH:mm:ss"));
        Console.WriteLine("Набор цитат:");
        for (int i = 0; i < quotes.Length; i++)
            Console.WriteLine($"{(i + 1)}) {quotes[i]}");
        Console.WriteLine();

        int port = 1024;

        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Any, port));
        listener.Listen(100);

        Console.WriteLine("Слушаю порт " + port);

        while (true)
        {
            Socket client = listener.Accept();
            if (AVAILABLE_THREADS == 0)
            {
                client.Send(Encoding.Unicode.GetBytes("NO|сервер сейчас находится под максимальной нагрузкой. Попробуйте подключиться через какое-то время."));
                continue;
            }

            client.Send(Encoding.Unicode.GetBytes("YES"));
            Thread t = new Thread(HandleClient);
            t.IsBackground = true;
            t.Start(client);
        }
    }

    static void HandleClient(object obj)
    {
        AVAILABLE_THREADS--;
        bool work = true;

        Socket s = (Socket)obj;
        string who = s.RemoteEndPoint.ToString();

        Console.WriteLine($"Кто подключился: " + who);
        Console.WriteLine($"Когда подключился: " + DateTime.Now.ToString("HH:mm:ss"));

        byte[] buf = new byte[2048];
        int l = s.Receive(buf);
        string log = Encoding.Unicode.GetString(buf, 0, l);

        if (log.Split('\n')[0] == LOGIN && log.Split('\n')[1] == PASSWORD)
        {
            s.Send(Encoding.Unicode.GetBytes("OK"));
        }
        else
        {
            s.Send(Encoding.Unicode.GetBytes("WRONG"));
            work = false;

        }

        Random rnd = new Random();
        int q_num = 0;

        try
        {
            while (work)
            {

                byte[] buffer = new byte[2048];
                int len = s.Receive(buffer);

                string cmd = Encoding.Unicode.GetString(buffer, 0, len);

                if (cmd=="EXIT")
                    break;

                if (cmd=="Q")
                {
                    string q = quotes[rnd.Next(quotes.Length)];

                    q_num++;

                    if (q_num >= MAX_Q)
                    {
                        string er = $"\n\nВы получили максимальное число цитат! ({MAX_Q})";
                        q = "EXIT" + q + er;
                        s.Send(Encoding.Unicode.GetBytes(q));
                        break;
                    }

                    s.Send(Encoding.Unicode.GetBytes(q));


                }


            }
        }
        catch (SocketException)
        {

        }
        finally
        {
            Console.WriteLine($"Время отключения ({who}): {DateTime.Now.ToString("HH:mm:ss")}");
            Console.WriteLine();

            s.Shutdown(SocketShutdown.Both);
            s.Close();
            AVAILABLE_THREADS++;

        }
    }
}
