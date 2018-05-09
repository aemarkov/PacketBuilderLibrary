using System;
using System.Text;
using System.IO.Ports;
using Markov.PacketBuilderLibrary;

namespace ReceiverExample
{
    class Program
    {
        private static SerialPort com;
        private static PacketBuilder builder;

        static void Main(string[] args)
        {
            com = new SerialPort("COM3", 9600);  // Change for your port

            // Setup builder - set same header and payload length as in Sender code
            builder = new PacketBuilder(new byte[] {0x34, 0x75}, 7);

            // Subscribe events
            com.DataReceived += Com_DataReceived;
            builder.PackageReceived += Builder_PackageReceived;

            com.Open();
            Console.ReadKey();
            com.Close();            
        }

        private static void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Receive all available bytes from SerialPort
            int size = com.BytesToRead;
            byte[] buffer = new byte[size];
            com.Read(buffer, 0, size);

            // Push data to the PacketBuilder
            builder.ProcessPart(buffer);
        }

        private static void Builder_PackageReceived(object sender, byte[] packet)
        {
            // This event fires when packet is fully received
            // packet variable contains packet payload (without header)        
            string text = Encoding.ASCII.GetString(packet);
            Console.WriteLine(text);
        }
    }
}
