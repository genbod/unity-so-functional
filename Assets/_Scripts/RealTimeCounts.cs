using Assets.Scripts.Json;
using ChartAndGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static F;

public class RealTimeCounts : MonoBehaviour {
    [SerializeField]
    public float UpdateFrequency;

    [SerializeField]
    private IntVariable _totalCount;

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

    [SerializeField]
    private Text Latency;

    [SerializeField]
    private Text Version;

    [SerializeField]
    private DoubleVariable Velocity;

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

    Dictionary<string, string> _settings = new Dictionary<string, string>();
    
    // Use this for initialization
    IEnumerator Start()
    {
        // Set Version
        var routine = new NestableCoroutine<string>(GetVersion());
        yield return routine.StartCoroutine(this);

        routine.Value.Match(
            () => Version.text = "Version Could not be read.",
            (f) => Version.text = f);
        //Version.text = routine.Value;

        _totalCount.Value = 0;
        foreach (var Dial in Dials)
        {
            Dial.Number.SetCurrentCount(0);
        }

        _latestTime = System.DateTime.Now.ToUniversalTime();
        StartCoroutine("GetRealTimeCounts");
    }

    IEnumerator GetVersion()
    {
        print(Application.streamingAssetsPath);

        NestableCoroutine<string> coroutineObject = new NestableCoroutine<string>(VersionHelper.GetVersion(Application.streamingAssetsPath));
        foreach(var x in coroutineObject.Routine) { yield return x; }

        print("Found Version: " + coroutineObject.Value);
        yield return coroutineObject.Value;
    }

    IEnumerator GetParameters(string filePath)
    {
        print(filePath);

        string result = "";

        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            WWW www = new WWW(filePath);
            yield return www;
            result = www.text;
        }
        else
        {
            result = File.ReadAllText(filePath);
        }

        if (result != "")
        {
            XmlDocument userXml1 = new XmlDocument();

            userXml1.LoadXml(result);

            XmlNodeList settingsList = userXml1.GetElementsByTagName("parameter");

            print("Found Parameters: " + settingsList.Count);

            foreach (XmlNode settingValue in settingsList)
            {
                _settings.Add(settingValue.Attributes["name"].Value, settingValue.Attributes["value"].Value);

            }
        }
    }

    T GetSetting<T>(string name) where T : IComparable
    {
        IComparable value = default(T);
        Type settingType = typeof(T);
        if (_settings.ContainsKey(name) && _settings[name] != "" && !_settings[name].StartsWith("#{"))
        {
            var valueString = _settings[name];
            if (settingType == typeof(int))
            {
                value = int.Parse(valueString);
            }
            else if (settingType == typeof(float))
            {
                value = float.Parse(valueString);
            }
            else if (settingType == typeof(string))
            {
                value = valueString;
            }
        }
        else if(PlayerPrefs.HasKey(name))// Look for value in PlayerPrefs
        {
            if (settingType == typeof(int))
            {
                value = PlayerPrefs.GetInt(name);
            }
            else if (settingType == typeof(float))
            {
                value = PlayerPrefs.GetFloat(name);
            }
            else if (settingType == typeof(string))
            {
                value = PlayerPrefs.GetString(name);
            }
        }

        // return value
        return (T)value;
    }

    protected IEnumerator GetRealTimeCounts()
    {
        // Get Settings
        string filePath = Application.streamingAssetsPath + "/parameters.xml";
        yield return StartCoroutine(GetParameters(filePath));

        while (true)
        {
            // Get Timestamp
            var now = System.DateTime.Now.ToUniversalTime();
            string timeStamp = GetTimeStamp(now);
            var lastTime = _latestTime;

            List<Tuple<string, string>> headers = new List<Tuple<string, string>>()
            {
                new Tuple<string,string>("Ocp-Apim-Trace", "true"),
                new Tuple<string,string>("Ocp-Apim-Subscription-Key", GetSetting<string>("ApiKey"))
            };
            var routine = Url.Of(GetSetting<string>("Url") + timeStamp).Map(url => WebRequest.GetJsonString(url, headers));
            NestableCoroutine<string> coroutine = new NestableCoroutine<string>(routine);
            foreach(var x in coroutine.Routine) { yield return x; }

            var text = coroutine.Value.Match(
                () => "",
                (f) => f);

            if (coroutine.e != null)
            {
                Debug.Log(coroutine.e);
            }
            else if (String.IsNullOrEmpty(text))
            {
                Debug.Log("Web Request did not return test");
            }
            else
            {
                // Fix JSON because Unity needs an object root
                var response = JsonHelper.FromJson<RealTimeRecord>(FixJson(text));

                // Get and display totalCount of events
                int totalCount = response.Sum(x => x.count);
                _totalCount.Value = totalCount;

                // Get Totals
                var _totals = GetTotals(response);

                // Update Velocities
                Velocity.Value = UpdateVelocities(_totals, now);

                // Display Top Velocities
                DisplayVelocities(Velocity.Value);

                // Display list of sources
                DisplaySources(_totals);

                // Update Partition Activity
                UpdatePartitions(response);

                // Display Partitions
                DisplayPartitions(lastTime);

                // Display Graph
                DisplayGraph(response);

                var latency = GetLatency(response);

                // Display Latency
                DisplayLatency(latency);
            }

            yield return new WaitForSeconds(UpdateFrequency);
        }
    }

    private void DisplayLatency(int latency)
    {
        Latency.text = latency.ToString();
    }

    private void DisplayGraph(List<RealTimeRecord> response)
    {
        if (Graph != null && _sourcesTimes.Count > 0)
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
            if (!_partitionTimes.ContainsKey(index))
            {
                continue;
            }

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
        // Clear list of sources
        foreach (Transform child in SourcesPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        if (_sourcesTimes.Count == 0)
        {
            return;
        }

        var index = currentRotation % _sourcesTimes.Keys.Count;
        Transform scrollTarget = contentPanel.transform;

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
                Dials[i].Number.TargetCount.Value = sortedVelocities[i].Value.count;
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

        // clear out old data
        if (totals.Count < _sourcesTimes.Count)
        {
            foreach (var source in _sourcesTimes.Keys.ToList())
            {
                if (!totals.Exists(x => x.source == source))
                {
                    _sourcesTimes.Remove(source);
                }
            }
        }

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

    private static int GetLatency(List<RealTimeRecord> response)
    {
        var latestEvent = response
            .OrderByDescending(x => x.processedAt).FirstOrDefault();
        if (latestEvent != null)
        {
            var period = DateTime.Parse(latestEvent.period.Substring(0, 4) + "/" + latestEvent.period.Substring(4, 2) + "/" + latestEvent.period.Substring(6, 2) + " " + latestEvent.period.Substring(8, 2) + ":00");
            var processedAt = DateTime.Parse(latestEvent.processedAt).ToUniversalTime();

            return (processedAt - period).Hours;
        }
        else return -1;
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
