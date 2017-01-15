using System;
namespace pfcore
{
	public class MaskUtils
	{
		public static float[] applyGaussianMask(float[] values, float sigma, int size)
		{
			float[] mask = buildGaussianMask(size, sigma);
			float[] ans = new float[values.Length];

			for (int i = 0; i < ans.Length; i++)
			{
				float sum = 0;
				for (int j = 0; j < size; j++)
				{
					sum += mask[j] * getValue(values, i - (size / 2) + j);
				}
				sum /= size;
				ans[i] = sum;
			}

			return ans;
		}

		private static float getValue(float[] values, int index)
		{
			if (index < 0)
			{
				return values[0];
			}
			else if (index >= values.Length)
			{
				return values[values.Length - 1];
			}
			else
			{
				return values[index];
			}
		}

		private static float[] buildGaussianMask(int size, float sigma)
		{
			float[] mask = new float[size];
			for (int i = -size / 2; i <= size / 2; i++)
			{
				mask[i + size / 2] = (float)((1 / (Math.Sqrt(2 * Math.PI) * sigma)) * Math.Exp(-(i * i) / (2 * sigma * sigma)));
			}

			return mask;
		}
	}
}
