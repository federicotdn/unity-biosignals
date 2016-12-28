using System;

using Accord.MachineLearning;

namespace pfcore
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            Console.WriteLine("Hello from pfcore.Main");
        }

        public static string TestAccordClass() {
            MinimumMeanDistanceClassifier t = new MinimumMeanDistanceClassifier();
            return t.ToString();
        }
	}
}
