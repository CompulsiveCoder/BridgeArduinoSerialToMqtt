using System;
using duinocom;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using System.Configuration;

namespace CSharpReadArduinoSerial
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var userId = ConfigurationSettings.AppSettings["UserId"];
			var pass = ConfigurationSettings.AppSettings["Password"];
			var host = ConfigurationSettings.AppSettings["Host"];
			var topic = ConfigurationSettings.AppSettings["Topic"];
			var topicPrefix = "/" + userId + "/";

			var detector = new DuinoPortDetector ();
			var port = detector.Guess ();
			//Console.WriteLine (port.PortName);
			int i = 0;

			using (var communicator = new DuinoCommunicator (port.PortName))
			{
				//communicator.LongPause = 500;
				//communicator.ReallyShortPause = 10;
				communicator.Open ();

				var isRunning = true;

				var client = new MqttClient (host);

				var clientId = Guid.NewGuid ().ToString ();

				client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
				client.Connect (clientId, userId, pass);

				while (isRunning) {
					var output = communicator.Read ();

					if (!String.IsNullOrEmpty (output.Trim ())) {
						Console.WriteLine (output);

						var fullTopic = topicPrefix + topic;

						Console.WriteLine (fullTopic);

						client.Publish (fullTopic, Encoding.UTF8.GetBytes (output));
					}
					
					Thread.Sleep (1);
				}

				communicator.Close ();
			}
		}

		// this code runs when a message was received
		public static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			string ReceivedMessage = Encoding.UTF8.GetString(e.Message);

			Console.WriteLine (ReceivedMessage);
		}
	}
}
