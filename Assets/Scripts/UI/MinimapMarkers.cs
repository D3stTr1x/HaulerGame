using System.Collections.Generic;
using UnityEngine;

public class MinimapMarkers : MonoBehaviour
{
    public Transform player;
    public RectTransform minimapRect;
    public Camera minimapCamera;
    public GameObject zoneMarkerPrefab;
    public float borderPadding = 5f;

    private Dictionary<Transform, RectTransform> activeMarkers = new Dictionary<Transform, RectTransform>();
    private List<Transform> deliveryZones = new List<Transform>();
    private float minimapHalfWidth;
    private float minimapHalfHeight;
    private float worldHalfWidth; 
    private float worldHalfHeight; 
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        minimapCamera = GameObject.FindGameObjectWithTag("MinimapCamera").GetComponent<Camera>();
        minimapRect = gameObject.GetComponent<RectTransform>();

        minimapHalfWidth = minimapRect.rect.width / 2 - borderPadding;
        minimapHalfHeight = minimapRect.rect.height / 2 - borderPadding;
        worldHalfWidth = 50f;
        worldHalfHeight = 50f;
    }
    void FindActiveZones()
    {
        GameObject[] zones = GameObject.FindGameObjectsWithTag("DeliveryZone");
        foreach (GameObject zone in zones)
        {
            if (zone.activeInHierarchy)
            {
                deliveryZones.Add(zone.transform);
            }
        }
    }
    void UpdateMarkers()
    {
        if (player == null || minimapCamera == null) return;

        FindActiveZones(); //???
        float playerAngle = player.eulerAngles.y * Mathf.Deg2Rad;

        if (deliveryZones.Count > 0)
        {
            foreach (Transform zone in deliveryZones)
            {
                Vector3 worldOffset = zone.position - player.position;

                float rotatedX = worldOffset.x * Mathf.Cos(playerAngle) - worldOffset.z * Mathf.Sin(playerAngle);
                float rotatedZ = worldOffset.x * Mathf.Sin(playerAngle) + worldOffset.z * Mathf.Cos(playerAngle);

                Vector2 minimapPos = new Vector2(rotatedX/worldHalfWidth*minimapHalfWidth, rotatedZ/worldHalfHeight*minimapHalfHeight); //!!

                bool isInsideMinimap = (Mathf.Abs(minimapPos.x) < minimapHalfWidth) && (Mathf.Abs(minimapPos.y) < minimapHalfHeight);
                bool isVisible = IsZoneVisible(zone);

                RectTransform markerRect;
                if (!activeMarkers.ContainsKey(zone))
                {
                    GameObject marker = Instantiate(zoneMarkerPrefab, minimapRect);
                    markerRect = marker.GetComponent<RectTransform>();
                    activeMarkers[zone] = markerRect;
                }
                else
                {
                    markerRect = activeMarkers[zone];
                }


                if (isInsideMinimap && isVisible)
                {
                    markerRect.anchoredPosition = minimapPos;
                    markerRect.rotation = Quaternion.identity;
                    markerRect.gameObject.SetActive(true);
                }
                else
                {
                    Vector2 direction = minimapPos.normalized;
                    Vector2 borderPos = ClampToBorder(minimapPos);
                    markerRect.anchoredPosition = borderPos;

                    float angle = Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg;
                    markerRect.rotation = Quaternion.Euler(0, 0, angle - 90);

                    markerRect.gameObject.SetActive(true);
                }
            }

        }
    }
    bool IsZoneVisible(Transform zone)
    {
        if (minimapCamera == null) return false;

        Vector3 viewportPoint = minimapCamera.WorldToViewportPoint(zone.position);
        float margin = 0.1f;
        return viewportPoint.x > margin && viewportPoint.x < 1 - margin && viewportPoint.y > margin && viewportPoint.y < 1 - margin && viewportPoint.z > 0;
    }
    public void DestroyMarkers()
    {
        foreach (var kvp in activeMarkers)
        {
            Destroy(kvp.Value.gameObject);   
        }
        activeMarkers.Clear();
        deliveryZones.Clear();
    }
    // Update is called once per frame
    void Update()
    {
        UpdateMarkers();
    }
    Vector2 ClampToBorder(Vector2 pos)
    {
        float absX = Mathf.Abs(pos.x);
        float absY = Mathf.Abs(pos.y);
        if (absX / minimapHalfWidth > absY / minimapHalfHeight)
        {
            float signX = Mathf.Sign(pos.x);
            float clampedY = pos.y * (minimapHalfWidth / absX);
            return new Vector2(signX * minimapHalfWidth, clampedY);
        }
        else
        {
            float signY = Mathf.Sign(pos.y);
            float clampedX = pos.x * (minimapHalfHeight / absY);
            return new Vector2(clampedX, signY * minimapHalfHeight);
        }
    }
}
