using System;
using System.IO.Ports;
using UnityEngine;

public class ArduinoController : MonoBehaviour {
    public static ArduinoController instance;
    public static event Action OnButtonPressed;

    SerialPort serialPort;
    string portName = "COM4"; // Asegúrate que coincide con el puerto de tu Arduino R4 Minima
    int baudRate = 9600;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        try {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 500;
            serialPort.Open();
            Debug.Log("Conectado al puerto " + portName);
        } catch (Exception e) {
            Debug.LogError("Error al abrir el puerto: " + e.Message);
        }
    }

    void Update() {
        // Enviar comando para alternar LED con tecla Space
        if (Input.GetKeyDown(KeyCode.Space)) {
            SendCommand("toggle_led");
        }


        // Leer datos enviados desde Arduino
        if (serialPort != null && serialPort.IsOpen) {
            try {
                string data = serialPort.ReadLine();
                Debug.Log("Mensaje desde Arduino: " + data);

                if (data == "BOTON") {
                    OnButtonPressed?.Invoke(); // Si quieres que algo ocurra cuando se presione físicamente
                }
            } catch (TimeoutException) {
                // No hacer nada si no hay datos
            } catch (Exception e) {
                Debug.LogError("Error leyendo datos: " + e.Message);
            }
        }
    }

    void SendCommand(string command) {
        if (serialPort != null && serialPort.IsOpen) {
            serialPort.WriteLine(command);
            Debug.Log("Comando enviado a Arduino: " + command);
        }
    }

    void OnApplicationQuit() {
        if (serialPort != null && serialPort.IsOpen) {
            serialPort.Close();
        }
    }
}
