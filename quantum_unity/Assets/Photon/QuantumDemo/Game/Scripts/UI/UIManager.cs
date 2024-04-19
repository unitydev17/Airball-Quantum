using System.Collections;
using Quantum;
using Quantum.AirBall.Commands;
using Quantum.Demo;
using UnityEngine;
using UnityEngine.EventSystems;
using Input = Quantum.Input;

namespace Photon.QuantumDemo.Game.Scripts.UI
{
    public class UIManager : QuantumCallbacks
    {
        public Transform[] _textTransforms;
        public Camera _camera;
        public Goal _goalUI;
        public GameObject _waitingUI;
        public GameObject _disconnectUI;

        private TrailRenderer _trail;
        private bool _isMaster;
        private bool _isDebugRun; 

        protected override void OnEnable()
        {
            _isDebugRun = UIMain.Client == null;
            
            Application.targetFrameRate = 60;
            UnityEngine.Input.multiTouchEnabled = false;
            
            base.OnEnable();
            QuantumEvent.Subscribe(this, (EventGoal evt) => OnGoal(evt));
            QuantumEvent.Subscribe(this, (EventPlayersReady evt) => OnPlayersReady());
            QuantumEvent.Subscribe(this, (EventRestarted evt) => OnRestarted());
            QuantumEvent.Subscribe(this, (EventDisconnect evt) => OnDisconnect(evt));
            
            if (!_isDebugRun) CheckMasterPlayerAdjustments();
        }

        private void OnDisconnect(EventDisconnect evt)
        {
            if (UIMain.Client.LocalPlayer.ActorNumber == evt.actorId)
            {
                UIMain.Client.Disconnect();
                return;
            }

            _disconnectUI.SetActive(true);
        }

        private void CheckMasterPlayerAdjustments()
        {
            _isMaster = UIMain.Client.LocalPlayer.IsMasterClient;
            
            if (!_isMaster)
            {
                var correctedRotationForNonMasterPlayer = Quaternion.Euler(90, 0, 180);

                _camera.transform.localRotation = correctedRotationForNonMasterPlayer;
                foreach (var tr in _textTransforms)
                {
                    tr.localRotation = correctedRotationForNonMasterPlayer;
                }
            }

            var cmd = new SpawnCommand
            {
                isMaster = _isMaster,
                actorNum = UIMain.Client.LocalPlayer.ActorNumber
            };
            QuantumRunner.Default.Game.SendCommand(cmd);
        }

        private void OnRestarted()
        {
            StartCoroutine(EnableTrails());
        }

        private void OnPlayersReady()
        {
            _waitingUI.gameObject.SetActive(false);
        }

        private void OnGoal(EventGoal evt)
        {
            DisableTrails();

            var goalStruct = evt.goalStruct;

            if (_isMaster)
            {
                _goalUI.SetScore(goalStruct.value_1, goalStruct.value_2);
            }
            else
            {
                _goalUI.SetScore(goalStruct.value_2, goalStruct.value_1);
            }

            _goalUI.gameObject.SetActive(true);
        }

        public void OnContinue()
        {
            _goalUI.gameObject.SetActive(false);
            _waitingUI.gameObject.SetActive(true);

            var command = new ContinueCommand();
            QuantumRunner.Default.Game.SendCommand(command);
        }

        private void DisableTrails()
        {
            _trail = FindObjectOfType<TrailRenderer>();
            _trail.emitting = false;
            _trail.Clear();
        }

        private IEnumerator EnableTrails()
        {
            yield return new WaitForSeconds(1);
            _trail.emitting = true;
        }
    }
}