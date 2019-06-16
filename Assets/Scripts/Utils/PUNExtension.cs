using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Utils
{
    public static class PUNExtension
    {
        public static T GetCustomPropertyOrDefault<T>(this Player player, string key, T defaultValue)
        {
            var properties = player.CustomProperties;
            if (!properties.ContainsKey(key)) return defaultValue;
            return (T)properties[key];
        }

        public static void SetCustomProperty(this Player player, string key, object value)
        {
            var properties = player.CustomProperties;
            var table = new Hashtable();
            table.Add(key, value);
            player.SetCustomProperties(table);
        }
    }
}