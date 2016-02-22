using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace GuiSystem {
	public class MusicItemHandler : MonoBehaviour, IPointerClickHandler {
		public MusicList MyMusicList;
		public int ListIndex;
		
		public void OnPointerClick(PointerEventData eventData) {
			MyMusicList.PlaySong (ListIndex);
		}
	}
}
