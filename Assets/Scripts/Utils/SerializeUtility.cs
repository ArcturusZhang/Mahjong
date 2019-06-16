using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Utils
{
    public static class SerializeUtility
    {
        public static string ToJson<T>(this T entity, bool prettyPrint = true)
        {
            return JsonUtility.ToJson(entity, prettyPrint);
        }

        public static void Save<T>(this T entity, string path, bool prettyPrint = true)
        {
            var json = entity.ToJson(prettyPrint);
            var filepath = Application.persistentDataPath + path;
            SaveContent(filepath, json);
        }

        public static T Load<T>(string path, string defaultValue)
        {
            var filepath = Application.persistentDataPath + path;
            var content = LoadContentOrDefault(filepath, defaultValue);
            return JsonUtility.FromJson<T>(content);
        }

        public static void Load<T>(this T entity, string path, string defaultValue)
        {
            var filepath = Application.persistentDataPath + path;
            var content = LoadContentOrDefault(filepath, defaultValue);
            JsonUtility.FromJsonOverwrite(content, entity);
        }

        public static void SaveContent(string filepath, string content)
        {
            var writer = new StreamWriter(filepath);
            writer.WriteLine(content);
            writer.Close();
        }

        public static string LoadContentOrDefault(string filepath, string defaultValue)
        {
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
            return content;
        }

        public static byte[] SerializeObject(object obj)
        {
            if (obj == null) return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static object DeserializeObject(byte[] bytes)
        {
            if (bytes == null) return null;
            BinaryFormatter binForm = new BinaryFormatter();
            using (var memStream = new MemoryStream())
            {
                memStream.Write(bytes, 0, bytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return (object)binForm.Deserialize(memStream);
            }
        }
    }
}