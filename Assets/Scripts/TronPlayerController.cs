using UnityEngine;
using System.IO.Ports;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class TronPlayerController : MonoBehaviour
{
    [Header("Serial Settings")]
    public string portName = "COM3";
    public int baudRate = 115200;
    private SerialPort port;
    private bool ready = false;

    [Header("Player Settings")]
    public Transform player;
    public float baseSpeed = 3f;
    public float boostSpeed = 6f;
    public float rotationSpeed = 100f;
    private float currentSpeed;

    [Header("Boost Settings")]
    public float boostDuration = 2f;
    public float cooldownDuration = 5f;
    private bool boosting = false;
    private bool cooldown = false;

    [Header("Trail")]
    public TrailRenderer trail;
    public float trailBackOffset = 0.5f;       // Distancia atrás local
    public float trailHorizontalOffset = 0f;   // Ajuste horizontal local 
    public float trailVerticalOffset = 0f;     // Ajuste vertical local
    private bool trailActivated = false;
    private EdgeCollider2D trailCollider;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;

    // Flags de botones
    private bool rotateLeft = false;
    private bool rotateRight = false;
    private bool boost = false;

    void Start()
    {
        port = new SerialPort(portName, baudRate)
        {
            ReadTimeout = 25,
            DtrEnable = true,
            RtsEnable = true
        };

        try
        {
            port.Open();
            Debug.Log("Puerto abierto");
            StartCoroutine(EnableAfterDelay(1f));
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error abriendo puerto: " + e.Message);
        }

        currentSpeed = baseSpeed;

        if (trail != null)
        {
            trail.emitting = false;
            trail.gameObject.tag = "Trail";
            trailCollider = trail.gameObject.AddComponent<EdgeCollider2D>();
            trailCollider.isTrigger = true;
            trailCollider.enabled = false;
        }

        gameObject.tag = "Player";
    }

    IEnumerator EnableAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        ready = true;
        Debug.Log("Listo para recibir datos de Arduino");
    }

    void Update()
    {
        if (!ready || player == null) return;

        rotateLeft = rotateRight = boost = false;

        if (port != null && port.IsOpen)
        {
            try
            {
                while (port.BytesToRead > 0)
                {
                    string command = port.ReadLine().Trim();
                    switch (command)
                    {
                        case "L": rotateLeft = true; break;
                        case "R": rotateRight = true; break;
                        case "U": boost = true; break;
                        case "D":
                            if (!trailActivated)
                            {
                                trailActivated = true;
                                if (trail != null) trail.emitting = true;
                            }
                            break;
                    }
                }
            }
            catch { }
        }

        player.Translate(Vector3.up * currentSpeed * Time.deltaTime, Space.Self);

        if (rotateLeft) player.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        if (rotateRight) player.Rotate(0, 0, -rotationSpeed * Time.deltaTime);

        if (boost && !boosting && !cooldown)
        {
            StartCoroutine(DoBoost());
        }

        if (trail != null)
        {
            // Posicionar trail usando espacio local para que siempre quede atrás del jugador sin interferir al girar
            Vector3 localOffset = new Vector3(trailHorizontalOffset, -trailBackOffset, trailVerticalOffset);
            Vector3 trailPos = player.TransformPoint(localOffset);

            trail.transform.position = trailPos;
            trail.transform.rotation = player.rotation;

            trailCollider.enabled = trail.emitting;
            if (trail.emitting) UpdateTrailCollider();
        }
    }

    IEnumerator DoBoost()
    {
        boosting = true;
        currentSpeed = boostSpeed;
        yield return new WaitForSeconds(boostDuration);
        currentSpeed = baseSpeed;
        boosting = false;
        cooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        cooldown = false;
    }

    void UpdateTrailCollider()
    {
        if (trail == null || trail.positionCount < 2) return;

        Vector3[] positions = new Vector3[trail.positionCount];
        trail.GetPositions(positions);

        Vector2[] points2D = new Vector2[positions.Length];
        for (int i = 0; i < positions.Length; i++)
            points2D[i] = trail.transform.InverseTransformPoint(positions[i]);

        trailCollider.points = points2D;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trail"))
        {
            TeleportToSpawn();
        }
    }

    void TeleportToSpawn()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No hay puntos de spawn asignados.");
            return;
        }

        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        player.position = spawn.position;
        player.rotation = spawn.rotation;
        trail.Clear();
        trail.emitting = false;
        trailActivated = false;
        boosting = false;
        cooldown = false;
        currentSpeed = baseSpeed;
    }

    void OnApplicationQuit()
    {
        if (port != null && port.IsOpen) port.Close();
    }
}
