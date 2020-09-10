using System.Collections;
using System.Collections.Generic;
using DavidRios.Building;
using TileOperations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DavidRios.UI
{
    public class DisplayJobsList : MonoBehaviour
    {
        public List<string> jobsToIgnore;
        public List<string> jobsThatAreNotModifiable;
        [SerializeField] private List<Job> listedJobs = new List<Job>();

        private Villager _selectedVillager;

        private UIManager _uiManagerScript;

        private void Awake()
        {
            _uiManagerScript = FindObjectOfType<UIManager>();
        }

        //On call, if just opened, redo everything. If not, then add/remove the last added/removed job.
        public IEnumerator DisplayVillagerJobs(string source, Job lastJob = null, Villager sourceVillager = null)
        {
            //Initialize necessary variables
            var selectedObject = Select.SelectedObject;

            var newlyOpened = !_uiManagerScript.MiscUIElements["JobsList"].activeInHierarchy;

            if (source == "Jobs Button")
                newlyOpened = true;

            yield return new WaitUntil(() => _uiManagerScript.MiscUIElements["JobsList"].activeInHierarchy);

            if (selectedObject == null)
                yield break;

            if (selectedObject.layer != LayerMask.NameToLayer("Villager"))
                yield break;

            if (source == "Villager.cs" && lastJob == null)
            {
                Debug.LogError("'lastJobType' cannot be null if Villager.cs is the source!");
                yield break;
            }

            //Initialize necessary variables
            var villager = selectedObject.GetComponent<Villager>();

            //Make it so that the function exits if the villager is not the same as the selected villager.
            if (sourceVillager != null)
                if (sourceVillager != villager)
                    yield break;

            var jobListDisplay = _uiManagerScript.MiscUIElements["JobsList"];

            _selectedVillager = villager;

            //If the job is still in the villager's job list, then it hasn't been removed. 
            var jobAdded = villager.jobList.Contains(lastJob);

            //If the current listed jobs is equal to the villager's job list, then there is no need to change anything.
            if (listedJobs == villager.jobList && villager.jobList.Count > 0)
                yield break;

            if (newlyOpened)
            {
                var jobsToList = villager.jobList;

                //Clear everything and start anew
                if (listedJobs.Count > 0)
                {
                    //If the only listed job is idle and the villager is idle, then leave the idle job there.
                    if (listedJobs.Count == 1 && listedJobs[0].jobType == "Idle")
                        if (villager.jobList.Count == 0)
                            yield break;

                    for (var i = 0; i < listedJobs.Count; i++) Destroy(jobListDisplay.transform.GetChild(i).gameObject);

                    listedJobs.Clear();
                }

                if (villager.jobList.Count == 0)
                    AddJobElement(jobListDisplay.transform, 1, new Job {jobType = "Idle"}, false);

                var lastAddedJobIndex = 0;
                for (var i = 0; i < villager.jobList.Count; i++)
                    if (!jobsToIgnore.Contains(villager.jobList[i].jobType))
                    {
                        AddJobElement(jobListDisplay.transform, lastAddedJobIndex + 1, villager.jobList[i], true);

                        lastAddedJobIndex++;
                    }
            }
            else
            {
                //Add/remove last added/removed job.
                if (jobAdded)
                {
                    if (listedJobs.Count > 0)
                        if (listedJobs[0].jobType == "Idle")
                        {
                            if (villager.jobList.Count == 0)
                                yield break;
                            RemoveJobElement(listedJobs[0]);
                        }

                    var jobIndex = listedJobs.Count + 1;

                    //Add new job.
                    if (listedJobs.Count == 0)
                        AddJobElement(jobListDisplay.transform, jobIndex, lastJob, true);
                    else
                        AddJobElement(jobListDisplay.transform, jobIndex, lastJob, false);
                }
                else
                {
                    //Remove last job. (Last listed job)
                    if (!jobsToIgnore.Contains(lastJob.jobType))
                    {
                        RemoveJobElement(lastJob);

                        //Reposition Jobs Here.
                        for (var i = 0; i < listedJobs.Count; i++)
                            jobListDisplay.transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition =
                                new Vector3(0f, -20f - 37.5f * (listedJobs.Count - i - 1), 0f);
                    }
                }
            }

            yield return new WaitForEndOfFrame();

            if (listedJobs.Count > 0 && villager.jobList.Count > 0)
            {
                if (listedJobs[0] == villager.jobList[0])
                    SetModifiabilityOfJobElement(jobListDisplay.transform, 1, false);
                else
                    SetModifiabilityOfJobElement(jobListDisplay.transform, 1, true);
            }

            //Debug.Log(listedJobs[0].jobType + ", " + villager.jobList[0].jobType);
        }

        private void AddJobElement(Transform jobListDisplay, int elementIndex, Job job, bool modifiable)
        {
            //Debug.Log("Job Added: " + job.jobType);

            var displayText = job.jobType == "Idle" ? "Idle" : JobUtils.ReturnJobName(job.jobType);

            listedJobs.Add(job);

            var jobElement = Instantiate(UIManager.JobPrefab, jobListDisplay.transform);
            jobElement.transform.SetAsFirstSibling();

            jobElement.GetComponent<RectTransform>().anchoredPosition =
                new Vector3(0f, -20f - 37.5f * (elementIndex - 1), 0f);
            jobElement.GetComponentInChildren<TMP_Text>().text = displayText;

            jobElement.transform.GetChild(0).Find("CancelBackground").GetChild(0).GetComponent<Button>().onClick
                .AddListener(() => RemoveJobGroup(job));
            //jobElement.transform.GetChild(0).Find("CancelBackground").GetChild(0).GetComponent<Button>().onClick.AddListener(() => RemoveJobElement(job));

            //Debug.Log("Modifiable: " + modifiable);
            if (!jobsThatAreNotModifiable.Contains(job.jobType))
                SetModifiabilityOfJobElement(jobElement, modifiable);
            else
                SetModifiabilityOfJobElement(jobElement, false);
        }

        /// <summary>
        ///     Indexes of job list elements are reversed. If the job's index is 2 and the job list has 5 elements, you would need
        ///     to grab the 3rd element (0, 1, 2, 3, 4).
        ///     The index of jobs would start at 1, not 0!
        ///     First element would have an index of jobListDisplay.transform.childCount - 1.
        /// </summary>
        /// <param name="jobListDisplay"></param>
        /// <param name="jobIndex"></param>
        public void RemoveJobElement(Job job)
        {
            var jobListDisplay = _uiManagerScript.MiscUIElements["JobsList"].transform;

            //if the job index is unavailable, just delete the first job.

            Debug.Log("Job removed; job index & job type: " +
                      (jobListDisplay.transform.childCount - listedJobs.IndexOf(job) - 1) + ", " + job.jobType);

            Destroy(jobListDisplay.GetChild(jobListDisplay.transform.childCount - listedJobs.IndexOf(job) - 1).gameObject);

            listedJobs.Remove(job);

            if (listedJobs.Count == 0 && _selectedVillager.jobList.Count == 0)
                AddJobElement(jobListDisplay.transform, 1, new Job {jobType = "Idle"}, false);
        }

        /// <summary>
        ///     jobIndex has to be starting at 1. Ex: if the first element's modifiable property has to be set to false, then
        ///     jobIndex must equal 1.
        /// </summary>
        /// <param name="jobListDisplay"></param>
        /// <param name="jobIndex"></param>
        /// <param name="modifiable"></param>
        private void SetModifiabilityOfJobElement(Transform jobListDisplay, int jobIndex, bool modifiable)
        {
            //Debug.Log("Modifiability Set: " + jobListDisplay.GetChild(jobListDisplay.childCount - jobIndex).GetChild(0).GetComponentInChildren<TMP_Text>().text + ", " + modifiable);

            var jobElementAnimator = jobListDisplay.GetChild(jobListDisplay.childCount - jobIndex).GetChild(0)
                .GetComponent<Animator>();

            if (!modifiable && jobElementAnimator.GetBool("OpenSettings"))
                jobElementAnimator.SetBool("OpenSettings", false);

            jobElementAnimator.SetBool("Modifiable", modifiable);
        }

        private void SetModifiabilityOfJobElement(GameObject jobElement, bool modifiable)
        {
            //Debug.Log("Modifiability Set: " + jobElement.GetComponentInChildren<TMP_Text>().text + ", " + modifiable);

            var jobElementAnimator = jobElement.transform.GetChild(0).GetComponent<Animator>();

            if (!modifiable && jobElementAnimator.GetBool("OpenSettings"))
                jobElementAnimator.SetBool("OpenSettings", false);

            jobElementAnimator.SetBool("Modifiable", modifiable);
        }

        private IEnumerator UpdateJobElementPositions()
        {
            var jobListDisplay = _uiManagerScript.MiscUIElements["JobsList"];

            yield return new WaitForEndOfFrame();

            for (var i = 0; i < listedJobs.Count; i++)
                jobListDisplay.transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition =
                    new Vector3(0f, -20f - 37.5f * (listedJobs.Count - i - 1), 0f);
        }

        public void RemoveJobGroup(Job job, Villager villager = null)
        {
            var jobManager = FindObjectOfType<JobManager>();

            //Debug.Log(job.jobType + "; " + selectedVillager);

            Villager selectedVillager = null;

            if (villager == null)
                selectedVillager = this._selectedVillager;
            else
                selectedVillager = villager;

            var jobIndex = selectedVillager.jobList.IndexOf(job);

            switch (job.jobType)
            {
                //Cancel both move and chop jobs. (index - 1 and index)
                case "Chop":
                {
                    if (selectedVillager.performingJob)
                    {
                        jobManager.RemoveJob(selectedVillager._index, jobIndex);
                        selectedVillager.moveJobCancelled = true;
                    }
                    else
                    {
                        jobManager.RemoveJob(selectedVillager._index, jobIndex);
                        jobManager.RemoveJob(selectedVillager._index, jobIndex - 1);
                    }

                    //Remove axe from tree.
                    job.objectiveTransforms[0].GetComponent<Resource>().RemoveHarvestIndicator();
                    break;
                }

                //Cancel both move and build jobs. (index - 1 and index)
                case "Build":
                {
                    /*
                if (_selectedVillager.performingJob)
                {
                    jobManager.RemoveJob(_selectedVillager._index, jobIndex);
                    _selectedVillager.moveJobCancelled = true;
                }
                else
                {
                    jobManager.RemoveJob(_selectedVillager._index, jobIndex);
                    jobManager.RemoveJob(_selectedVillager._index, jobIndex - 1);
                }
                */

                    //Remove building prefab
                    StartCoroutine(PositionBuildingTemplate.RemoveTemplate(job.objectiveTransforms[0]));
                    break;
                }
                default:
                {
                    Debug.LogError("Invalid job type provided: " + job.jobType);
                    break;
                }
            }

            StartCoroutine(UpdateJobElementPositions());
        }
    }
}