using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Android;

public class LocationChecker : MonoBehaviour
{
    public GameObject welcomePanel;
    public GameObject errorPanel;
    public GameObject scannerPanel;
    public GameObject arUIGroup;
    public TextMeshProUGUI messageText;
    public Button retryButton;

    public TextMeshProUGUI latitudeText;
    public TextMeshProUGUI longitudeText;

    public GameObject LineOptionsButton;
    public GameObject DebugOptionsButton;
    public GameObject FloorOptionsButton;
    public GameObject LineOptionsPanel;
    public GameObject MiniMapRawImage;

    private double targetLatitude = 7.28108739005917;
    private double targetLongitude = 80.62005485982979;
    private float allowedDistance = 100f;

    private bool isChecking = false;
    private bool hasLocationAccess = false;
    private string currentLocationId = null;

    private bool permissionRequested = false;

    void Start()
    {
        retryButton.onClick.AddListener(RetryLocation);
        HideAllPanels();

        StartCoroutine(CheckPermissionAndLocation());
    }

    IEnumerator CheckPermissionAndLocation()
    {
#if UNITY_ANDROID
        // Request location permission if not granted
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) && !permissionRequested)
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            permissionRequested = true;
        }

        // Wait until user grants permission or app loses focus (user can deny)
        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            yield return null;
        }
#endif
        // Once permission is granted, start location checking loop
        while (!hasLocationAccess)
        {
            yield return StartCoroutine(CheckLocationOnce());
            if (!hasLocationAccess)
            {
                // Wait a bit before retrying automatically
                yield return new WaitForSeconds(5f);
            }
        }
    }

    IEnumerator CheckLocationOnce()
    {
        if (isChecking)
            yield break;

        isChecking = true;

        if (!Input.location.isEnabledByUser)
        {
            ShowError("Location services are disabled on this device.");
            isChecking = false;
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            ShowError("Failed to determine device location.");
            Input.location.Stop();
            isChecking = false;
            yield break;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            ShowError("Unable to get location. Please ensure location is turned on.");
            Input.location.Stop();
            isChecking = false;
            yield break;
        }

        double userLat = Input.location.lastData.latitude;
        double userLon = Input.location.lastData.longitude;

        float distance = GetDistance(userLat, userLon, targetLatitude, targetLongitude);

        Debug.Log($"User Latitude: {userLat}");
        Debug.Log($"User Longitude: {userLon}");
        Debug.Log($"Distance to Target: {distance} meters");

        if (distance <= allowedDistance)
        {
            EnableScanner();
        }
        else
        {
            ShowError("Sorry, Route Mate is not available at your current location.");
        }

        Input.location.Stop();
        isChecking = false;
        yield return null;
    }

    void EnableScanner()
    {
        if (hasLocationAccess) return; // Prevent multiple calls

        Debug.Log("Inside allowed location. Showing scanner panel.");
        hasLocationAccess = true;

        welcomePanel.SetActive(true);
        errorPanel.SetActive(false);
        scannerPanel.SetActive(true);
        arUIGroup.SetActive(false);

        messageText.text = "Welcome to APIIT Kandy Campus.\n\nPlease scan the QR code to begin.";
    }

    float GetDistance(double lat1, double lon1, double lat2, double lon2)
    {
        float R = 6371000f;
        float dLat = Mathf.Deg2Rad * (float)(lat2 - lat1);
        float dLon = Mathf.Deg2Rad * (float)(lon2 - lon1);

        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(Mathf.Deg2Rad * (float)lat1) * Mathf.Cos(Mathf.Deg2Rad * (float)lat2) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return R * c;
    }

    void ShowError(string msg)
    {
        Debug.Log("ShowError called: " + msg);

        welcomePanel.SetActive(false);
        scannerPanel.SetActive(false);
        errorPanel.SetActive(true);
        arUIGroup.SetActive(false);

        messageText.text = msg;

        //if (Input.location.status == LocationServiceStatus.Running)
        //{
        //    latitudeText.text = $"Latitude: {Input.location.lastData.latitude}";
        //    longitudeText.text = $"Longitude: {Input.location.lastData.longitude}";
        //}
        //else
        //{
        //    latitudeText.text = "Latitude: Unknown";
        //    longitudeText.text = "Longitude: Unknown";
        //}
    }

    public void RetryLocation()
    {
        Debug.Log("RetryLocation called.");
        isChecking = false;
        hasLocationAccess = false;
        errorPanel.SetActive(false);
        StartCoroutine(CheckLocationOnce());
    }

    public void OnScanComplete(string scannedId)
    {
        if (!hasLocationAccess)
        {
            Debug.LogWarning("Scan ignored. User is not inside a valid location zone.");
            return;
        }

        if (string.IsNullOrEmpty(scannedId))
        {
            Debug.LogWarning("Invalid scan detected. Ignoring.");
            return;
        }

        if (scannedId == currentLocationId)
        {
            Debug.Log("Same QR scanned again. Ignoring.");
            return;
        }

        currentLocationId = scannedId;
        Debug.Log("New QR scanned: " + scannedId);

        welcomePanel.SetActive(false);
        errorPanel.SetActive(false);
        arUIGroup.SetActive(true);

        if (LineOptionsButton) LineOptionsButton.SetActive(true);
        if (DebugOptionsButton) DebugOptionsButton.SetActive(true);
        if (FloorOptionsButton) FloorOptionsButton.SetActive(true);
        if (LineOptionsPanel) LineOptionsPanel.SetActive(true);
        if (MiniMapRawImage) MiniMapRawImage.SetActive(true);

        // Additional AR logic based on scannedId
    }

    void HideAllPanels()
    {
        welcomePanel.SetActive(false);
        errorPanel.SetActive(false);
        scannerPanel.SetActive(false);
        arUIGroup.SetActive(false);
    }
}
