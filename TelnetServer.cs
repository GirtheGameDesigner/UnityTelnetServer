using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using UnityEngine;

namespace UnityTelnet
{
    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    /// <summary>
    /// Telnet Server that reads multiple client inputs and submits commands.
    /// </summary>
    public class TelnetServer : MonoBehaviour
    {
        public delegate void LogHandler(string logMessage);

        public LogHandler OnLogReceived;

        private UnityTelnetManager UTM = null;
        private string[] _commandKeywords = null;

        Socket newSocket = null;

        /// <summary>
        /// Should be called before starting server. Sets necessary information for using the Telnet server.
        /// </summary>
        /// <param name="manager">UnityTelnetManager should be contained here.</param>
        /// <param name="commands">CSV that was parsed in UnityTelnetManager.</param>
        public void SetServerData(UnityTelnetManager manager, string[] commands)
        {
            // Sets the manager
            UTM = manager;

            // Sets the keyword array
            _commandKeywords = commands;
        }

        /// <summary>
        /// Binds the network info for the server and begins listening for connections.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public void StartServer(string ipAddress, int port = 100)
        {
            IPAddress serverIP = IPAddress.Parse(ipAddress).MapToIPv4();
            IPEndPoint endpoint = new IPEndPoint(serverIP, port);

            newSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                print(gameObject.name + " Starting server...");

                newSocket.Bind(endpoint);
                newSocket.Listen(3);

                print(gameObject.name + " Server Address is: " + serverIP.ToString());
                print(gameObject.name + " Port Address is: " + port);
                print(gameObject.name + " Server Ready!");

                // Start an asynchronous socket to listen for connections.  
                print(gameObject.name + " Waiting for a connection...");
                newSocket.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    newSocket);

                GC.Collect();

            } catch (SocketException e)
            {
                print("Could not connect! Here's the error code: " + e.ErrorCode.ToString());
            }
        }

        // Accepts client connection and begins receiving data
        private void AcceptCallback(IAsyncResult AR)
        {
            // Get the socket the handles the client request.
            Socket listener = (Socket)AR.AsyncState;
            Socket handler = listener.EndAccept(AR);

            print("Client Connected!");

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);

            // Begin the process of listening for another client.
            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
        }

        // Takes input from client and works through it.
        // Calls itself again after input to allow multiple commands to fire per connection.
        private void ReceiveCallback(IAsyncResult AR)
        {
            String content = string.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)AR.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(AR);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Check for \r\n at the end of the line. If it is not there, read more data.  
                content = state.sb.ToString();

                if (content.IndexOf("\r\n") > -1)
                {
                    // Process content and return a byte array to send back to the client.
                    byte[] data = ProcessContent(content);

                    // Look for another input from the client.
                    state.sb.Clear();
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);

                    // Send a message over to the client.
                    handler.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), handler);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
                }
            }
        }

        // Receives raw text and parses it.
        private byte[] ProcessContent(string content)
        {
            byte[] data;

            // Format content.
            content = content.ToLower();

            // Strip out line breaks and spaces.
            content = content.Replace(" ", string.Empty);
            content = content.Replace("\r\n", string.Empty);

            // Edge-Case Stripping on PuTTY.
            content = content.Replace("'", string.Empty);
            content = content.Replace("?", string.Empty);
            content = content.Replace("", string.Empty);
            content = content.Replace("", string.Empty);
            content = content.Replace("", string.Empty);

            print("Formatted Text: " + content);

            // Search for valid command in Dictionary.
            TelnetCommand cmd = SearchForValidCommand(content);

            string response;

            if (cmd.commandName != string.Empty)
            {
                // Get response from the valid command.
                response = UTM.SendCommandToController(cmd);
            }
            else
            {
                response = "! INVALID COMMAND\r\n";
            }

            data = Encoding.ASCII.GetBytes(response);

            print("Response is: " + response);

            return data;
        }

        // Takes a string and searches for a valid command to return.
        private TelnetCommand SearchForValidCommand(string text)
        {
            TelnetCommand command = new TelnetCommand();

            command.commandName = string.Empty;
            command.commandParam = null;

            // Strip commandParams from the text if applicable and assign to the TelnetCommand.
            string b = string.Empty;
            bool isNegative = false;

            // Check for symbols and integers in the string and apply it as a commmandParam.
            if (text.Contains("-"))
            {
                text = text.Replace("-", string.Empty);
                isNegative = true;
            }
            b = String.Join("", text.Where(char.IsDigit));

            if (b.Length > 0)
            {
                command.commandParam = int.Parse(b);
                if (isNegative)
                {
                    command.commandParam *= -1;
                }

                text = text.Replace(b, string.Empty);
            }
            
            // Search all command keywords for a valid command.
            if (_commandKeywords.Length > 0)
            {
                for (int i = 0; i < _commandKeywords.Length; i++)
                {
                    if (_commandKeywords[i] == text)
                    {
                        command.commandName = _commandKeywords[i];
                        break;
                    }
                }
            }

            return command;
        }

        // Sends a response to the client.
        private void SendCallback(IAsyncResult AR)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)AR.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(AR);

                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);

            } catch(Exception e)
            {
                print(e.ToString());
            }
        }

        /// <summary>
        /// Closes the server.
        /// </summary>
        public void StopServer()
        {
            print("Server Closed!");
            newSocket.Close();
        }
    }
}