using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace Config
{
    // Partial struct for storing UI configuration data
    public partial struct UIConfig
    {
        // Deserialize UIConfig data from an Addressable asset
        public static void DeserializeByAddressable(string directory)
        {
            string path = $"{directory}/UIConfig.json";
            UnityEngine.TextAsset ta = Addressables.LoadAssetAsync<UnityEngine.TextAsset>(path).WaitForCompletion();
            string json = ta.text;
            datas = new List<UIConfig>();
            indexMap = new Dictionary<int, int>();
            JArray array = JArray.Parse(json);
            Count = array.Count;
            for (int i = 0; i < array.Count; i++)
            {
                JObject dataObject = array[i] as JObject;
                UIConfig data = (UIConfig)dataObject.ToObject(typeof(UIConfig));
                datas.Add(data);
                indexMap.Add(data.ID, i);
            }
            GameManager.UI.OpenUI(UIViewID.LoginUI); // Open Login UI after deserialization
        }

        // Deserialize UIConfig data from a file
        public static void DeserializeByFile(string directory)
        {
            string path = $"{directory}/UIConfig.json";
            using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(fs))
                {
                    datas = new List<UIConfig>();
                    indexMap = new Dictionary<int, int>();
                    string json = reader.ReadToEnd();
                    JArray array = JArray.Parse(json);
                    Count = array.Count;
                    for (int i = 0; i < array.Count; i++)
                    {
                        JObject dataObject = array[i] as JObject;
                        UIConfig data = (UIConfig)dataObject.ToObject(typeof(UIConfig));
                        datas.Add(data);
                        indexMap.Add(data.ID, i);
                    }
                }
            }
        }

        // Deserialize UIConfig data from an asset bundle
        public static System.Collections.IEnumerator DeserializeByBundle(string directory, string subFolder)
        {
            string bundleName = $"{subFolder}/UIConfig.bytes".ToLower();
            string fullBundleName = $"{directory}/{bundleName}";
            string assetName = $"assets/{bundleName}";

            #if UNITY_WEBGL && !UNITY_EDITOR
            UnityEngine.AssetBundle bundle = null;
            UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(fullBundleName);
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                UnityEngine.Debug.LogError(request.error);
            }
            else
            {
                bundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
            }
            #else
            yield return null;
            UnityEngine.AssetBundle bundle = UnityEngine.AssetBundle.LoadFromFile($"{fullBundleName}", 0, 0);
            #endif

            UnityEngine.TextAsset ta = bundle.LoadAsset<UnityEngine.TextAsset>($"{assetName}");
            string json = ta.text;
            datas = new List<UIConfig>();
            indexMap = new Dictionary<int, int>();
            JArray array = JArray.Parse(json);
            Count = array.Count;
            for (int i = 0; i < array.Count; i++)
            {
                JObject dataObject = array[i] as JObject;
                UIConfig data = (UIConfig)dataObject.ToObject(typeof(UIConfig));
                datas.Add(data);
                indexMap.Add(data.ID, i);
            }
        }

        // Static variables to store UIConfig data
        public static int Count;
        private static List<UIConfig> datas;
        private static Dictionary<int, int> indexMap;

        // Get UIConfig data by ID
        public static UIConfig ByID(int id)
        {
            if (id <= 0)
            {
                return Null;
            }
            if (!indexMap.TryGetValue(id, out int index))
            {
                throw new System.Exception($"UIConfig找不到ID:{id}"); // Throw an exception if ID is not found
            }
            return ByIndex(index);
        }

        // Get UIConfig data by index
        public static UIConfig ByIndex(int index)
        {
            return datas[index];
        }

        // Property to check if UIConfig data is null
        public bool IsNull { get; private set; }

        // Static property for Null UIConfig
        public static UIConfig Null { get; } = new UIConfig() { IsNull = true };

        // Properties for storing UI configuration data
        public System.Int32 ID { get; set; }
        public string Description { get; set; }
        public string Asset { get; set; }
        public UIMode Mode { get; set; }
    }
}