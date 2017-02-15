using System;
using System.Collections.Generic;

namespace pfcore
{
	public class EEGAnalysis
	{
		private string trainingSet;
		private string predictionSet;

		public EEGAnalysis(string trainingSet, string predictionSet)
		{
			this.trainingSet = trainingSet;
			this.predictionSet = predictionSet;
		}

		public void Start()
		{
			EEGReader reader;
			reader = new EEGFileReader(trainingSet, false);
			EEGProcessor processor = new EEGProcessor(reader);


			processor.Training = true;
			processor.Start();
			while (!processor.Finished)
			{
				processor.Update();
			}

			List<TrainingValue> trainingData = processor.TrainingValues;
			List<TrainingValue> alphaTrainingData = processor.AlphaTrainingValues;

			Trainer trainer = new Trainer(EEGProcessor.FEATURE_COUNT, ClassifierType.DecisionTree);
			trainer.Train(trainingData);


			reader = new EEGFileReader(predictionSet, false);
			processor = new EEGProcessor(reader);

			processor.Start();
			while (!processor.Finished)
			{
				processor.Update();
			}

			List<TrainingValue> predictionData = processor.TrainingValues;
			List<TrainingValue> alphaPredictionData = processor.AlphaTrainingValues;

			trainAndAnalize(trainingData, predictionData, EEGProcessor.FEATURE_COUNT);
			trainAndAnalize(alphaTrainingData, alphaPredictionData, 2);
		}

		internal static List<TrainingValue> getTrainingValues(string filepath) {
			EEGReader reader;
			reader = new EEGFileReader(filepath, false);
			EEGProcessor processor = new EEGProcessor(reader);


			processor.Training = true;
			processor.Start();
			while (!processor.Finished)
			{
				processor.Update();
			}

			return processor.TrainingValues;
		}

		private void trainAndAnalize(List<TrainingValue> trainingData, List<TrainingValue> predictionData, int featureSize)
		{
			Trainer trainer = new Trainer(featureSize, ClassifierType.DecisionTree);
			trainer.Train(trainingData);


			int[] outputs = trainer.Predict(predictionData);

			int[,] confusionMatrix = new int[2, 2];
			for (int i = 0; i < outputs.Length; i++)
			{
				if (predictionData[i].State == (int)EyesStatus.CLOSED)
				{
					if (outputs[i] == (int)EyesStatus.CLOSED)
					{
						confusionMatrix[0, 0]++;
					}
					else
					{
						confusionMatrix[0, 1]++;
					}
				}
				else
				{
					if (outputs[i] == (int)EyesStatus.OPEN)
					{
						confusionMatrix[1, 1]++;
					}
					else
					{
						confusionMatrix[1, 0]++;
					}
				}
			}

			Console.WriteLine("Finished training and predicting.");
			Console.WriteLine("\nConfusion matrix: \n");
			Console.WriteLine("  C     O");

			int truePositive = confusionMatrix[0, 0];
			int falseNegative = confusionMatrix[0, 1];
			int trueNegative = confusionMatrix[1, 1];
			int falsePositive = confusionMatrix[1, 0];

			float SE = truePositive / (float)(truePositive + falseNegative);
			float SP = trueNegative / (float)(trueNegative + falsePositive);

			Console.WriteLine("C " + confusionMatrix[0, 0] + "   " + confusionMatrix[0, 1]);
			Console.WriteLine("O " + confusionMatrix[1, 0] + "   " + confusionMatrix[1, 1]);
			Console.WriteLine("\nAAC: " + (confusionMatrix[0, 0] + confusionMatrix[1, 1]) / (float)(predictionData.Count));

			Console.WriteLine("Sensitivity: " + SE);
			Console.WriteLine("Specificity: " + SP);
		}
	}
}
