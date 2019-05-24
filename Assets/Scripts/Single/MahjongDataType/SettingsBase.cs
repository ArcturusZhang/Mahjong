using System.IO;
using UnityEngine;

namespace Single.MahjongDataType
{
    public class SettingsBase : ScriptableObject
    {
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public void Save(string path)
        {
            var json = ToJson();
            var filepath = Application.persistentDataPath + path;
            var writer = new StreamWriter(filepath);
            writer.WriteLine(json);
            writer.Close();
        }

        public void Load(string path, string defaultValue)
        {
            var filepath = Application.persistentDataPath + path;
            string content;
            try
            {
                using (var reader = new StreamReader(filepath))
                {
                    content = reader.ReadToEnd();
                }
            }
            catch (FileNotFoundException)
            {
                content = defaultValue;
            }
            Debug.Log(content);
            JsonUtility.FromJsonOverwrite(content, this);
        }
    }
}