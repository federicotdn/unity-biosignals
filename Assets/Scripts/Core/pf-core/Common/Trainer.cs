using System;
using System.Collections.Generic;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.Bayes;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Statistics.Analysis;
using Accord.Statistics.Distributions.Univariate;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.MachineLearning.VectorMachines;

namespace pfcore
{
	public enum ClassifierType {
		DecisionTree, LDA, Bayes, SVM
	}

	public class Trainer
	{
		private DecisionTree tree;
		private LinearDiscriminantAnalysis.Pipeline pipeline;
		private NaiveBayes<NormalDistribution> bayes;
		private SupportVectorMachine svm;
		private int featureSize;
		private ClassifierType type;
		public bool Trained { get; private set; }

		public Trainer(int featureSize, ClassifierType type)
		{
			this.featureSize = featureSize;
			this.type = type;
		}

		public void  Train(List<TrainingValue> trainingData) {
			List<DecisionVariable> trainingVariables = new List<DecisionVariable>();

			for (int i = 0; i < featureSize; i++) {
				trainingVariables.Add(DecisionVariable.Continuous(i.ToString()));
			}

			tree = new DecisionTree(inputs: trainingVariables, classes: 2);


			double[][] featuresArray = new double[trainingData.Count][];
			int[] labels = new int[trainingData.Count];

			for (int i = 0; i < featuresArray.Length; i++)
			{
				featuresArray[i] = trainingData[i].Features;
				labels[i] = Convert.ToInt32(trainingData[i].State);
			}

			switch (type) {
				case ClassifierType.DecisionTree:
					C45Learning teacher = new C45Learning(tree);
					teacher.Learn(featuresArray, labels);
					break;
				case ClassifierType.LDA:
					LinearDiscriminantAnalysis lda = new LinearDiscriminantAnalysis();
					pipeline = lda.Learn(featuresArray, labels);
					break;
				case ClassifierType.SVM:
					LinearCoordinateDescent svmLearner = new LinearCoordinateDescent();
					svm = svmLearner.Learn(featuresArray, labels);
					break;
				case ClassifierType.Bayes:
					NaiveBayesLearning<NormalDistribution> learner = new NaiveBayesLearning<NormalDistribution>();
					bayes = learner.Learn(featuresArray, labels);
					break;
			}

			Trained = true;
		}

		public int[] Predict(List<TrainingValue> predictionData)
		{
			if (!Trained)
			{
				throw new Exception("Train must be called first!");
			}

			double[][] featuresArray = new double[predictionData.Count][];

			for (int i = 0; i < featuresArray.Length; i++)
			{
				featuresArray[i] = predictionData[i].Features;
			}

			switch (type)
			{
				case ClassifierType.DecisionTree:
					return tree.Decide(featuresArray);
				case ClassifierType.LDA:
					return pipeline.Decide(featuresArray);
				case ClassifierType.SVM:
					return convertBoolArray(svm.Decide(featuresArray));
				case ClassifierType.Bayes:
					return bayes.Decide(featuresArray);
			}

			return null;
		}

		public int Predict(TrainingValue val) {
			if (!Trained)
			{
				throw new Exception("Train must be called first!");
			}

			switch(type) {
				case ClassifierType.DecisionTree:
					return tree.Decide(val.Features);
				case ClassifierType.LDA:
					return pipeline.Decide(val.Features);
				case ClassifierType.SVM:
					return Convert.ToInt32(svm.Decide(val.Features));
				case ClassifierType.Bayes:
					return bayes.Decide(val.Features);
			}

			return -1;
		}

		private static int[] convertBoolArray(bool[] a) {
			int[] ans = new int[a.Length];
			int i = 0;
			foreach (bool b in a) {
				ans[i++] = Convert.ToInt32(b);
			}

			return ans;
		}
	}
}
