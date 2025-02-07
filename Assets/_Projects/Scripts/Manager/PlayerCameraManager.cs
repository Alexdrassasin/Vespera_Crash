using PurrNet;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraManager : NetworkBehaviour
{
    public List<PlayerCamera> _allPlayerCameras = new();

    private bool _canSwitchCamera;

    private int _currentCameraIndex;
    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<PlayerCameraManager>();
    }

    public void RegisterCamera(PlayerCamera cam)
    {
        if (_allPlayerCameras.Contains(cam))
        {
            return;
        }

        _allPlayerCameras.Add(cam);
        if (cam.isOwner)
        {
            _canSwitchCamera = false;
            cam.ToggleCamera(true);
        }
    }

    public void UnregisterCamera(PlayerCamera cam)
    {
        if (_allPlayerCameras.Contains(cam))
        {
            _allPlayerCameras.Remove(cam);
        }

        if (cam.isOwner)
        {
            _canSwitchCamera = true;
            InstanceHandler.GetInstance<MainGameView>().toggleSpectatingPlayerName(true);
            switchNext();
        }
    }

    private void Update()
    {
        if (!_canSwitchCamera)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            switchNext();
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            switchPrevious();
        }

    }
    private void switchNext()
    {
        if(_allPlayerCameras.Count <= 0)
        {
            return;
        }

        _allPlayerCameras[_currentCameraIndex].parent.GetComponent<PlayerHealth>().isSpectatingThisPlayer = false;
        _allPlayerCameras[_currentCameraIndex].ToggleCamera(false);

        _currentCameraIndex++;

        if(_currentCameraIndex >= _allPlayerCameras.Count)
        {
            _currentCameraIndex = 0;
        }

        _allPlayerCameras[_currentCameraIndex].parent.GetComponent<PlayerHealth>().isSpectatingThisPlayer = true;
        _allPlayerCameras[_currentCameraIndex].parent.GetComponent<PlayerHealth>().UpdateSpecatorUI();
        _allPlayerCameras[_currentCameraIndex].ToggleCamera(true);
        InstanceHandler.GetInstance<MainGameView>().UpdateSpectatingPlayerName(_allPlayerCameras[_currentCameraIndex].owner.Value.ToString());
    }

    private void switchPrevious()
    {
        if (_allPlayerCameras.Count <= 0)
        {
            return;
        }

        _allPlayerCameras[_currentCameraIndex].parent.GetComponent<PlayerHealth>().isSpectatingThisPlayer = false;
        _allPlayerCameras[_currentCameraIndex].ToggleCamera(false);

        _currentCameraIndex--;

        if (_currentCameraIndex < 0)
        {
            _currentCameraIndex = _allPlayerCameras.Count - 1;
        }

        _allPlayerCameras[_currentCameraIndex].parent.GetComponent<PlayerHealth>().isSpectatingThisPlayer = true;
        _allPlayerCameras[_currentCameraIndex].parent.GetComponent<PlayerHealth>().UpdateSpecatorUI();
        _allPlayerCameras[_currentCameraIndex].ToggleCamera(true);
        InstanceHandler.GetInstance<MainGameView>().UpdateSpectatingPlayerName(_allPlayerCameras[_currentCameraIndex].owner.Value.ToString());
    }
}
