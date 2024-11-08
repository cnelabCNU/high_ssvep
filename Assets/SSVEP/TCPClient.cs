using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

public class BackendController : MonoBehaviour
{
    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    private bool isConnected = false;

    public string serverIP = "127.0.0.1";
    public int serverPort = 887;

    public ButtonState buttonState = ButtonState.Inactive;
    public string stimuliFrequency = "";
    public bool isStimuliActive = false; 

    // Connection state enum
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Error
    }

    public ConnectionState currentState { get; private set; } = ConnectionState.Disconnected;

    void Start()
    {
        ConnectToServer();
        StartCoroutine(TryReconnect());
       
    }

    void OnDestroy()
    {
        DisconnectFromServer();
    }

    private IEnumerator TryReconnect()
    {
        while (true)
        {
            if (currentState != ConnectionState.Connected)
            {
                ConnectToServer();
            }
            yield return new WaitForSeconds(5f); // Try every 5 seconds
        }
    }

    private void ConnectToServer()
    {
        try
        {
            currentState = ConnectionState.Connecting;
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to server: {e.Message}");
            currentState = ConnectionState.Error;
        }
    }

    private void ListenForData()
    {
        try
        {
            socketConnection = new TcpClient(serverIP, serverPort);
            byte[] buffer = new byte[4096];

            currentState = ConnectionState.Connected;
            isConnected = true;

            while (isConnected)
            {
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    int length;
                    while ((length = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        var incomingData = new byte[length];
                        Array.Copy(buffer, 0, incomingData, 0, length);
                        string jsonString = Encoding.UTF8.GetString(incomingData);

                        // Process JSON data on the main thread
                        ProcessReceivedData(jsonString);
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.LogError($"Socket exception: {socketException.Message}");
            currentState = ConnectionState.Error;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error receiving data: {e.Message}");
            currentState = ConnectionState.Error;
        }
        finally
        {
            DisconnectFromServer();
        }
    }

    private void ProcessReceivedData(string jsonData)
    {
        // Execute on main thread since Unity API calls are not thread-safe
        //UnityMainThreadDispatcher.Instance().Enqueue(() =>
        //   {
        try
        {
            if (isStimuliActive)
            {
                // Example: Deserialize JSON to a dictionary
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
                // Process your data here
                Debug.Log($"Received data: {data["Action"]}");
                //PogressBar progressBar = online_design.freq_stimuliidx[data["Frequency"]];

                if (data["Action"] == "Hover" && buttonState == ButtonState.Idle)
                {
                    buttonState = ButtonState.Hover;
                    stimuliFrequency = data["Frequency"]; 

                }
                else if (data["Action"] == "Cancel" && buttonState == ButtonState.Hover)
                {
                    buttonState = ButtonState.Cancel;

                }
                else if (data["Action"] == "Selection" && buttonState == ButtonState.Hover)
                {
                    buttonState = ButtonState.Selection;
                }
            }
                    }
        catch (Exception e)
        {
            Debug.LogError($"Error processing JSON data: {e.Message}");
        }
        //});
    }

    public void DisconnectFromServer()
    {
        isConnected = false;

        if (socketConnection != null)
        {
            socketConnection.Close();
            socketConnection = null;
        }

        if (clientReceiveThread != null)
        {
            clientReceiveThread.Abort();
            clientReceiveThread = null;
        }

        currentState = ConnectionState.Disconnected;
    }

    // Public method to check connection state
    public bool IsConnected()
    {
        return currentState == ConnectionState.Connected;
    }
}