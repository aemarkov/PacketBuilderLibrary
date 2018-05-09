using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markov.PacketBuilderLibrary
{
    public delegate void PacketReceivedDelegate(object sender, byte[] packet);

    /// <summary>
    /// Merge some packet splitted into several packets.
    /// 
    /// It is useful for receiving data via SerialPort, when the packet
    /// may be splitted into several DataReceived events
    /// </summary>
    public class PacketBuilder
    {
        public event PacketReceivedDelegate PackageReceived;

        enum ReceiveState
        {
            RECEIVING_HEADER,
            RECEVING_BODY
        };

        //Receiving statemachine
        private ReceiveState _state;
        private List<byte> _receivedQueue;
        private int _currentHeaderByte;

        private int _packageLength;
        private byte[] _packageHeder;

        /// <summary>
        /// Create new PacketBuilder with specific header
        /// and data length
        /// </summary>
        /// <param name="header">Header</param>
        /// <param name="packageLength">Payload length (without header)</param>
        public PacketBuilder(byte[] header, int packageLength)
        {
            _packageHeder = new byte[header.Length];
            Array.Copy(header, _packageHeder, header.Length);

            _packageLength = packageLength;

            _receivedQueue = new List<byte>();
            _currentHeaderByte = 0;
            _state = ReceiveState.RECEIVING_HEADER;
        }

        /// <summary>
        /// Proceed next part of data
        /// </summary>
        /// <param name="buffer">Received data</param>
        public void ProcessPart(byte[] buffer)
        {
            int i = 0;
            while (i < buffer.Length)
            {
                if (_state == ReceiveState.RECEIVING_HEADER)
                {
                    //Wait for data header

                    if (buffer[i] == _packageHeder[_currentHeaderByte])
                    {
                        // Reading header
                        _state = ReceiveState.RECEIVING_HEADER;
                        _currentHeaderByte++;
                    }
                    else
                    {
                        // This isn't header, ignore
                        _currentHeaderByte = 0;
                        _state = ReceiveState.RECEIVING_HEADER;
                    }

                    if (_currentHeaderByte == _packageHeder.Length)
                    {
                        // Header is read, waiting for body
                        _state = ReceiveState.RECEVING_BODY;
                    }
                }
                else if (_state == ReceiveState.RECEVING_BODY)
                {
                    // Reading packet body
                    _receivedQueue.Add(buffer[i]);

                    if (_receivedQueue.Count == _packageLength)
                    {
                        // Packet received!
                        _state = ReceiveState.RECEIVING_HEADER;
                        _currentHeaderByte = 0;

                        if (PackageReceived != null)
                            PackageReceived(this, _receivedQueue.ToArray());

                        _receivedQueue.Clear();
                    }
                }

                i++;
            }
        }
    }
}
