namespace pfcore {
    struct EMGReading {
        public float value;
        public long timeStamp;

        public EMGReading(float value, long timeStamp) {
            this.value = value;
            this.timeStamp = timeStamp;
        }
    }
}
