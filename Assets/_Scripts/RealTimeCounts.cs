using Assets.Scripts.Json;
using ChartAndGraph;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static F;

public class RealTimeCounts : SerializedMonoBehaviour {
    public bool UpdateValues;

    [SerializeField]
    public float UpdateFrequency;

    [SerializeField]
    private IntVariable _totalCount;

    [SerializeField]
    private RectTransform ContentTransform;

    [SerializeField]
    private GameObject contentVisuals;

    [SerializeField]
    private GameObject contentLayout;

    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private GameObject SourceItemPrefab;

    [SerializeField]
    private GameObject SourceLayoutPrefab;

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
    private Material RealTimeInnerFillMaterial;

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
    Dictionary<string, SourceData> _sources = new Dictionary<string, SourceData>();
    Dictionary<string, List<DateAndCount>> _realTimeGraphData = new Dictionary<string, List<DateAndCount>>();
    Dictionary<string, List<DateAndCount>> _deepStorageGraphData = new Dictionary<string, List<DateAndCount>>();

    [SerializeField]
    List<Transform> _sourceLocations = new List<Transform>();
    Dictionary<string, GameObject> _sourceObjects = new Dictionary<string, GameObject>();

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

    [SerializeField]
    private IntVariable latestTimeStamp;
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
            var timeStamp = Some(now).Map(GetTimeStamp).Bind(Int.Parse);
            var lastTime = _latestTime;

            print("Current TimeStamp: " + now);

            var isNewDay = IsNewDay(latestTimeStamp.Value, timeStamp);
            latestTimeStamp.SetValue(timeStamp);

            if (isNewDay)
            {
                Reset();
            }

            // Update Processing flag
            var processingTime = new TimeSpan(0, 10, 0);
            IsProcessing.SetValue((now - lastTime).TotalSeconds > processingTime.TotalSeconds ? false : true);

            // Get RealTime Stats
            var realTimeRoutine = GetRealTimeRoutine(getStringSetting, "RealTimeApiKey", "RealTimeUrl", timeStamp);

            NestableCoroutine<string> rtCoroutine = new NestableCoroutine<string>(realTimeRoutine);
            foreach(var x in rtCoroutine.Routine) { yield return x; }

            var realTimeResponse = rtCoroutine.Value
                .Map(JsonHelper.FixJson)
                .Map(JsonHelper.FromJson<RealTimeRecord>);

            // Get DeepStorage Stats
            var deepStorageRoutine = GetRealTimeRoutine(getStringSetting, "DeepStorageApiKey", "DeepStorageUrl", timeStamp);

            NestableCoroutine<string> dsCoroutine = new NestableCoroutine<string>(deepStorageRoutine);
            foreach(var x in dsCoroutine.Routine) { yield return x; }

            var deepStorageResponse = dsCoroutine.Value
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
            realTimeResponse.AsEither()
                .Map(y => y.Aggregate(Some(0), (acc, record) => acc.Map(num => num + record.count)))
                .Match(
                    (e) => print(e),
                    (f) => _totalCount.SetValue(f));

            // Get Totals
            realTimeResponse.Map(GetTotals)
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

            // Update RealTime Graph Data
            deepStorageResponse.AsEither()
                .Match(
                    (e) => { print(e); },
                    (f) =>
                    {
                        // Update DeepStorage Data
                        UpdateGraphData(f, _deepStorageGraphData, true);
                    }
                );

            // Update Partitions and Graph
            realTimeResponse.AsEither()
                .Match(
                    (e) => { print(e); },
                    (f) =>
                    {
                        // Update Partition Activity
                        UpdatePartitions(f);

                        // Display Partitions
                        DisplayPartitions(lastTime);

                        // Update Graph
                        UpdateGraphData(f, _realTimeGraphData);

                        // Display Graph
                        DisplayGraph();
                    });

            // Get and Display Latency
            realTimeResponse.Map(GetLatency)
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

    private Option<IEnumerator> GetRealTimeRoutine(Func<string, Option<string>> getStringSetting, string apiKeyName, string urlName, Option<int> timeStamp)
    {
        // Get RealTime Stats
            var apiKey = getStringSetting(apiKeyName).Match(() => "", (f) => f);

            // Set Headers
            List<Tuple<string, string>> headers = new List<Tuple<string, string>>()
            {
                new Tuple<string,string>("Ocp-Apim-Trace", "true"),
                new Tuple<string,string>("Ocp-Apim-Subscription-Key", apiKey)
            };

            // Get response
            var routine = (from a in getStringSetting(urlName)
                            from b in timeStamp
                            select StringHelper.Append(a,b.ToString()))
                                .Bind(Url.Of)
                                .Map(url => WebRequest.GetWebText(url, headers, true));

            return routine;
    }

    private void DisplayLatency(int latency)
    {
        Latency.SetValue(latency);
    }

