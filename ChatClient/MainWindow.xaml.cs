using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChatClient.ServiceChat;
using System.Numerics;
using Microsoft.Win32;
using System.IO;
using System.ServiceModel;
using static System.Net.Mime.MediaTypeNames;
using System.ServiceModel.Channels;

namespace ChatClient {
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>

	public class Message {

		private string time;
		private string author;
		private string text;
		private byte[] fileData;

		public Message(string time, string author, string text) {
			this.time = time;
			this.author = author;
			this.text = text;
		}

		public string getTime() {
			return time;
		}

		public string getAuthor() {
			return author;
		}

		public string getText() {
			return text;
		}

		public byte[] getFileData() {
			return fileData;
		}

		public void setFileData(byte[] fileData) {
			this.fileData = fileData;
		}
	}
	
	public class Chat {

		public int interlocutorID;
		private string interlocutorName;
		private List<Message> messages = new List<Message>();

		public Chat(int interlocutorID, string interlocutorName) {
			this.interlocutorID = interlocutorID;
			this.interlocutorName = interlocutorName;
		}

		public void setInterlocutorID(int interlocutorID) {
			this.interlocutorID = interlocutorID;
		}

		public int getInterlocutorID() {
			return interlocutorID;
		}

		public void setInterlocutorName(string interlocutorName) {
			this.interlocutorName = interlocutorName;
		}

		public string getInterlocutorName() {
			return interlocutorName;
		}

		public void addMessage(Message msg) {
			messages.Add(msg);
		}

		public List<Message> getMessages() {
			return messages;
		}
	}

	public class RSA {

		public static string encrypt(string m, BigInteger eB, BigInteger nB) {
			string c = "";

			byte[] inputBytes = Encoding.ASCII.GetBytes(m);
			int[] inputInts = new int[inputBytes.Length];
			for (int i = 0; i < inputBytes.Length; i++) {
				inputInts[i] += Convert.ToInt32(inputBytes[i]);
			}

			int[] outputInts = new int[inputInts.Length];
			string str = "";
			for (int i = 0; i < outputInts.Length; i++) {
				outputInts[i] = (int) ModPower(inputInts[i], eB, nB);
				str += outputInts[i].ToString() + " ";
			}
			c = str.Trim();

			return c;
		}

		public static byte[] encryptFile(byte[] inputBytes, BigInteger eB, BigInteger nB) {

			int[] inputInts = new int[inputBytes.Length];
			for (int i = 0; i < inputBytes.Length; i++) {
				inputInts[i] += Convert.ToInt32(inputBytes[i]);
			}

			int[] outputInts = new int[inputInts.Length];
			byte[] outputBytes = new byte[inputInts.Length * 2];
			int t = 0;
			for (int i = 0; i < outputInts.Length; i++) {
				outputInts[i] = (int) ModPower(inputInts[i], eB, nB);

				byte[] arrbyte = BitConverter.GetBytes(outputInts[i]);
				outputBytes[t++] = arrbyte[0];
				outputBytes[t++] = arrbyte[1];
			}

			return outputBytes;
		}

		public static string decrypt(string c, BigInteger eA, BigInteger pA, BigInteger qA) {
			string m = "";

			string[] inputIntsStr = c.Split(' ');
			int[] inputInts = inputIntsStr.Select(int.Parse).ToArray();

			BigInteger f = (pA - 1) * (qA - 1);
			BigInteger n = pA * qA;
			BigInteger d = findD(f, eA);

			int[] outputInts = new int[inputInts.Length];
			byte[] outputBytes = new byte[inputInts.Length];
			for (int i = 0; i < outputInts.Length; i++) {
				outputInts[i] = (int) ModPower(inputInts[i], d, n);
				outputBytes[i] = Convert.ToByte(outputInts[i]);
			}
			m = Encoding.ASCII.GetString(outputBytes);

			return m;
		}

