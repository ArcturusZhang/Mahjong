using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace PUNLobby
{
    public class RoomListPanel : MonoBehaviour
    {
        public RectTransform contentParent;
        public GameObject roomEntryPrefab;
        private const float height = 60;

        public void SetRoomList(IList<RoomInfo> rooms)
        {
            var size = contentParent.sizeDelta;
            contentParent.sizeDelta = new Vector2(size.x, rooms.Count * height);
            for (int i = 0; i < rooms.Count; i++)
            {
                RoomEntry entry;
                if (i < contentParent.childCount)
                {
                    var t = contentParent.GetChild(i);
                    t.gameObject.SetActive(true);
                    entry = t.GetComponent<RoomEntry>();
                }
                else
                {
                    var obj = Instantiate(roomEntryPrefab, contentParent);
                    entry = obj.GetComponent<RoomEntry>();
                }
                entry.SetRoom(rooms[i]);
            }
            for (int i = rooms.Count; i < contentParent.childCount; i++)
            {
                var t = contentParent.GetChild(i);
                t.gameObject.SetActive(false);
            }
        }
    }
}
