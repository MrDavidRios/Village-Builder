using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class UnderConstruction : MonoBehaviour
{
    private Vector3 initialScale;

    public float laborAmount;
    public float laborRequired;

    public int currentStage;
    public int stageAmount;

    public string[] stageDirectories;
    public StageInfo[] stageSettings;
    private GameObject[] stages;

    [HideInInspector] public bool currentlyProgressing;
    public bool resetPosition;

    private Vector3 templatePositionOffset;

    private GameObject constructionSitePrefab;

    public Building buildingInfo;

    private void Awake()
    {
        currentlyProgressing = false;

        constructionSitePrefab = Resources.Load<GameObject>("Prefabs/Buildings/Construction_Site");

        stages = new GameObject[stageDirectories.Length];

        stageAmount = stages.Length - 1;

        //0 stage is construction site stage
        currentStage = 0;

        switch (tag)
        {
            case "Infrastructure":
                buildingInfo = GetComponent<InfrastructureBuilding>().building;
                break;
            case "Residential":
                buildingInfo = GetComponent<ResidentialBuilding>().building;
                break;
            case "Industrial":
                buildingInfo = GetComponent<IndustrialBuilding>().building;
                break;
            default:
                Debug.LogWarning("Building Tag not found: " + tag);
                break;
        }

        for (int i = 0; i < stageDirectories.Length; i++)
        {
            stages[i] = Resources.Load<GameObject>(stageDirectories[i]);
        }

        initialScale = stageSettings[0].scale;

        templatePositionOffset = GetComponent<BuildingTemplate>().positionOffset;
    }

    //Places temporary template of building before the actual construction occurs. This is so that the user can better visualize their city.
    public void PlaceTemplate()
    {
        Material cloneMaterial = Instantiate<Material>(GetComponent<Renderer>().sharedMaterials[0]);

        for (int i = 0; i < GetComponent<Renderer>().materials.Length; i++)
        {
            GetComponent<Renderer>().materials[i] = cloneMaterial;
        }

        GetComponent<Renderer>().material = cloneMaterial;

        GameObject.Destroy(GetComponent<BuildingTemplate>());

        //Add building job
        FindObjectOfType<JobManager>().AssignJobGroup("Build", transform.position, new Transform[1] { transform });
    }

    public void InitializeConstructionSite()
    {
        if (resetPosition)
            transform.position -= templatePositionOffset;

        UpdateBuildingAppearance(0);
    }

    public void ClearGrass(float laborCoefficient, Vector3 endScale)
    {
        float laborNeeded = Mathf.Pow(buildingInfo.width, 2) * 10f;

        laborAmount += laborCoefficient * Time.deltaTime;

        float percentageComplete = laborAmount / laborNeeded;

        transform.localScale = Vector3.Lerp(initialScale, endScale, percentageComplete);

        //Debug.Log("Labor Amount: " + laborAmount + "; Labor Required: " + laborRequired + "; Percentage Complete: " + percentageComplete);

        if (percentageComplete >= 1f)
        {
            laborAmount = 0;
            currentlyProgressing = true;
            return;
        }
    }

    public void WorkOnBuilding(float laborCoefficient)
    {
        laborAmount += laborCoefficient;

        int nextStage = currentStage + 1;

        if (laborAmount > laborRequired)
            laborAmount = laborRequired;

        //If labor amount is greater than or equal to what is required for the next stage, then update the building's appearance.
        if (laborAmount >= (laborRequired / stageAmount) * nextStage)
            UpdateBuildingAppearance(nextStage);
    }

    public void UpdateBuildingAppearance(int stage)
    {
        currentStage = stage;

        GetComponent<MeshFilter>().sharedMesh = stages[stage].GetComponent<MeshFilter>().sharedMesh;
        GetComponent<MeshRenderer>().materials = stages[stage].GetComponent<MeshRenderer>().sharedMaterials;

        transform.position += stageSettings[stage].positionOffset;
        transform.eulerAngles += stageSettings[stage].rotationOffset;

        if (stageSettings[stage].scale != Vector3.zero)
            transform.localScale = stageSettings[stage].scale;

        if (stage > 0)
            GetComponent<BoxCollider>().size = Vector3.one;

        if (stage == stageAmount && stageAmount > 0)
        {
            DestroyScript();
            return;
        }
    }

    public ItemBundle[] ReturnRequiredResources()
    {
        return buildingInfo.requiredResources;
    }

    public void DestroyScript() => GameObject.Destroy(this.GetComponent<UnderConstruction>());
}
