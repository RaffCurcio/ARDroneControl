using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using PassthroughCameraSamples.MultiObjectDetection;
using System.Collections.Concurrent;
using System.Threading;

public class UdpSender : MonoBehaviour
{
    private DroneCommandSender _commandSender;
    private const float SendInterval = 0.05f; // 50 ms
    private float _lastSendTime;
    private Thread _commandThread;
    private bool _isRunning = false;
    private ConcurrentQueue<string> _commandQueue = new();


    [Header("App Control")]
    public DetectionUiMenuManager uiMenuManager;

    [Header("Connection")]
    public string droneIp = "192.168.10.1";
    public int dronePort = 8889;

    [Header("XR Input Actions")]
    public InputActionProperty takeoffAction;
    public InputActionProperty landAction;
    public InputActionProperty moveAction;
    public InputActionProperty secondaryMoveAction;
    public InputActionProperty flipAction;

    private bool _isFlying;
    private bool _hasFlipped;

    // Ultimi valori inviati per evitare invii inutili
    private Vector2 _lastMoveInput = Vector2.zero;
    private Vector2 _lastSecondaryMoveInput = Vector2.zero;

    // Deadzone per evitare rumore analogico
    private const float Deadzone = 0.15f;

    private void Start()
    {
        _commandSender = new DroneCommandSender(droneIp, dronePort);
        _commandSender.Send("command"); // inizializza modalità SDK

        // Avvia thread per invio comandi
        _isRunning = true;
        _commandThread = new Thread(CommandLoop);
        _commandThread.IsBackground = true;
        _commandThread.Start();
    }

    private void OnEnable()
    {
        takeoffAction.action.Enable();
        landAction.action.Enable();
        moveAction.action.Enable();
        secondaryMoveAction.action.Enable();
        flipAction.action.Enable();
    }

    private void OnDisable()
    {
        takeoffAction.action.Disable();
        landAction.action.Disable();
        moveAction.action.Disable();
        secondaryMoveAction.action.Disable();
        flipAction.action.Disable();
    }

    private void OnDestroy()
    {
        _isRunning = false;
        _commandThread?.Join();
        _commandSender?.Close();
    }

    private void Update()
    {
        if (!ChangeColorOnTrigger.IsDroneSelected)
            return;

        if (takeoffAction.action.triggered)
        {
            SendCommand("takeoff");
            _isFlying = true;
        }

        if (landAction.action.triggered)
        {
            SendCommand("land");
            _isFlying = false;
        }

        if (_isFlying)
        {
            HandleFlip();

            if (Time.time - _lastSendTime > SendInterval)
            {
                SendMovementCommand();
                _lastSendTime = Time.time;
            }
        }
    }

    private void HandleFlip()
    {
        if (flipAction.action.ReadValue<float>() > 0)
        {
            Vector2 flipInput = secondaryMoveAction.action.ReadValue<Vector2>();

            if (flipInput.magnitude < 0.01f)
                _hasFlipped = false;

            if (!_hasFlipped)
            {
                if (flipInput.y > 0.8f) SendCommand("flip f");
                else if (flipInput.y < -0.8f) SendCommand("flip b");
                else if (flipInput.x > 0.8f) SendCommand("flip r");
                else if (flipInput.x < -0.8f) SendCommand("flip l");

                _hasFlipped = true;
            }
        }
    }

    private void SendMovementCommand()
    {
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        Vector2 secondaryMoveInput = secondaryMoveAction.action.ReadValue<Vector2>();

        // Applica deadzone
        moveInput = ApplyDeadzone(moveInput);
        secondaryMoveInput = ApplyDeadzone(secondaryMoveInput);

        // Se input non è cambiato rispetto all'ultimo inviato, invia comunque per mantenere sincronizzato
        bool inputChanged = moveInput != _lastMoveInput || secondaryMoveInput != _lastSecondaryMoveInput;

        if (!inputChanged)
        {
            // Ma se non cambia, invia comunque comando zero se entrambi gli stick sono a riposo
            if (moveInput == Vector2.zero && secondaryMoveInput == Vector2.zero)
            {
                SendCommand("rc 0 0 0 0");
            }
            else
            {
                // Ripeti ultimo comando se vuoi (opzionale)
                string lastCommand = $"rc {Mathf.RoundToInt(_lastMoveInput.x * 100)} {Mathf.RoundToInt(_lastMoveInput.y * 100)} {Mathf.RoundToInt(_lastSecondaryMoveInput.y * 100)} {Mathf.RoundToInt(_lastSecondaryMoveInput.x * 100)}";
                SendCommand(lastCommand);
            }
            return;
        }

        // Comando movimento
        string movementCommand =
            $"rc {Mathf.RoundToInt(moveInput.x * 100)} {Mathf.RoundToInt(moveInput.y * 100)} {Mathf.RoundToInt(secondaryMoveInput.y * 100)} {Mathf.RoundToInt(secondaryMoveInput.x * 100)}";

        SendCommand(movementCommand);

        // Aggiorna ultimi input
        _lastMoveInput = moveInput;
        _lastSecondaryMoveInput = secondaryMoveInput;
    }

    private Vector2 ApplyDeadzone(Vector2 input)
    {
        if (input.magnitude < Deadzone)
            return Vector2.zero;
        return input;
    }

    public void SendCommandByPanel(Button button)
    {
        SendCommand(button.gameObject.tag);
    }

    private void SendCommand(string command)
    {
        Debug.Log("Queued command: " + command);
        _commandQueue.Enqueue(command);
    }


    private void CommandLoop()
    {
        while (_isRunning)
        {
            if (_commandQueue.TryDequeue(out var cmd))
            {
                _commandSender.Send(cmd);
            }

            Thread.Sleep((int)(SendInterval * 1000)); // 50 ms
        }
    }

}
