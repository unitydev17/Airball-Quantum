using Photon.Deterministic;
using Quantum;
using Quantum.Demo;
using UnityEngine;

public class LocalInput : MonoBehaviour
{
    private bool _isDebugRun;
    private Vector3 _mousePos;
    private bool _tap;
    private bool _isMaster;

    private void OnEnable()
    {
        Init();
        QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    private void Init()
    {
        _isDebugRun = UIMain.Client == null;

        if (_isDebugRun == false && UIMain.Client != null) _isMaster = UIMain.Client.LocalPlayer.IsMasterClient;
        else _isMaster = true;
    }

    private void PollInput(CallbackPollInput callback)
    {
        var input = new Quantum.Input();


        _tap = UnityEngine.Input.GetMouseButton(0);


        var mousePos = UnityEngine.Input.mousePosition;
        var deltaMouse = mousePos - _mousePos;

        _mousePos = mousePos;


        if (!_isMaster) deltaMouse = -deltaMouse; // inverse move for second player

        if (deltaMouse.magnitude < 1)
        {
            deltaMouse = Vector3.zero;
        }

        var moveVector = new Vector2(deltaMouse.x, deltaMouse.y * 1.25f);
        moveVector = Vector3.ClampMagnitude(moveVector, 200);
        input.MouseDirection = _tap ? moveVector.ToFPVector2() : default;

        callback.SetInput(input, DeterministicInputFlags.Repeatable);
    }
}