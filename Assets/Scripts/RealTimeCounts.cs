using Assets.Scripts.Json;
using Bolt;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RealTimeCounts : MonoBehaviour {
    [SerializeField]
    public float UpdateFrequency;

    [SerializeField]
    private AnimatedCounter _totalCount;

    [SerializeField]
    private Dial[] Dials;

    [SerializeField]
    private GameObject SourcesPanel;

    [SerializeField]
    private GameObject SourceItemPrefab;

    struct DateAndCount
    {
        public DateTime timestamp;
        public int count;
        public double velocity;
    }

    struct Total
    {
        public string source;
        public int count;
        public string lastTime;
    }

    Dictionary<string, DateAndCount> _sourcesTimes = new Dictionary<string, DateAndCount>();

    [Serializable]
    public class RealTimeRecord
    {
        public string id;
        public string period;
        public string sourceId;
        public string partitionId;
        public int count;
        public string processedAt;
    }

    DateTime _latestTime;
    
    // Use this for initialization
    void Start()
    {
        _totalCount.SetCurrentCount(0);
        foreach (var Dial in Dials)
        {
            Dial.Number.SetCurrentCount(0);
        }
        _latestTime = System.DateTime.Now.ToUniversalTime();
        StartCoroutine("GetRealTimeCounts");
    }

    protected IEnumerator GetRealTimeCounts()
    {
        while (true)
        {
            // Get Timestamp
            var now = System.DateTime.Now.ToUniversalTime();
            string timeStamp = GetTimeStamp(now);

            // Get Data
            Action<string> callback = (responseFromServer) =>
            {
                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    // Fix JSON because Unity needs an object root
                    var response = JsonHelper.FromJson<RealTimeRecord>(FixJson(responseFromServer));

                    // Get and display totalCount of events
                    int totalCount = response.Sum(x => x.count);
                    _totalCount.TargetCount = totalCount;

                    // Get Totals
                    var _totals = GetTotals(response);

                    // Update Velocities
                    double totalVelocity = UpdateVelocities(_totals, now);

                    // Display Top Velocities
                    DisplayVelocities(totalVelocity);

                    // Display list of sources
                    DisplaySources(_totals);
                }
            };

            StartCoroutine(GetData(timeStamp, callback));

            yield return new WaitForSeconds(UpdateFrequency);
        }
    }

    private void DisplaySources(List<Total> totals)
    {
        // Populate list of sources
        foreach (Transform child in SourcesPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        for (int i = 0; i < totals.Count(); i++)
        {
            GameObject newSource = Instantiate(SourceItemPrefab) as GameObject;
            ListItemController controller = newSource.GetComponent<ListItemController>();
            controller.Title.text = totals[i].source;
            controller.Count.text = totals[i].count.ToString("#,#", CultureInfo.InvariantCulture);
            newSource.transform.parent = SourcesPanel.transform;
            newSource.transform.localScale = Vector3.one;
        }
    }

    private void DisplayVelocities(double totalVelocity)
    {
        var sortedVelocities = _sourcesTimes.OrderByDescending(c => c.Value.velocity).Take(Dials.Length).ToList();
        totalVelocity = totalVelocity > 0 ? totalVelocity : 1;

        for (int i = 0; i < Dials.Length; i++)
        {
            if (i < sortedVelocities.Count)
            {
                Dials[i].gameObject.SetActive(true);
                Dials[i].Title.text = sortedVelocities[i].Key;
                Dials[i].Number.TargetCount = sortedVelocities[i].Value.count;
                Dials[i].Fill.fillAmount = (float)(sortedVelocities[i].Value.velocity / totalVelocity);
            }
            else
            {
                Dials[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Calculates current velocities of every source and updates sourcesTimes dictionary.  Returns total current velocities and updates
    /// _latestTime based on the newest timestamp it has seen.
    /// </summary>
    /// <param name="totals">Results from web request</param>
    /// <param name="now">Current TimeStamp</param>
    /// <param name="sourcesTimes">Dictionary of velocities to be updated</param>
    /// <returns>Total current velocity</returns>
    private double UpdateVelocities(List<Total> totals, DateTime now)
    {
        double totalVelocity = 0;

        foreach (var item in totals)
        {
            DateTime curTimeStamp = DateTime.Parse(item.lastTime).ToUniversalTime();
            if (_sourcesTimes.ContainsKey(item.source))
            {
                DateAndCount prevTimeCount = _sourcesTimes[item.source];
                if (curTimeStamp > prevTimeCount.timestamp)
                {
                    var countsProcessed = item.count - prevTimeCount.count;
                    var timeElapsed = (curTimeStamp - prevTimeCount.timestamp).TotalSeconds;
                    var velocity = countsProcessed / timeElapsed;
                    prevTimeCount.count = item.count;
                    prevTimeCount.timestamp = curTimeStamp;
                    prevTimeCount.velocity = velocity;
                    totalVelocity += velocity;
                }
                else
                {

                    var timeSinceLastCount = (now - curTimeStamp).TotalSeconds;
                    prevTimeCount.velocity = timeSinceLastCount > 180 ? 0 : Mathf.Floor((float)(prevTimeCount.count / timeSinceLastCount));
                    totalVelocity += prevTimeCount.velocity;
                }
                // Update entry
                _sourcesTimes[item.source] = prevTimeCount;
            }
            else
            {
                // Get elapsed Time
                var elapsedTime = now - _latestTime;
                var curVelocity = item.count / elapsedTime.TotalSeconds;
                totalVelocity += curVelocity;
                _sourcesTimes[item.source] = new DateAndCount() { count = item.count, timestamp = curTimeStamp, velocity = curVelocity };
            }

            _latestTime = curTimeStamp > _latestTime ? curTimeStamp : _latestTime;
        }

        return totalVelocity;
    }

    private static List<Total> GetTotals(List<RealTimeRecord> response)
    {
        return response.GroupBy(x => x.sourceId)
            .Select(g => new Total
            {
                source = g.Key,
                count = g.Sum(c => c.count),
                lastTime = g.OrderByDescending(c => c.processedAt).Select(c => c.processedAt).FirstOrDefault()
            }).OrderByDescending(x => x.count).ToList();
    }

    private static IEnumerator GetData(string timeStamp, Action<string> callback)
    {
        var url = "https://artisapi.redteam.ms/realTime/counts?timeIntervalStart=" + timeStamp;
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Ocp-Apim-Trace", "true");
        www.SetRequestHeader("Ocp-Apim-Subscription-Key", "cc46c19debb84717a0f5ba759190865c");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            callback(string.Empty);
        }
        else
        {
            var result = www.downloadHandler.text;
            callback(result);
        }
    }

    private static string GetTimeStamp(DateTime now)
    {
        var year = now.Year.ToString();
        var month = now.Month.ToString().Length < 2 ? "0" + now.Month : now.Month.ToString();
        var day = now.Day.ToString().Length < 2 ? "0" + now.Day : now.Day.ToString();
        return year + month + day + "00";
    }

    private string FixJson(string value)
    {
        value = "{\"Items\":" + value + "}";
        return value;
    }
}
