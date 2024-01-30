using ChatUser;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


class Program
{
    static int _userName;
    private const string _host = "192.168.2.33";
    private const int port = 8888;
    static int _receiverName;
    static TcpClient _tcpClient;
    static NetworkStream _stream;

    static void Main(string[] args)
    {
        Console.Write("Введите свое имя: ");
        _userName = int.Parse(Console.ReadLine());
        Console.Write("Введите имя получателя: ");
        _receiverName = int.Parse(Console.ReadLine());
        //_userName = "Отправитель: ";
        //_receiverName = "Получатель: ";

        _tcpClient = new TcpClient();

        try
        {
            _tcpClient.Connect(_host, port);
            _stream = _tcpClient.GetStream();

            var message = new Message
            {
                SenderId = _userName,
                ReceiverId = _receiverName,
                Content = ""
            };

            var jsonMessage = JsonSerializer.Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(jsonMessage);

            _stream.Write(data, 0, data.Length);

            Thread receivedThread = new Thread(ReceiveMessage);
            receivedThread.Start();

            Console.WriteLine("Добро пожаловать: " + _userName);

            SendMessage();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Disconnect();
        }
    }


    static void SendMessage()
    {
        Console.WriteLine("Введите сообщение: ");

        while (true)
        {
            string content = Console.ReadLine();
            var message = new Message
            {
                SenderId = _userName,
                ReceiverId = _receiverName,
                Content = content
            };


            var jsonMessage = JsonSerializer.Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
            _stream.Write(data, 0, data.Length);
        }
    }


    static void ReceiveMessage()
    {
        while (true)
        {
            try
            {
                byte[] data = new byte[64];
                StringBuilder builder = new StringBuilder();
                int bytes = 0;

                do
                {
                    bytes = _stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (_stream.DataAvailable);

                var message = JsonSerializer.Deserialize<Message>(builder.ToString());
                Console.WriteLine(message?.Content);
            }
            catch
            {
                Console.WriteLine("Подключение прервано!"); //соединение было
                Console.ReadLine();
                Disconnect();
            }
        }
    }



    static void Disconnect()
    {
        if (_stream != null)
            _stream.Close();
        if (_tcpClient != null)
            _tcpClient.Close();

        Environment.Exit(0);
    }

    
}
