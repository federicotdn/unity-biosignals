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

		internal static List<TrainingValue> getTrainingValues(string filepath) {
			EEGReader reader;
			reader = new EEGFileReader(filepath);
			EEGProcessor processor = new EEGProcessor(reader, true);

			processor.Training = true;

			processor.Start();
			while (!processor.Finished)
			{
				processor.Update();
			}

			return processor.TrainingValues;
		}
	}
}
