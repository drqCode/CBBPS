namespace PredictionLogic.Prediction {
	public interface IBranch {
		BranchInfo getBranchInfo();
		bool taken();
		uint getTargetAddress();
	}
}
