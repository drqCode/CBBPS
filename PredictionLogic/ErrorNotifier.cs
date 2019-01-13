namespace PredictionLogic {
	public static class ErrorNotifier {
		public static ErrorNotifierAction errorNotifierAction = null;

		public static void showError(string message) {
			if (errorNotifierAction != null) {
				errorNotifierAction.showError(message);
			}
		}
	}
}
