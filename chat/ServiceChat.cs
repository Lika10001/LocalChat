using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using OpenAI_API;
using OpenAI_API.Chat;
using static System.Net.Mime.MediaTypeNames;

namespace chat {

	public class AiChat {

		private Conversation conversation;
		private int userId;

		public AiChat(int userId, OpenAIAPI api) {
			this.userId = userId;
			conversation = api.Chat.CreateConversation();
		}

		public int getUserId() {
			return userId;
		}

		public Conversation getConversation() {
			return conversation;
		}
	}

	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class ServiceChat : IServiceChat {

		List<ServerUser> users = new List<ServerUser>();
		int nextId = 2;
		OpenAIAPI api = new OpenAIAPI("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");
		List<AiChat> aiChats = new List<AiChat>();

		public async Task<string> GetChatResponseAsync(string userInput, int idReceiver) {

			var chat = aiChats.FirstOrDefault(i => i.getUserId() == idReceiver).getConversation();
			chat.AppendUserInput(userInput);
			string response = await chat.GetResponseFromChatbotAsync();

			return response;
		}

		public int Connect(string name) {

			aiChats.Add(new AiChat(nextId, api));

			ServerUser user = new ServerUser() {
				ID = nextId,
				name = name,
				operationContext = OperationContext.Current
			};
			nextId++;
			SendMsg(user.name + " entered the chat!", 0, 0);
			users.Add(user);

			return user.ID;
		}

		public void Disconnect(int id) {
			var user = users.FirstOrDefault(i => i.ID == id);
			if (user != null) {
				users.Remove(user);
				SendMsg(user.name + " left the chat!", 0, 0);
			}
			var chat = aiChats.FirstOrDefault(i => i.getUserId() == id);
			if (user != null) {
				aiChats.Remove(chat);
			}
		}

		public int GetUserIdByName(string name) {
			foreach (var u in users) {
				if (u.name == name) {
					return u.ID;
				}
			}
			if (name.Equals("All")) return 0;
			if (name.Equals("AI Assistant")) return 1;
			return 0;
		}

		public string GetUserNameById(int id) {
			if (id == 0) return "";
			if (id == 1) return "AI Assistant";
			foreach (var u in users) {
				if (u.ID == id) {
					return u.name;
				}
			}
			return "---";
		}

		public void SendMsg(string text, int idSender, int idReceiver) {
			string time = DateTime.Now.ToShortTimeString();
			foreach (var u in users) {
				if (idReceiver == 0 || u.ID == idReceiver) {
					u.operationContext.GetCallbackChannel<IServiceChatCallback>().MsgCallback(time, GetUserNameById(idSender), text, idSender, idReceiver);
				}
			}
		}

		public async void SendMsgChat(string text, int idReceiver) {
			string answer = await GetChatResponseAsync(text, idReceiver);
			string time = DateTime.Now.ToShortTimeString();
			foreach (var u in users) {
				if (u.ID == idReceiver) {
					u.operationContext.GetCallbackChannel<IServiceChatCallback>().MsgChatCallback(time, GetUserNameById(1), answer);
				}
			}
		}

		public void UpdateUsers() {
			List<int> userIds = new List<int>();
			List<string> userNames = new List<string>();
			foreach (var u in users) {
				userIds.Add(u.ID);
				userNames.Add(u.name);
			}
			foreach (var u in users) {
				u.operationContext.GetCallbackChannel<IServiceChatCallback>().UsersCallback(userIds.ToArray(), userNames.ToArray());
			}
		}

		public void SendFile(string name, byte[] fileData, int idSender, int idReceiver) {
			string time = DateTime.Now.ToShortTimeString();
			foreach (var u in users) {
				if (idReceiver == 0 || u.ID == idReceiver) {
					u.operationContext.GetCallbackChannel<IServiceChatCallback>()
						.FileCallback(time, GetUserNameById(idSender), name, fileData, idSender, idReceiver);
				}
			}
		}
	}
}
