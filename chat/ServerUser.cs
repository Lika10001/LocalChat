using System.ServiceModel;

namespace chat {
	internal class ServerUser {
		public int ID { get; set; }

		public string name { get; set; }

		public OperationContext operationContext { get; set; }
	}
}
