using UnityEngine;

public class Zone : MonoBehaviour
{
    public int zoneId;

    void OnCaptureStarted(int playerId)
    {
        NarrationManager.Instance.TryNarrate(new NarrationEvent
        {
            type = NarrationEventType.ZoneCaptureStarted,
            playerId = playerId,
            zoneId = zoneId
        });
    }
}