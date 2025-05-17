using UnityEngine;
using System.IO.Ports;
using System.Collections;

public class Test : MonoBehaviour {
    [Header("Serial Settings")]
    public string portName = "COM4";
    public int baudRate = 115200;   // <-- 115200

    private SerialPort port;
    private bool ready = false;

    void Start() {
        // 1) Lista puertos disponibles
        Debug.Log("Puertos disponibles: " + string.Join(", ", SerialPort.GetPortNames()));

        // 2) Configura el puerto
        port = new SerialPort(portName, baudRate) {
            ReadTimeout = 200,
            DtrEnable = true,   // fuerza DTR para levantar el CDC
            RtsEnable = true
        };

        // 3) Ábrelo
        try {
            port.Open();
            Debug.Log($"Puerto {portName} abierto a {baudRate}bps");
            StartCoroutine(EnableReadingAfterDelay(1.0f));
        } catch (System.Exception e) {
            Debug.LogError($"Error abriendo {portName}: {e.Message}");
        }
    }

    IEnumerator EnableReadingAfterDelay(float secs) {
        Debug.Log($"Esperando {secs}s para que Arduino arranque...");
        yield return new WaitForSeconds(secs);
        ready = true;
        Debug.Log("Listo para leer datos");
    }

    void Update() {
        if (!ready || port == null || !port.IsOpen) return;

        try {
            string line = port.ReadLine().Trim();
            Debug.Log($"Dato recibido: [{line}]");
            if (line == "1") Debug.Log("Estoy Vivo");
        } catch (System.TimeoutException) { /* no llegó nada */ } catch (System.Exception e) {
            Debug.LogError($"Error lectura: {e.Message}");
        }
    }

    void OnApplicationQuit() {
        if (port != null && port.IsOpen) port.Close();
    }
}
