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


    Dictionary<string, DateAndCount> _sourcesTimes = new Dictionary<string, DateAndCount>();

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
            var year = now.Year.ToString();
            var month = now.Month.ToString().Length < 2 ? "0" + now.Month : now.Month.ToString();
            var day = now.Day.ToString().Length < 2 ? "0" + now.Day : now.Day.ToString();
            string timeStamp = year + month + day + "00";

            // Get elapsed Time
            var elapsedTime = now - _latestTime;

            // Get Data
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

                // Get and display totals
                var _totals = response.GroupBy(x => x.sourceId)
                    .Select(g => new
                    {
                        source = g.Key,
                        count = g.Sum(c => c.count),
                        lastTime = g.OrderByDescending(c => c.processedAt).Select(c => c.processedAt).FirstOrDefault(),
                        velocity = 0
                    }).OrderByDescending(x => x.count).ToList();

                double totalVelocity = 0;

                foreach (var item in _totals)
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
                        // Update entry;
                        _sourcesTimes[item.source] = prevTimeCount;
                    }
                    else
                    {
                        var curVelocity = item.count / elapsedTime.TotalSeconds;
                        totalVelocity += curVelocity;
                        _sourcesTimes[item.source] = new DateAndCount() { count = item.count, timestamp = curTimeStamp, velocity = curVelocity };
                    }

                    _latestTime = curTimeStamp > _latestTime ? curTimeStamp : _latestTime;
                }

                var sortedVelocities = _sourcesTimes.OrderByDescending(c => c.Value.velocity).Take(Dials.Length).ToList();
                totalVelocity = totalVelocity > 0 ? totalVelocity : 1;

                for (int i = 0; i < Dials.Length; i++)
                {
                    if (i < _totals.Count)
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

                // Populate list of sources
                foreach (Transform child in SourcesPanel.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
                for (int i = 0; i < _totals.Count(); i++)
                {
                    GameObject newSource = Instantiate(SourceItemPrefab) as GameObject;
                    ListItemController controller = newSource.GetComponent<ListItemController>();
                    controller.Title.text = _totals[i].source;
                    controller.Count.text = _totals[i].count.ToString("#,#", CultureInfo.InvariantCulture);
                    newSource.transform.parent = SourcesPanel.transform;
                    newSource.transform.localScale = Vector3.one;
                }
            }

            yield return new WaitForSeconds(UpdateFrequency);
        }
    }
    public static class JsonHelper
    {
        public static List<T> FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(List<T> array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(List<T> array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public List<T> Items;
        }
    }

    private string FixJson(string value)
    {
        value = "{\"Items\":" + value + "}";
        return value;
    }

    private string GetPrettyNumber(float totalCount)
    {
        if (totalCount > 100000)
        {
            var millions = totalCount / 1000000;
            return millions.ToString("0.00") + "M";
        }
        return (totalCount / 1000).ToString("0.00") + "K";
    }

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
}
