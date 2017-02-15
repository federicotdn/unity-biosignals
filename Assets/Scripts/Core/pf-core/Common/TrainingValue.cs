using System;

namespace pfcore
{
	public struct TrainingValue
	{
		public double[] Features { get; set; }
		public int State { get; set; }

		public TrainingValue(int state, int featureCount)
		{
			State = state;
			Features = new double[featureCount];
		}
	}
}
