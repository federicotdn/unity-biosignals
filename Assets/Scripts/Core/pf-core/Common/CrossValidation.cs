using System;
using System.Collections.Generic;

namespace pfcore
{
	public class CrossValidation
	{
		public static void CrossValidate(List<List<TrainingValue>> dataSets, List<int> ks, int featureSize, List<string> filenames)
		{

			List<ClassifierType> types = new List<ClassifierType> {
				ClassifierType.Bayes,
				ClassifierType.DecisionTree,
				ClassifierType.LDA,
				ClassifierType.SVM,
			};


			double[] typeAvgs = new double[types.Count];
			int dataSetIndex = 0;
			foreach (List<TrainingValue> data in dataSets)
			{
				Console.WriteLine("\nFile: " + filenames[dataSetIndex++]);
				int typeIndex = 0;
				foreach (ClassifierType type in types)
				{
					Console.WriteLine("\nClassifier: " + type);
					Console.WriteLine("========================================\n");

					double typeAvg = 0;
					foreach (int k in ks)
					{
						int toDiscard = data.Count % k;
						data.RemoveRange(data.Count - toDiscard, toDiscard);

						int sampleSize = data.Count / k;

						List<TrainingValue> workingCopy = new List<TrainingValue>();
						double avg = 0;
						for (int index = 0; index < k; index++)
						{
							workingCopy.AddRange(data);
							List<TrainingValue> sample = workingCopy.GetRange(index * sampleSize, sampleSize);
							workingCopy.RemoveRange(index * sampleSize, sampleSize);
							Trainer trainer = new Trainer(featureSize, type);
							trainer.Train(workingCopy);


							int[,] confMat = new int[2, 2];

							foreach (TrainingValue predValue in sample)
							{
								int result = trainer.Predict(predValue);

								int i = (predValue.State == 1) ? 1 : 0;
								int j = (result == 1) ? 1 : 0;

								confMat[i, j]++;
							}

							avg += (confMat[0, 0] + confMat[1, 1]) / (double)sampleSize;

							workingCopy.Clear();
						}
						avg /= k;
						typeAvg += avg;
						Console.WriteLine("k = " + k + ": " + avg);
					}
					typeAvgs[typeIndex++] += typeAvg / ks.Count;
					Console.WriteLine("Total average: " + typeAvg / ks.Count);
					Console.WriteLine("");
				}
			}

			Console.WriteLine("\n\nClasifiers precision");
			Console.WriteLine("========================================\n");

			int aux = 0;
			foreach (ClassifierType type in types)
			{
				Console.WriteLine(type + ": " + typeAvgs[aux++] / dataSets.Count);
			}
		}

	}
}
