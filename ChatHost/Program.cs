﻿using System;
using System.ServiceModel;

namespace ChatHost {
	class Program {
		static void Main(string[] args) {

			using (var host = new ServiceHost(typeof(chat.ServiceChat))) {
				host.Open();
				Console.WriteLine("Host is running");
				Console.ReadLine();
			}
		}
	}
}
