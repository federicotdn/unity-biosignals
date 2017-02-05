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

			List<EEGTrainingValue> trainingData = processor.TrainingValues;
			List<EEGTrainingValue> alphaTrainingData = processor.AlphaTrainingValues;

			EEGTrainer trainer = new EEGTrainer();
			trainer.Train(trainingData);


			reader = new EEGFileReader(predictionSet, false);
			processor = new EEGProcessor(reader);

			processor.Start();
			while (!processor.Finished)
			{
				processor.Update();
			}

			List<EEGTrainingValue> predictionData = processor.TrainingValues;
			List<EEGTrainingValue> alphaPredictionData = processor.AlphaTrainingValues;

			trainAndAnalize(trainingData, predictionData);
			trainAndAnalize(alphaTrainingData, alphaPredictionData);
		}

		private void trainAndAnalize(List<EEGTrainingValue> trainingData, List<EEGTrainingValue> predictionData)
		{
			EEGTrainer trainer = new EEGTrainer();
			trainer.Train(trainingData);


			List<EyesStatus> outputs = trainer.Predict(predictionData);

			int[,] confusionMatrix = new int[2, 2];
			for (int i = 0; i < outputs.Count; i++)
			{
				if (predictionData[i].Status == EyesStatus.CLOSED)
				{
					if (outputs[i] == EyesStatus.CLOSED)
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
					if (outputs[i] == EyesStatus.OPEN)
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
