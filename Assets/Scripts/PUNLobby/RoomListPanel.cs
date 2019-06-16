using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace PUNLobby
{
    public class RoomListPanel : MonoBehaviour
    {
        public GameObject roomEntryPrefab;

        public void SetRoomList(IList<RoomInfo> rooms)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                RoomEntry entry;
                if (i < transform.childCount)
                {
                    var t = transform.GetChild(i);
                    t.gameObject.SetActive(true);
                    entry = t.GetComponent<RoomEntry>();
                }
                else
                {
                    var obj = Instantiate(roomEntryPrefab, transform);
                    entry = obj.GetComponent<RoomEntry>();
                }
                entry.SetRoom(rooms[i]);
            }
            for (int i = rooms.Count; i < transform.childCount; i++)
            {
                var t = transform.GetChild(i);
                t.gameObject.SetActive(false);
            }
        }
    }
}
