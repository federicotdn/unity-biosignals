using System;

namespace pfcore {
    class EKGPacket {
        // Data packet received from CMS50+ Contec Pulse Oximeter
        public const int PACKET_SIZE = 5;

        private const int PEAK_MASK = 0x40;
        private const int HRHIGH_MASK = 0x40;
        private const int HRLOW_MASK = 0x7F;

        public byte status;
        public byte waveform;
        public byte highHr;
        public byte lowHr;
        public byte oxygen;
        // 5 bytes total

        public long timeStamp;

        public byte HeartRate {
            get {
                return (byte)(((highHr & HRHIGH_MASK) << 1) + (lowHr & HRLOW_MASK));
            }
        }

        public bool Peak {
            get {
                return (status & PEAK_MASK) == PEAK_MASK;
            }
        }

        public void Unpack(byte[] buf) {
            status = buf[0];
            waveform = buf[1];
            highHr = buf[2];
            lowHr = buf[3];
            oxygen = buf[4];

            timeStamp = DateTime.Now.Ticks;
        }
    }
}
