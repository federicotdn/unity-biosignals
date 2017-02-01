using System;
using System.Collections.Generic;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.MachineLearning.DecisionTrees;

namespace pfcore
{
	public class EEGTrainer
	{
		private DecisionTree tree;

		public EEGTrainer()
		{
		}



		public void Train(List<EEGTrainingValue> trainingData)
		{

			tree = new DecisionTree(
				inputs: new List<DecisionVariable>
					{
						DecisionVariable.Continuous("X"),
						DecisionVariable.Continuous("Y"),
						DecisionVariable.Continuous("Z"),
						DecisionVariable.Continuous("W")
					},
				classes: 2);

			double[][] featuresArray = new double[trainingData.Count][];
			int[] outputs = new int[trainingData.Count];

			for (int i = 0; i < featuresArray.Length; i++)
			{
				featuresArray[i] = trainingData[i].Features;
				outputs[i] = (int)trainingData[i].Status;
			}

			C45Learning teacher = new C45Learning(tree);
			teacher.Learn(featuresArray, outputs);
		}

		public int[] Predict(List<EEGTrainingValue> predictionData)
		{
			if (tree == null)
			{
				throw new Exception("Train must be called first!");
			}

			double[][] featuresArray = new double[predictionData.Count][];

			for (int i = 0; i < featuresArray.Length; i++)
			{
				featuresArray[i] = predictionData[i].Features;
			}

			return tree.Decide(featuresArray);
		}
	}
}
