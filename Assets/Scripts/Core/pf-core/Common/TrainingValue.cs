using System;

namespace pfcore
{
	public struct TrainingValue<T> where T : struct, IConvertible
	{
		public double[] Features { get; set; }
		public T State { get; set; }

		public TrainingValue(T state, int featureCount)
		{
			State = state;
			Features = new double[featureCount];
		}
	}
}
