using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public enum IconType {
	None,	// place holder for empty icons
	Spell,	// performs a spell
	Item,	// Use it to chop wood etc
	Gear,	// adds stat increases
	Macro	// contains a script that performs a game function
};

// I should remove this, put IconType in ItemGui
// Put an index to ItemGui inside the spells/items etc
// mainly GUI data
[System.Serializable]
public class Icon {
	//public Texture Texture;
	public int Index;
	public IconType MyIconType;
	public bool NeedsUpdating = false;	// needs to update the texture
	
	public Icon() {
		MyIconType = IconType.None;
		Index = 0;
		NeedsUpdating = true;
	}
	
	public Icon(IconType NewType) {
		MyIconType = NewType;
		Index = 0;
		NeedsUpdating = true;
	}
	public Icon(Icon NewIcon) {
		Index = NewIcon.Index;
		MyIconType = NewIcon.MyIconType;
		NeedsUpdating = true;
	}

	public void Clear() {
		Index = 0;
		MyIconType = IconType.None;
		NeedsUpdating = true;
	}
};

