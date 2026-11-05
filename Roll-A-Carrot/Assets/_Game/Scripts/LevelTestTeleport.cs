using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController), typeof(Rigidbody))]
public class LevelTestTeleport : MonoBehaviour
{
#if UNITY_EDITOR
    private SkyboxZone[] zones;
    private int currentIndex = -1;
    private string status = "N: next area";

    private void Start()
    {
        zones = FindObjectsByType<SkyboxZone>(FindObjectsSortMode.None);
        // This level's platforms progress along the world's X axis.
        Array.Sort(zones, (a, b) =>
            a.GetComponent<BoxCollider>().bounds.center.x.CompareTo(
                b.GetComponent<BoxCollider>().bounds.center.x));

        // Start cycling from the area where the player currently stands.
        for (int i = 0; i < zones.Length; i++)
        {
            if (zones[i].GetComponent<BoxCollider>().bounds.Contains(transform.position))
            {
                currentIndex = i;
                break;
            }
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
            NextArea();
    }

    public string Status => status;
    public bool LastTeleportSucceeded { get; private set; }

    public void NextArea()
    {
        LastTeleportSucceeded = false;
        Rigidbody body = GetComponent<Rigidbody>();
        if (body.isKinematic)
        {
            status = "Restart Play after Win / Game Over.";
            return;
        }

        if (zones == null || zones.Length == 0)
        {
            status = "No active skybox zones found.";
            return;
        }

        int nextIndex = (currentIndex + 1) % zones.Length;
        BoxCollider area = zones[nextIndex].GetComponent<BoxCollider>();
        Bounds bounds = area.bounds;
        Vector3 origin = bounds.center;
        origin.y = bounds.max.y + 100f;

        // Find solid ground, ignoring trigger volumes, the player and moving props.
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down,
            bounds.size.y + 200f, ~0, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.attachedRigidbody != null ||
                hit.collider.transform.IsChildOf(transform) || hit.normal.y < 0.6f)
                continue;

            float feetOffset = 0.5f;
            Collider playerCollider = GetComponent<Collider>();
            if (playerCollider != null)
                feetOffset = transform.position.y - playerCollider.bounds.min.y;

            Vector3 destination = hit.point + Vector3.up * (feetOffset + 0.2f);
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = destination;
            transform.position = destination;
            zones[nextIndex].ApplySettings();
            currentIndex = nextIndex;
            LastTeleportSucceeded = true;
            status = zones[nextIndex].name;
            return;
        }

        status = "No safe floor below " + zones[nextIndex].name;
    }
#endif
}