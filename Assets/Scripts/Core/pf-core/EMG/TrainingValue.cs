namespace pfcore {
    public struct TrainingValue {
        public const int FEATURE_COUNT = 16;

        public double[] features;
        public MuscleState muscleState;

        public TrainingValue(MuscleState muscleState) {
            this.muscleState = muscleState;
            features = new double[FEATURE_COUNT];
        }
    }
}
