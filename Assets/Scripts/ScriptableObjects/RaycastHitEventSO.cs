using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/RaycastHitEventSO")]
public class RaycastHitEventSO : ScriptableObject
{
    public UnityAction<RaycastHit> OnEventRaised;
    public void RaiseEvent(RaycastHit ray)
    {
        OnEventRaised?.Invoke(ray);
    }
}
