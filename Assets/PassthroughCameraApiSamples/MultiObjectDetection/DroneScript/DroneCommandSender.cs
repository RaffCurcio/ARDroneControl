using System.Net.Sockets;
using System.Text;

public class DroneCommandSender
{
    private readonly UdpClient _udpClient;
    private readonly string _ip;
    private readonly int _port;
    private readonly byte[] _buffer = new byte[256]; // Riutilizzabile

    public DroneCommandSender(string ip, int port)
    {
        _ip = ip;
        _port = port;
        _udpClient = new UdpClient();
    }

    public void Send(string command)
    {
        int length = Encoding.UTF8.GetBytes(command, 0, command.Length, _buffer, 0);
        _udpClient.Send(_buffer, length, _ip, _port);
    }

    public void Close()
    {
        _udpClient?.Close();
    }
}