# PacketBuilderLibrary
Merge some packet splitted into several buffers

### Why
If you have worked with SerialPort you know, that the data packet can be splitted into two or more `DataReceived` calls or there can be 
some trash before first data.

If your packets are fixed-length, this simplies library will help you with this problem. Just add some header before your data and use this
library

### Example
#### Sender

This is an example of some data sender stuff (e.g. Arduino)
```c++
uint8_t my_header[] = {0x34, 0x75}; // some random header
uint8_t my_data[] = "Hello!";       // some data

void setup()
{
  Serial.begin(9600);
}

void loop()
{
  Serial.write(my_header, sizeof(my_header));

  // Send first 3 bytes of data, wait 100ms and send rest bytes
  Serial.write(my_data, 3);
  delay(100);
  Serial.write(my_data+3, sizeof(my_data)-3);
  delay(500);

  delay(500);
}
```

#### Receiver (C#)

This is a simple C# console-app

```c#
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

```

Example of receiving without `PacketBuilder`
```
                                                                   Hel 
lo!

He
l
lo!
Hel
l
o!
Hel
lo
!
Hel
lo
!
H
el
lo!
```

And now example of receiving with `PacketBuilder`
```
Hello!
Hello!
Hello!
Hello!
Hello!
```

```
Markov Alexey, 2018
```
