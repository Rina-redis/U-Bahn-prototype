using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GpsMvgManager gpsManager;

    private bool? isMoving = null;
    private bool eventTriggered = false;
    private string fromStationId = "";
    private string toStationId = "";
    private DbManager db_service;

    private void OnEnable()
    {
        if (gpsManager != null)
        {
            gpsManager.OnHasStartedChanged += OnHasStartedChanged;
            gpsManager.OnHasStoppedChanged += OnHasStoppedChanged;
        }
    }

    private void OnDisable()
    {
        if (gpsManager != null)
        {
            gpsManager.OnHasStartedChanged -= OnHasStartedChanged;
            gpsManager.OnHasStoppedChanged -= OnHasStoppedChanged;
        }
    }

    private void Start()
    {
        db_service = new DbManager("Pawel");
        if (gpsManager == null)
        {
            Debug.LogError("gpsManager is NULL!");
        }
    }

    private async void OnHasStartedChanged(bool hasStarted, string from, string to)
    {
        isMoving = hasStarted;
        eventTriggered = true;
        fromStationId = from;
        toStationId = to;

        if (db_service != null)
        {
            try
            {
                await db_service.enter_the_train(from, to);
                
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in enter_the_train: {ex.Message}");
            }
        }

        Debug.Log($"OnHasStartedChanged: hasStarted={hasStarted}, from={from}, to={to}");
    }

    private async void OnHasStoppedChanged(bool hasStopped, string from, string to)
    {
        if (hasStopped)
        {
            isMoving = false;
            eventTriggered = true;

            if (db_service != null)
            {
                try
                {
                    await db_service.leave_the_train(from, to);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error in leave_the_train: {ex.Message}");
                }
            }

            Debug.Log("OnHasStoppedChanged: hasStopped=true");
        }
    }

    private void Update()
    {
        if (eventTriggered && isMoving.HasValue)
        {
            if (isMoving.Value)
            {
                Debug.Log($"GAME STARTED from {fromStationId} to {toStationId}");
            }
            else
            {
                Debug.Log($"GAME STOPPED from {fromStationId} to {toStationId}");
            }

            eventTriggered = false;
        }
    }
}