		public static byte[] decryptFile(byte[] inputBytes, BigInteger eA, BigInteger pA, BigInteger qA) {

			int[] inputInts = new int[inputBytes.Length / 2];
			int t = 0;
			for (int i = 0; i < inputBytes.Length; i += 2) {
				byte[] arrbyte = new byte[4];
				arrbyte[0] = inputBytes[i];
				arrbyte[1] = inputBytes[i + 1];
				arrbyte[2] = 0;
				arrbyte[3] = 0;
				int intos = BitConverter.ToInt32(arrbyte, 0);
				inputInts[t++] = intos;
			}

			BigInteger f = (pA - 1) * (qA - 1);
			BigInteger n = pA * qA;
			BigInteger d = findD(f, eA);

			int[] outputInts = new int[inputInts.Length];
			byte[] outputBytes = new byte[inputInts.Length];
			for (int i = 0; i < outputInts.Length; i++) {
				outputInts[i] = (int) ModPower(inputInts[i], d, n);
				outputBytes[i] = Convert.ToByte(outputInts[i]);
			}

			return outputBytes;
		}

		private static BigInteger ModPower(BigInteger x, BigInteger y, BigInteger n) {
			if (y == 0) return 1;
			BigInteger z = ModPower(x, y / 2, n);
			if (y % 2 == 0)
				return (z * z) % n;
			else
				return (x * z * z) % n;
		}

		private static BigInteger findD(BigInteger a, BigInteger b) {

			BigInteger d0 = a;
			BigInteger d1 = b;
			BigInteger x0 = 1;
			BigInteger x1 = 0;
			BigInteger y0 = 0;
			BigInteger y1 = 1;
			BigInteger q, d2, x2, y2;

			while (d1 > 1) {
				q = d0 / d1;
				d2 = d0 % d1;
				x2 = x0 - q * x1;
				y2 = y0 - q * y1;
				d0 = d1;
				d1 = d2;
				x0 = x1;
				x1 = x2;
				y0 = y1;
				y1 = y2;
			}

			if (y1 < 0) y1 += a;

			return y1;
		}
	}

	public partial class MainWindow : Window, IServiceChatCallback {

		private ServiceChatClient client;
		private int ID;
		private int currInterlocutorID = 0;
		private bool isConnected = false;
		private List<Chat> chats = new List<Chat>();
		string username = "";

		public MainWindow() {
			InitializeComponent();
		}

		public void ConnectUser(string username) {
			client = new ServiceChatClient(new System.ServiceModel.InstanceContext(this));
			chats = new List<Chat>();
			ID = client.Connect(username);
			client.UpdateUsers();
			currInterlocutorID = 0;
		}

		public void DisconnectUser() {
			client.Disconnect(ID);
			client.UpdateUsers();
			lbChat.Items.Clear();
			lbUsers.Items.Clear();
			tbMessage.Text = string.Empty;
			client = null;
		}

		public void UpdateChats(int[] userIds, string[] userNames) {

			if (chats.Count == 0) {
				chats.Add(new Chat(0, "All"));
				chats.Add(new Chat(1, "AI Assistant"));
			}

			for (int i = 0; i < userIds.Length; i++) {
				bool isCreated = false;
				foreach (var c in chats) {
					if (c.getInterlocutorID() == userIds[i]) {
						isCreated = true;
						break;
					}
				}
				if (!isCreated && userIds[i] != ID) {
					chats.Add(new Chat(userIds[i], userNames[i]));
				}
			}

			for (int i = 2; i < chats.Count; i++) {
				bool isPresent = false;
				foreach (var u in userIds) {
					if (chats[i].getInterlocutorID() == u) {
						isPresent = true;
						break;
					}
				}
				if (!isPresent) {
					chats.RemoveAt(i);
				}
			}
		}

		public void UsersCallback(int[] userIds, string[] userNames) {
			UpdateChats(userIds, userNames);
			UpdateUsersListBox();
		}

		public int GetChatId(int interlocutorID) {
			for (int i = 0; i < chats.Count; i++) {
				if (chats[i].getInterlocutorID() == interlocutorID) {
					return i;
				}
			}
			return -1;
		}

		public string GetMessageStr(string time, string author, string text) {
			return time + " " + author + "    " + text;
		}

