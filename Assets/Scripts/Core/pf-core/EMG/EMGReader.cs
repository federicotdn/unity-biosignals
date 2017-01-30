namespace pfcore {
    public abstract class EMGReader {
        protected ConcurrentQueue<EMGPacket> packetQueue = new ConcurrentQueue<EMGPacket>();
        protected int maxQueueSize;
        protected bool running = true;

        public EMGReader(int maxQueueSize) {
            this.maxQueueSize = maxQueueSize;
        }

        abstract public void Start();

        public void Stop() {
            running = false;
        }

        public bool TryDequeue(out EMGPacket packet) {
            return packetQueue.TryDequeue(out packet);
        }
    }
}
