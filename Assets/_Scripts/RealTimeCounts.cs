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
    public bool UpdateValues;

    [SerializeField]
    public float UpdateFrequency;

    [SerializeField]
    private IntVariable _totalCount;

    [SerializeField]
    private GameObject SourcesPanel;

    [SerializeField]
    private RectTransform contentPanel;

    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private GameObject SourceItemPrefab;

    [SerializeField]
    private GameObject DialPrefab;

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
    private IntVariable Latency;

    [SerializeField]
    private Text Version;

    [SerializeField]
    private DoubleVariable Velocity;

    [SerializeField]
    private BoolVariable IsProcessing;

    [SerializeField]
    private GameObject DialOrigin;

    [SerializeField]
    private List<Transform> DialLocations;

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
    Dictionary<string, DialData> _dials = new Dictionary<string, DialData>();

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
        // Get Version
        var routine = new NestableCoroutine<string>(GetVersion());
        yield return routine.StartCoroutine(this);

        routine.Value.Match(
            (e) => Version.text = "Version Could not be read.",
            (f) => Version.text = f);

        _latestTime = System.DateTime.Now.ToUniversalTime();
        StartCoroutine("GetRealTimeCounts");
    }

    IEnumerator GetVersion()
    {
        string filePath = Application.streamingAssetsPath + "/version.xml";
        print(filePath);

        NestableCoroutine<string> coroutineObject = new NestableCoroutine<string>(VersionHelper.GetVersion(filePath));
        foreach(var x in coroutineObject.Routine) { yield return x; }

        var message = coroutineObject.Value.Match(
            (e) => "Could Not Find Version.",
            (f) => "Found Version: " + f);
        print(message);
        yield return coroutineObject.Value;
    }

    protected IEnumerator GetRealTimeCounts()
    {
        // Get Settings
        string filePath = Application.streamingAssetsPath + "/parameters.xml";
        var getSettingsR = new NestableCoroutine<IEnumerable<Tuple<string, string>>>(LocalSettingsHelper.GetSettings(filePath));
        yield return getSettingsR.StartCoroutine(this);
        _settings = getSettingsR.Value.Match(
            (e) => new Dictionary<string, string>(),
            (f) => f.ToDictionary(x => x.Item1, x => x.Item2));

        print("Found Parameters: " + _settings.Count);

        var getStringSetting = _settings.GetSetting<string>();

        while (true)
        {
            // Get Timestamp
            var now = System.DateTime.Now.ToUniversalTime();
            string timeStamp = GetTimeStamp(now);
            var lastTime = _latestTime;
            var apiKey = getStringSetting("ApiKey").Match(() => "", (f) => f);

            print("Current TimeStamp: " + now);

            // Update Processing flag
            var processingTime = new TimeSpan(0, 10, 0);
            IsProcessing.SetValue((now - lastTime).TotalSeconds > processingTime.TotalSeconds ? false : true);

            // Set Headers
            List<Tuple<string, string>> headers = new List<Tuple<string, string>>()
            {
                new Tuple<string,string>("Ocp-Apim-Trace", "true"),
                new Tuple<string,string>("Ocp-Apim-Subscription-Key", apiKey)
            };

            // Get response
            var routine = getStringSetting("Url")
                .Map(s => StringHelper.Append(s, timeStamp))
                .Bind(Url.Of)
                .Map(url => WebRequest.GetWebText(url, headers));
            NestableCoroutine<string> coroutine = new NestableCoroutine<string>(routine);
            foreach(var x in coroutine.Routine) { yield return x; }

            var response = coroutine.Value
                .Map(JsonHelper.FixJson)
                .Map(JsonHelper.FromJson<RealTimeRecord>);

            // If we aren't updating values then wait and short-circuit
            if (!UpdateValues)
            {
                yield return new WaitForSeconds(UpdateFrequency);
                continue;
            }

            //------------------- Start of Side Effects :( _---------------------------------------
            // Get and display totalCount of events
            response.AsEither()
                .Map(y => y.Aggregate(Some(0), (acc, record) => acc.Map(num => num + record.count)))
                .Match(
                    (e) => print(e),
                    (f) => _totalCount.SetValue(f));

            // Get Totals
            response.Map(GetTotals)
                .AsEither()
                .Match(
                    (e) => { print(e); },
                    (_totals) =>
                    {
                        // Update Velocities
                        var velocity = UpdateVelocities(_totals, now, lastTime);

                        Velocity.SetValue(Some(velocity));

                        // Display Top Velocities
                        DisplayVelocities(velocity);

                        // Display list of sources
                        DisplaySources(_totals);
                    });

            // Update Partitions and Graph
            response.AsEither()
                .Match(
                    (e) => { print(e); },
                    (f) =>
                    {
                        // Update Partition Activity
                        UpdatePartitions(f);

                        // Display Partitions
                        DisplayPartitions(lastTime);

                        // Display Graph
                        DisplayGraph(f);
                    });

            // Get and Display Latency
            response.Map(GetLatency)
                .Match(
                    (e) => { print(e); },
                    (latency) =>
                    {
                        // Display Latency
                        DisplayLatency(latency);
                    });

            yield return new WaitForSeconds(UpdateFrequency);
        }
    }

    private void DisplayLatency(int latency)
    {
        Latency.SetValue(latency);
    }

    private void DisplayGraph(List<RealTimeRecord> response)
    {
        if (Graph != null && _sourcesTimes.Count > 0)
        {
            var index = currentRotation++ % _sourcesTimes.Keys.Count;
            var source = _sourcesTimes.Keys.ElementAt(index);
            var data = response.Where(x => x.sourceId == source)
                .GroupBy(x =>
                {
                    var dateTime = DateTime.Parse(x.processedAt).ToUniversalTime();
                    return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
                })
                .OrderBy(g => g.Key)
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
            int runningTotal = 0;
            foreach (var record in data)
            {
                var timeStamp = record.processedAt;
                runningTotal += record.count;
                //var timeStamp = record.processedAt.Substring(0, 4) + "/" + record.processedAt.Substring(4, 2) + "/" + record.processedAt.Substring(6, 2) + " " + record.processedAt.Substring(8, 2) + ":00";
                Graph.DataSource.AddPointToCategory(record.sourceId, timeStamp, runningTotal);
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

            if(!IsProcessing.GetValue())
            {
                Partitions[i].Text.color = Color.white;
            }
            else if (curPartition.velocity < 1)
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
        var sortedVelocities = _sourcesTimes.OrderByDescending(c => c.Value.velocity).ToList();
        totalVelocity = totalVelocity > 0 ? totalVelocity : 1;

        for (int i = 0; i < sortedVelocities.Count(); i++)
        {
            DialData dial;
            var item = sortedVelocities[i];
            // Check to see if the DialData already exists
            if (_dials.ContainsKey(item.Key))
            {
                dial = _dials[item.Key];
            }
            else
            {
                dial = ScriptableObject.CreateInstance<DialData>();
                dial.Name.SetValue(item.Key);
                GameObject newDial = Instantiate(DialPrefab) as GameObject;
                DialController controller = newDial.GetComponent<DialController>();
                controller.dialData = dial;
                controller.DialLocations = DialLocations;
                controller.DeepSix = DialOrigin.transform;
                newDial.transform.parent = DialOrigin.transform;
                newDial.transform.position = DialOrigin.transform.position;
                newDial.transform.localScale = Vector3.one;
            }
            dial.Count.SetValue(item.Value.count);
            dial.FillAmount.SetValue((float)(item.Value.velocity / totalVelocity));
            dial.Index.SetValue(i);
            if (i <= 5)
            {
                dial.Enabled.SetValue(true);
            }
            else dial.Enabled.SetValue(false);

            _dials[item.Key] = dial;
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
    private double UpdateVelocities(List<Total> totals, DateTime now, DateTime lastTime)
    {
        print("Entered Update Velocities");

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
                var elapsedTime = now - lastTime;
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
}