		public void MsgCallback(string time, string author, string text, int idSender, int idReceiver) {
			if (idSender != ID) {
				if (idReceiver != 0 && idSender != 0) {
					try {
						BigInteger eA = BigInteger.Parse(tbEA.Text);
						BigInteger pA = BigInteger.Parse(tbPA.Text);
						BigInteger qA = BigInteger.Parse(tbQA.Text);
						text = RSA.decrypt(text, eA, pA, qA);
					} catch (Exception e) {
						text = "Message can not be decrypted";
					}
				}
				if ((idSender == currInterlocutorID && idReceiver != 0) ||
					((idSender == 0 || idReceiver == 0) && currInterlocutorID == 0)) {
					lbChat.Items.Add(GetMessageStr(time, author, text));
					lbChat.ScrollIntoView(lbChat.Items[lbChat.Items.Count - 1]);
				}
				if (idReceiver == 0) {
					chats[0].addMessage(new Message(time, author, text));
				} else {
					int chatId = GetChatId(idSender);
					chats[chatId].addMessage(new Message(time, author, text));
				}
			}
		}

		public void MsgChatCallback(string time, string author, string text) {
			if (currInterlocutorID == 1) {
				lbChat.Items.Add(GetMessageStr(time, author, text));
				lbChat.ScrollIntoView(lbChat.Items[lbChat.Items.Count - 1]);
			}
			chats[1].addMessage(new Message(time, author, text));
		}

		public void FileCallback(string time, string author, string name, byte[] fileData, int idSender, int idReceiver) {
			if (idSender != ID) {

				if (idReceiver != 0 && idSender != 0) {
					try {
						BigInteger eA = BigInteger.Parse(tbEA.Text);
						BigInteger pA = BigInteger.Parse(tbPA.Text);
						BigInteger qA = BigInteger.Parse(tbQA.Text);
						fileData = RSA.decryptFile(fileData, eA, pA, qA);
					} catch (Exception e) { }
				}

				Message message = new Message(time, author, "@" + name);
				message.setFileData(fileData);

				if ((idSender == currInterlocutorID && idReceiver != 0) ||
					((idSender == 0 || idReceiver == 0) && currInterlocutorID == 0)) {
					lbChat.Items.Add(GetMessageStr(time, username, message.getText()));
					lbChat.ScrollIntoView(lbChat.Items[lbChat.Items.Count - 1]);
				}
				if (idReceiver == 0) {
					chats[0].addMessage(message);
				} else {
					int chatId = GetChatId(idSender);
					chats[chatId].addMessage(message);
				}
			}
		}

