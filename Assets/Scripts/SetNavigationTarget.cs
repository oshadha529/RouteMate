using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class SetNavigationTarget : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown navigationTargetDropDown;
    [SerializeField]
    private List<Target> navigationTargetObjects = new List<Target>();
    [SerializeField]
    private Slider navigationYOffset;
    [SerializeField]
    private GameObject[] targetIndicators; // Array of indicators for navigation targets

    private NavMeshPath path; // current calculated path
    private LineRenderer line; // line renderer to display path
    private Vector3 targetPosition = Vector3.zero; // current target position

    private int currentFloor = 1;
    private bool lineToggle = false;

    private void Start()
    {
        path = new NavMeshPath();
        line = transform.GetComponent<LineRenderer>();
        line.enabled = lineToggle;

        // Initially hide all indicators
        HideAllIndicators();
    }

    private void Update()
    {
        if (lineToggle && targetPosition != Vector3.zero)
        {
            NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
            line.positionCount = path.corners.Length;
            Vector3[] calculatedPathAndOffset = AddLineOffset();
            line.SetPositions(calculatedPathAndOffset);
        }
    }

    public void SetCurrentNavigationTarget(int selectedValue)
    {
        // Prevent execution if the placeholder is selected
        if (selectedValue == 0) return;

        targetPosition = Vector3.zero;
        string selectedText = navigationTargetDropDown.options[selectedValue].text;
        Target currentTarget = navigationTargetObjects.Find(x => x.Name.ToLower().Equals(selectedText.ToLower()));

        if (currentTarget != null)
        {
            // Hide all indicators first
            HideAllIndicators();

            // Show the target's indicator
            ShowTargetIndicator(currentTarget);

            // Set the navigation target position
            targetPosition = currentTarget.PositionObject.transform.position;

            if (!line.enabled)
            {
                ToggleVisibility();
            }
        }
    }

    // Function to hide all indicators
    private void HideAllIndicators()
    {
        foreach (var indicator in targetIndicators)
        {
            indicator.SetActive(false);
        }
    }

    // Function to show the selected target's indicator
    private void ShowTargetIndicator(Target currentTarget)
    {
        foreach (var indicator in targetIndicators)
        {
            if (indicator.name.Equals(currentTarget.Name)) // Assuming the indicator's name matches the target's name
            {
                indicator.SetActive(true);
                break;
            }
        }
    }

    public void ToggleVisibility()
    {
        lineToggle = !lineToggle;
        line.enabled = lineToggle;
    }

    public void ChangeActiveFloor(int floorNumber)
    {
        currentFloor = floorNumber;
        SetNavigationTargetDropDownOptions(currentFloor);
    }

    private Vector3[] AddLineOffset()
    {
        if (navigationYOffset.value == 0)
        {
            return path.corners;
        }

        Vector3[] calculatedLine = new Vector3[path.corners.Length];
        for (int i = 0; i < path.corners.Length; i++)
        {
            calculatedLine[i] = path.corners[i] + new Vector3(0, navigationYOffset.value, 0);
        }
        return calculatedLine;
    }

    private void SetNavigationTargetDropDownOptions(int floorNumber)
    {
        navigationTargetDropDown.ClearOptions();

        List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>();

        // Add placeholder as the first option
        newOptions.Add(new TMP_Dropdown.OptionData("Select a destination"));

        if (floorNumber == 1)
        {
            newOptions.Add(new TMP_Dropdown.OptionData("HeadOfOperations"));
            newOptions.Add(new TMP_Dropdown.OptionData("Library"));
            newOptions.Add(new TMP_Dropdown.OptionData("ManagerOperations"));
            newOptions.Add(new TMP_Dropdown.OptionData("MarketingDepartment"));
            newOptions.Add(new TMP_Dropdown.OptionData("MeetingRoom"));
            newOptions.Add(new TMP_Dropdown.OptionData("Reception"));
            newOptions.Add(new TMP_Dropdown.OptionData("Staircase"));
            newOptions.Add(new TMP_Dropdown.OptionData("StudyGlobalUnit"));
            newOptions.Add(new TMP_Dropdown.OptionData("WashRoom"));
        }

        if (floorNumber == 2)
        {
            newOptions.Add(new TMP_Dropdown.OptionData("AcademicStaffRoom"));
            newOptions.Add(new TMP_Dropdown.OptionData("AdminDepartment"));
            newOptions.Add(new TMP_Dropdown.OptionData("FirstFloorEntrance"));
            newOptions.Add(new TMP_Dropdown.OptionData("FirstFloorWashRoom"));
            newOptions.Add(new TMP_Dropdown.OptionData("FreeArea"));
            newOptions.Add(new TMP_Dropdown.OptionData("ITDepartment"));
            newOptions.Add(new TMP_Dropdown.OptionData("ITLab1"));
            newOptions.Add(new TMP_Dropdown.OptionData("LectureTheater1"));
            newOptions.Add(new TMP_Dropdown.OptionData("LectureTheater2"));
        }

        navigationTargetDropDown.AddOptions(newOptions);
        navigationTargetDropDown.value = 0; // Select placeholder
        navigationTargetDropDown.RefreshShownValue();

        if (line.enabled)
        {
            ToggleVisibility(); // Hide line when floor changes
        }
    }
}
