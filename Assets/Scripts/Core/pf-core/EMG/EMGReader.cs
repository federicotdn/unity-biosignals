namespace pfcore {
    public abstract class EMGReader {
        protected ConcurrentQueue<EMGPacket> packetQueue = new ConcurrentQueue<EMGPacket>();
        protected readonly int maxQueueSize;
        protected bool running = true;
        public bool Running {
            get {
                return running;
            }
        }

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
