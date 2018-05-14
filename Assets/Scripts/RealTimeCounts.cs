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
    private Text _totalCount;

    [SerializeField]
    private GameObject[] Dials;

    [SerializeField]
    private GameObject SourcesPanel;

    [SerializeField]
    private GameObject SourceItemPrefab;
    
    // Use this for initialization
    void Start()
    {
        StartCoroutine("GetRealTimeCounts");
    }

    protected IEnumerator GetRealTimeCounts()
    {
        while (true)
        {
            // Get Timestamp
            var n = System.DateTime.Now;
            var year = n.Year.ToString();
            var month = n.Month.ToString().Length < 2 ? "0" + n.Month : n.Month.ToString();
            var day = n.Day.ToString().Length < 2 ? "0" + n.Day : n.Day.ToString();
            string timeStamp = year + month + day + "00";

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
                float totalCount = response.Sum(x => x.count);
                _totalCount.text = GetPrettyNumber(totalCount);

                // Get and display percentage dials
                var _percentages = response.GroupBy(x => x.sourceId)
                    .Select(g => new
                    {
                        source = g.Key,
                        count = g.Sum(c => c.count),
                        percent = g.Sum(c => c.count) / totalCount
                    }).OrderByDescending(x => x.count).ToList();

                for (int i = 0; i < Dials.Length; i++)
                {
                    if (i < _percentages.Count)
                    {
                        Dials[i].SetActive(true);
                        Dials[i].transform.Find("Title").GetComponent<Text>().text = _percentages[i].source;
                        Dials[i].transform.Find("Number").GetComponent<Text>().text = _percentages[i].count.ToString("#,#", CultureInfo.InvariantCulture);
                        Dials[i].transform.Find("Radial_PFB/Fill").GetComponent<Image>().fillAmount = _percentages[i].percent; 
                    }
                    else
                    {
                        Dials[i].SetActive(false);
                    }
                }

                // Populate list of sources
                foreach (Transform child in SourcesPanel.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
                for (int i = 0; i < _percentages.Count(); i++)
                {
                    GameObject newSource = Instantiate(SourceItemPrefab) as GameObject;
                    ListItemController controller = newSource.GetComponent<ListItemController>();
                    controller.Title.text = _percentages[i].source;
                    controller.Count.text = _percentages[i].count.ToString("#,#", CultureInfo.InvariantCulture);
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
