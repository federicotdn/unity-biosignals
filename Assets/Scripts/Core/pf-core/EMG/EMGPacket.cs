using System;

namespace pfcore {
    public class EMGPacket {
        // Data packet received from Olimexino328
        private const int DATA_LEN = 6;
        private const int DATA_OFFSET = 4;

        public const int PACKET_SIZE = 17;
        public const byte SYNC0_BYTE = 0xA5;
        public const byte SYNC1_BYTE = 0x5A;
        public const byte VERSION_BYTE = 2;

        public byte sync0;
        public byte sync1;
        public byte version;
        public byte count;
        public UInt16[] data;
        public byte switches;
        // 17 bytes total

        public EMGPacket() {
            data = new UInt16[DATA_LEN];
        }

        public void Unpack(byte[] buf) {
            sync0 = buf[0];
            sync1 = buf[1];
            version = buf[2];
            count = buf[3];

            int dataIdx = 0;
            for (int i = DATA_OFFSET; i < DATA_OFFSET + (DATA_LEN * 2); i += 2) {
                byte upper = buf[i];
                byte lower = buf[i + 1];

                UInt16 val = (UInt16)(upper << 8);
                val += lower;
                data[dataIdx++] = val;
            }

            switches = buf[16];
        }
    }
}
