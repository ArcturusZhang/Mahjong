using System.IO;
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
            var writer = new StreamWriter(filepath);
            writer.WriteLine(json);
            writer.Close();
        }

        public static T Load<T>(string path, string defaultValue) {
            var filepath = Application.persistentDataPath + path;
            string content = LoadContentOrDefault(filepath, defaultValue);
            return JsonUtility.FromJson<T>(content);
        }

        public static void Load<T>(this T entity, string path, string defaultValue)
        {
            var filepath = Application.persistentDataPath + path;
            string content = LoadContentOrDefault(filepath, defaultValue);
            JsonUtility.FromJsonOverwrite(content, entity);
        }

        private static string LoadContentOrDefault(string path, string defaultValue) {
            string content;
            try
            {
                using (var reader = new StreamReader(path))
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
    }
}