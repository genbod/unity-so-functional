using Assets.Scripts.Json;
using ChartAndGraph;
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
    private RectTransform contentPanel;

    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private GameObject SourceItemPrefab;

    [SerializeField]
    private Partition[] Partitions;

    [SerializeField]
    private GraphChartBase Graph;

    [SerializeField]
    private Material CategoryMaterial;

    [SerializeField]
    private Material PointMaterial;

    [SerializeField]
    private Material InnerFillMaterial;

    [SerializeField]
    private Vector2 ScrollSnapPivot;

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
    Dictionary<string, DateAndCount> _partitionTimes = new Dictionary<string, DateAndCount>();

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

    int currentRotation = 0;
    
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
            var lastTime = _latestTime;

            var url = "https://artisapi.redteam.ms/realTime/counts?timeIntervalStart=" + timeStamp;
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SetRequestHeader("Ocp-Apim-Trace", "true");
            www.SetRequestHeader("Ocp-Apim-Subscription-Key", "cc46c19debb84717a0f5ba759190865c");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Fix JSON because Unity needs an object root
                var response = JsonHelper.FromJson<RealTimeRecord>(FixJson(www.downloadHandler.text));

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

                // Update Partition Activity
                UpdatePartitions(response);

                // Display Partitions
                DisplayPartitions(lastTime);

                // Display Graph
                DisplayGraph(response);
            }

            yield return new WaitForSeconds(UpdateFrequency);
        }
    }

    private void DisplayGraph(List<RealTimeRecord> response)
    {
        if (Graph != null)
        {
            var index = currentRotation++ % _sourcesTimes.Keys.Count;
            var source = _sourcesTimes.Keys.ElementAt(index);
            var data = response.Where(x => x.sourceId == source)
                .GroupBy(x => x.processedAt)
                .Select(g => new
                {
                    processedAt = g.Key,
                    g.FirstOrDefault().sourceId,
                    count = g.Sum(x => x.count)
                });
            Debug.Log("Loading Graph...");
            Graph.DataSource.StartBatch();
            Graph.DataSource.Clear();

            // Get one result
            Graph.DataSource.AddCategory(source, CategoryMaterial, 2, new MaterialTiling() { EnableTiling = false }, InnerFillMaterial, false, PointMaterial, 6);
            foreach (var record in data)
            {
                var timeStamp = record.processedAt;
                //var timeStamp = record.processedAt.Substring(0, 4) + "/" + record.processedAt.Substring(4, 2) + "/" + record.processedAt.Substring(6, 2) + " " + record.processedAt.Substring(8, 2) + ":00";
                Graph.DataSource.AddPointToCategory(record.sourceId, DateTime.Parse(timeStamp).ToUniversalTime(), record.count);
            }
            //foreach (var sourceId in _sourcesTimes.Keys)
            //{
            //    Graph.DataSource.AddCategory(sourceId, CategoryMaterial, 2, new MaterialTiling() { EnableTiling = false }, null, false, PointMaterial, 6);
            //}
            //foreach (var record in response)
            //{
            //    var timeStamp = DateTime.Parse(record.processedAt).ToUniversalTime();
            //    Graph.DataSource.AddPointToCategory(record.sourceId, timeStamp, record.count);
            //}
            Graph.DataSource.EndBatch();
        }
    }

    private void DisplayPartitions(DateTime lastTime)
    {
        for (int i = 0; i < Partitions.Length; i++)
        {
            string index = i.ToString();
            var timeSinceLastUpdate = _latestTime - _partitionTimes[index].timestamp;
            var curPartition = _partitionTimes[i.ToString()];
            if (curPartition.timestamp > lastTime)
            {
                Partitions[i].Fill.FillAmount = 1;
                Partitions[i].Text.color = Color.white;
                curPartition.velocity = 3;
            }
            else
            {
                curPartition.velocity = curPartition.velocity >= 1 ? curPartition.velocity - 1 : 0; 
            }
            _partitionTimes[index] = curPartition;

            if (curPartition.velocity < 1)
            {
                Partitions[i].Text.color = Color.red;
            }
            else if (curPartition.velocity < 2)
            {
                Partitions[i].Text.color = Color.yellow;
            }
        }
    }

    private void UpdatePartitions(List<RealTimeRecord> response)
    {
        var partitions = response.GroupBy(x => x.partitionId)
            .Select(g => new
            {
                partition = g.Key,
                lastTime = g.OrderByDescending(c => c.processedAt).Select(c => c.processedAt).FirstOrDefault(),
                count = g.Sum(c => c.count)
            });

        foreach (var p in partitions)
        {
            var curTime = DateTime.Parse(p.lastTime).ToUniversalTime();
            if (_partitionTimes.ContainsKey(p.partition))
            {
                var existingPartition = _partitionTimes[p.partition];
                if (curTime > existingPartition.timestamp)
                {
                    existingPartition.timestamp = curTime;
                    //existingPartition.velocity = (p.count - existingPartition.count) / (curTime - existingPartition.timestamp).TotalSeconds;
                    existingPartition.count = p.count;
                    _partitionTimes[p.partition] = existingPartition;
                }
            }
            else
            {
                DateAndCount newPartition = new DateAndCount() { count = p.count, timestamp = curTime, velocity = p.count / (curTime - _latestTime).TotalSeconds };
                _partitionTimes[p.partition] = newPartition;
            }
        }
    }

    private void DisplaySources(List<Total> totals)
    {
        var index = currentRotation % _sourcesTimes.Keys.Count;
        Transform scrollTarget = contentPanel.transform;
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
            if (i == index) // highlight button
            {
                Button btn = newSource.GetComponent<Button>();
                btn.Select();
                btn.OnSelect(null);
                scrollTarget = newSource.transform;
                Debug.Log("Current Source: " + totals[i].source);
            }
        }
        Canvas.ForceUpdateCanvases();
        contentPanel.anchoredPosition = (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
            - (Vector2)scrollRect.transform.InverseTransformPoint(scrollTarget.position) + ScrollSnapPivot;
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