		private void Border_MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.LeftButton == MouseButtonState.Pressed) {
				DragMove();
			}
		}

		private void ButtonMinimize_Click(object sender, RoutedEventArgs e) {
			System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
		}

		private void WindowStateButton_Click(object sender, RoutedEventArgs e) {
			if (System.Windows.Application.Current.MainWindow.WindowState != WindowState.Maximized) {
				System.Windows.Application.Current.MainWindow.WindowState = WindowState.Maximized;
			} else {
				System.Windows.Application.Current.MainWindow.WindowState = WindowState.Normal;
			}
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e) {
			System.Windows.Application.Current.Shutdown();
		}

		private void lbConnDisconn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			if (isConnected) {
				brdConnDisconn.Background = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#ff523b"));
				lbConnDisconn.Content = "Disconnected";
				lbUsername.Content = "Username";
				username = "";
				isConnected = false;
				tbUsername.IsReadOnly = false;
				DisconnectUser();
			} else {
				if (tbUsername.Text.Length > 0) {
					brdConnDisconn.Background = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#3BFF6F"));
					lbConnDisconn.Content = "Connected";
					lbUsername.Content = "All";
					username = tbUsername.Text;
					tbUsername.IsReadOnly = true;
					isConnected = true;
					ConnectUser(username);
				}
			}
		}

		public void UpdateChat() {
			lbChat.Items.Clear();
			List<Message> messages = chats[GetChatId(currInterlocutorID)].getMessages();
			foreach (var message in messages) {
				lbChat.Items.Add(GetMessageStr(message.getTime(), message.getAuthor(), message.getText()));
			}
			if (lbChat.Items.Count > 0) {
				lbChat.ScrollIntoView(lbChat.Items[lbChat.Items.Count - 1]);
			}
		}

		private void lbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (lbUsers.SelectedIndex > -1) {
				int prevID = currInterlocutorID;
				currInterlocutorID = client.GetUserIdByName((string) lbUsers.Items[lbUsers.SelectedIndex]);
				lbUsername.Content = (string) lbUsers.Items[lbUsers.SelectedIndex];
				if (prevID != currInterlocutorID) {
					UpdateChat();
				}
			}
		}

		private void lbChat_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			Chat chat = chats[GetChatId(currInterlocutorID)];
			var messages = chat.getMessages();
			var message = new Message("", "", "");
			if (lbChat.SelectedIndex < messages.Count && lbChat.SelectedIndex != -1) {
				message = messages[lbChat.SelectedIndex];
			}

			if (message.getText().Length > 0 && message.getText()[0] == '@') {
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				string fileName = message.getText().Substring(1);
				saveFileDialog.FileName = fileName;
				if (saveFileDialog.ShowDialog() == true) {
					string savePath = saveFileDialog.FileName;
					File.WriteAllBytes(savePath, message.getFileData());
				}
			}
		}

		private void UpdateUsersListBox() {
			lbUsers.Items.Clear();
			foreach (var c in chats) {
				lbUsers.Items.Add(c.getInterlocutorName());
			}
		}

		public void SendMessage() {
			string time = DateTime.Now.ToShortTimeString();
			string text = tbMessage.Text;
			chats[GetChatId(currInterlocutorID)].addMessage(new Message(time, username, text));
			lbChat.Items.Add(GetMessageStr(time, username, text));
			lbChat.ScrollIntoView(lbChat.Items[lbChat.Items.Count - 1]);
			if (currInterlocutorID == 1) {
				client.SendMsgChat(tbMessage.Text, ID);
			} else {
				if (currInterlocutorID > 1) {
					BigInteger eB = BigInteger.Parse(tbEB.Text);
					BigInteger nB = BigInteger.Parse(tbNB.Text);
					string encryptedMsg = RSA.encrypt(tbMessage.Text, eB, nB);
					client.SendMsg(encryptedMsg, ID, currInterlocutorID);
				} else {
					client.SendMsg(tbMessage.Text, ID, currInterlocutorID);
				}
			}
			tbMessage.Text = string.Empty;
		}

		private void tbMessage_KeyDown(object sender, KeyEventArgs e) {
			if (isConnected && e.Key == Key.Enter && tbMessage.Text.Count() > 0 && client != null) {
				SendMessage();
			}
		}

		private void imSendMsg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			if (isConnected &&  client != null && tbMessage.Text.Count() > 0) {
				SendMessage();
			}
		}

		private void Window_Closed(object sender, EventArgs e) {
			if (isConnected) {
				DisconnectUser();
			}
		}

		private void SendFileToServer(string filePath) {
			try {
				using (FileStream fs = new FileStream(filePath, FileMode.Open)) {

					byte[] fileData = new byte[fs.Length];
					fs.Read(fileData, 0, fileData.Length);

					string time = DateTime.Now.ToShortTimeString();
					string fileName = System.IO.Path.GetFileName(filePath);
					Message message = new Message(time, username, "@" + fileName);
					message.setFileData(fileData);
					chats[GetChatId(currInterlocutorID)].addMessage(message);
					lbChat.Items.Add(GetMessageStr(time, username, message.getText()));
					lbChat.ScrollIntoView(lbChat.Items[lbChat.Items.Count - 1]);

					if (currInterlocutorID != 0) {
						BigInteger eB = BigInteger.Parse(tbEB.Text);
						BigInteger nB = BigInteger.Parse(tbNB.Text);
						fileData = RSA.encryptFile(fileData, eB, nB);
					}
					client.SendFile(fileName, fileData, ID, currInterlocutorID);
				}
			} catch (Exception ex) {
				MessageBox.Show("Can not send message to server: " + ex.Message, "Error");
			}
		}

		private void imSendFile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			if (isConnected) {
				OpenFileDialog openFileDialog = new OpenFileDialog();
				if (openFileDialog.ShowDialog() == true) {
					string filePath = openFileDialog.FileName;
					SendFileToServer(filePath);
				}
			}
		}
	}
}