    private void UpdateGraphData(List<RealTimeRecord> response, Dictionary<string, List<DateAndCount>> graphData, bool addZulu = false)
    {
       var data = response
                .GroupBy(w =>
                {
                    var time = addZulu ? w.processedAt + "Z" : w.processedAt;
                    var dateTime = DateTime.Parse(time).ToUniversalTime();
                    return new
				    {
					    sourceId = w.sourceId,
					    processedAt = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0)
				    };
                })
                .Select(g => new
                {
                    sourceId = g.Key.sourceId,
					processedAt = g.Key.processedAt,
                    count = g.Sum(x => x.count)
                })
				.OrderBy(h => h.processedAt)
				.GroupBy(y => y.sourceId);
        foreach (var group in data)
        {
            var sourceId = group.Key;
            var list = group.Select(x => new DateAndCount(){ timestamp = x.processedAt, count = x.count}).ToList();
            graphData[sourceId] = list;
        }
    }

    private void DisplayGraph()
    {
        if (Graph != null && _sourcesTimes.Count > 0)
        {
            var index = currentRotation++ % _sourcesTimes.Keys.Count;
            var source = _sourcesTimes.Keys.ElementAt(index);
            DisplayGraphForSource(source);

            Transform scrollTarget = ContentTransform.transform;
            var childTransform = contentVisuals.transform.GetChild(index);
            Button btn = childTransform.GetComponent<Button>();

            if (btn != null) // highlight button
            {
                btn.Select();
                btn.OnSelect(null);
                scrollTarget = childTransform;
                Debug.Log("Current Source: " + source);
            }

            ContentTransform.anchoredPosition = (Vector2)scrollRect.transform.InverseTransformPoint(ContentTransform.position)
                - (Vector2)scrollRect.transform.InverseTransformPoint(scrollTarget.position) + ScrollSnapPivot;
        }
    }

    private void DisplayGraphForSource(string source)
    {
        if (!_realTimeGraphData.ContainsKey(source))
        {
            Debug.Log(source + " selected but no data exists!");
            return;
        }

        List<DateAndCount> rtData = new List<DateAndCount>();
        if (_realTimeGraphData.ContainsKey(source))
        {
            rtData = _realTimeGraphData[source];
        } 
        List<DateAndCount> dsData = new List<DateAndCount>();
        if (_deepStorageGraphData.ContainsKey(source))
        {
            dsData = _deepStorageGraphData[source];
        }
        Debug.Log("Loading Graph...");
        Graph.DataSource.StartBatch();
        Graph.DataSource.Clear();

        // Add real-time data
        Graph.DataSource.AddCategory(source, CategoryMaterial, 2, new MaterialTiling() { EnableTiling = false }, InnerFillMaterial, false, PointMaterial, 6);
        int runningTotal = 0;
        foreach (var record in rtData)
        {
            var timeStamp = record.timestamp;
            runningTotal += record.count;
            Graph.DataSource.AddPointToCategory(source, timeStamp, runningTotal);
        }

        // Add deep storage data
        var dsCategory = source + "_ds";
        Graph.DataSource.AddCategory(dsCategory, CategoryMaterial, 2, new MaterialTiling() { EnableTiling = false }, RealTimeInnerFillMaterial, false, PointMaterial, 6);
        foreach (var record in dsData)
        {
            var timeStamp = record.timestamp;
            // Real-Time data is already a running total
            Graph.DataSource.AddPointToCategory(dsCategory, timeStamp, record.count);
        }

        Graph.DataSource.EndBatch();
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
        if (_sourcesTimes.Count == 0)
        {
            return;
        }
        
        // Make sure there are enough place holders
        if (_sourceLocations.Count() < totals.Count())
        {
            var difference = totals.Count() - _sourceLocations.Count();
            for (int i = 0; i <= difference; i++)
            {
                var newPlaceholder = Instantiate(SourceLayoutPrefab);
                newPlaceholder.transform.parent = contentLayout.transform;
                _sourceLocations.Add(newPlaceholder.transform);
            }
        }

        var index = currentRotation % _sourcesTimes.Keys.Count;
        Transform scrollTarget = ContentTransform.transform;

        for (int i = 0; i < totals.Count(); i++)
        {
            SourceData sourceData;
            GameObject source;
            var item = totals[i];
            // Check to see if the SourceData already exists
            if (_sources.ContainsKey(item.source))
            {
                sourceData = _sources[item.source];
                source = _sourceObjects[item.source];
            }
            else
            {
                sourceData = ScriptableObject.CreateInstance<SourceData>();
                sourceData.Title.SetValue(item.source);
                source = Instantiate(SourceItemPrefab) as GameObject;
                SourceController controller = source.GetComponent<SourceController>();
                controller.sourceData = sourceData;
                controller.SourceLocations = _sourceLocations;
                controller.DeepSix = contentVisuals.transform;
                source.transform.parent = contentVisuals.transform;
                source.transform.position = contentVisuals.transform.position;
                source.transform.localScale = Vector3.one;
            }
            sourceData.Count.SetValue(item.count);
            sourceData.Index.SetValue(i);
            sourceData.Enabled.SetValue(true);
            
            _sources[item.source] = sourceData;
            _sourceObjects[item.source] = source;
        }
        Canvas.ForceUpdateCanvases();
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

    public void Reset()
    {
        Latency.SetValue(None);
        Velocity.SetValue(None);
        _totalCount.SetValue(None);
        SceneManager.LoadScene(0);
    }

    public void SourceClicked(string buttonName)
    {
        Debug.Log(buttonName + " Clicked");
        DisplayGraphForSource(buttonName);
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

    private static bool IsNewDay(Option<int> lastTime, Option<int> now)
    {
        return lastTime
            .Match(
            () => now.Match(
                () => false,
                (f) => true
            ),
            (f) => now.Match(
                () => true,
                (n) => f/100 != n/100
            )
        );
    }
}
