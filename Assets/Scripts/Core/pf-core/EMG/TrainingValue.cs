namespace pfcore {
    public struct TrainingValue {
        public const int FEATURE_COUNT = 2;
        public const float LOWER_FREQ = 50;
        public const float HIGHER_FREQ = 150;

        public double[] features;
        public MuscleState muscleState;

        public TrainingValue(MuscleState muscleState) {
            this.muscleState = muscleState;
            features = new double[FEATURE_COUNT];
        }
    }
}
