using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace chat {
	[ServiceContract(CallbackContract = typeof(IServiceChatCallback))]
	public interface IServiceChat {
		[OperationContract]
		int Connect(string name);

		[OperationContract]
		void Disconnect(int id);

		[OperationContract(IsOneWay = true)]
		void SendMsg(string text, int idSender, int idReceiver);

		[OperationContract(IsOneWay = true)]
		void SendMsgChat(string text, int idReceiver);

		[OperationContract(IsOneWay = true)]
		void SendFile(string name, byte[] fileData, int idSender, int idReceiver);

		[OperationContract(IsOneWay = true)]
		void UpdateUsers();

		[OperationContract]
		int GetUserIdByName(string name);

		[OperationContract]
		string GetUserNameById(int id);
	}

	public interface IServiceChatCallback {
		[OperationContract(IsOneWay = true)]
		void MsgCallback(string time, string author, string text, int idSender, int idReceiver);

		[OperationContract(IsOneWay = true)]
		void MsgChatCallback(string time, string author, string text);

		[OperationContract(IsOneWay = true)]
		void UsersCallback(int[] userIds, string[] userNames);

		[OperationContract(IsOneWay = true)]
		void FileCallback(string time, string author, string name, byte[] fileData, int idSender, int idReceiver);
	}
}
