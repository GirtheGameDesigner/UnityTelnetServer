using UnityEngine;

namespace UnityTelnet
{
    /// <summary>
    /// Unity implementation of a Telnet server.
    /// Receives input from Telnet clients and converts them into UnityEvents.
    /// </summary>
    [RequireComponent(typeof(TelnetCommandController), typeof(TelnetServer), typeof(UnityMainThreadDispatcher))]
    public class UnityTelnetManager : MonoBehaviour
    {
        public string IPAddress = "127.0.0.1";
        public int Port = 0;
        public string[] Commands = null;

        private TelnetCommandController TCC = null;
        private TelnetServer Server = null;

        private void Awake()
        {
            Server = GetComponent<TelnetServer>();
            TCC = GetComponent<TelnetCommandController>();
        }

        private void Start()
        {
            Server.SetServerData(this, Commands);

            print("Calling " + IPAddress + " and " + Port + " from " + gameObject.name);

            Server.StartServer(IPAddress, Port);
        }

        private void OnDestroy()
        {
            Server.StopServer();
        }

        internal string SendCommandToController(TelnetCommand command)
        {
            string commandResponse = TCC.FireCommand(command);

            return commandResponse;
        }

        private void PrintMessage(string logMessage)
        {
            Debug.Log(logMessage);
        }
    } 
}
