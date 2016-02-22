using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DialogueSystem;

public static class SpeechUtilities {
	public static string RemoveCommand(string MyString) {
		MyString = RemoveWhiteSpace (MyString);
		int NonCommandIndex = 0;
		for (int i = 0; i < MyString.Length; i++) {
			if (MyString[i] == ' ') {
				NonCommandIndex = i+1;
				i = MyString.Length;
			}
		}
		MyString = MyString.Remove(0, NonCommandIndex);

		// remove white space from end
		MyString = RemoveWhiteSpaceFromEnd (MyString);

		//MyString = SpeechFileReader.CheckStringForLastChar (MyString);
		return MyString;
	}
	// if first non character is a slash, it is a command
	public static string GetCommand(string MyString) {
		MyString.ToLower();
		int MyCommandStartIndex = -1;
		for (int i = 0; i < MyString.Length; i++) {
			if (MyString[i] == '/') {
				MyCommandStartIndex = i;
			} else if (MyString[i] == ' ') {
				if (MyCommandStartIndex != -1) {
					return MyString.Substring(MyCommandStartIndex, i-MyCommandStartIndex);
				}
			}
		}
		if (MyCommandStartIndex != -1) {
			return MyString.Substring(MyCommandStartIndex,  MyString.Length-MyCommandStartIndex);
		}
		return "No Command";
	}
	public static bool IsAllLetters(string NewInput) {
		NewInput = SpeechUtilities.RemoveWhiteSpace (NewInput);
		for (int i = 0; i < NewInput.Length; i++) {
			if (!System.Char.IsLetter(NewInput[i]))
				return false;
		}
		return true;
	}
	public static bool IsNumbersInput(string NewInput) {
		NewInput = SpeechUtilities.RemoveWhiteSpace (NewInput);
		for (int i = 0; i < NewInput.Length; i++) {
			if (!System.Char.IsNumber(NewInput[i]))
				return false;
		}
		return true;
	}
	// if first non character is a slash, it is a command
	public static bool IsCommand(string MyString) {
		MyString.ToLower();
		for (int i = 0; i < MyString.Length; i++) {
			if (System.Char.IsLetter(MyString[i])) {
				return false;
			} else if (MyString[i] == '/') {
				return true;
			}
		}
		return false;
	}
	public static bool IsNonWhiteSpace(char CheckChar) {
		return (System.Char.IsLetter (CheckChar) || System.Char.IsNumber (CheckChar) 
		        || CheckChar == '.' || CheckChar == ',' || CheckChar == '/' || CheckChar == '*');
	}
	public static string RemoveWhiteSpaceFromEnd(string MyString) {
		for (int i = MyString.Length-1; i >= 0; i--) 
		{
			if (!IsNonWhiteSpace(MyString[i]))
			{
				MyString = MyString.Remove (i, 1);
			} else {
				i = -1;
			}
		}
		return MyString;
	}
	public static string RemoveWhiteSpace(string MyString) {
		if (MyString.Length == 0)
			return MyString;
		int NonWhiteSpaceIndex = 0;
		for (int i = 0; i < MyString.Length; i++) 
		{
			if (IsNonWhiteSpace(MyString[i])) 
			{
				NonWhiteSpaceIndex = i;
				i = MyString.Length;
			}
		}
		MyString = MyString.Remove(0, NonWhiteSpaceIndex);
		MyString = RemoveWhiteSpaceFromEnd (MyString);
		return MyString;
	}
	public static string RemoveNonCharacters(string MyString) {
		for (int i = MyString.Length-1; i>= 0; i--) {
			if (!System.Char.IsLetter(MyString[i])) {
				MyString.Remove(i, 1);
			}
		}
		//MyString = MyString.Remove(0, NonWhiteSpaceIndex);
		return MyString;
	}

	public static List<int> GetInts(string MyIntsString) {
		string[] MyInts = MyIntsString.Split(' ', ',');
		List<int> NewInts = new List<int> ();

		if (MyInts != null)
		for (int j = 0; j < MyInts.Length; j++) {
			if (MyInts[j].Length > 0) {
				try {
					if (System.Char.IsNumber(MyInts[j][0])) {
						int IsInt = int.Parse(MyInts[j]);
						NewInts.Add(IsInt);
					}
				} catch(System.FormatException e) {
					
				}
			}
		}
		return NewInts;
	}

	
	public static string CheckStringForLastChar(string MyString) {
			if (string.IsNullOrEmpty (MyString))
				return "";
			if (MyString.Length > 0) {
				int LastChar = (int)MyString [MyString.Length - 1];
				if (LastChar == 13 || LastChar == 32) {
					MyString = MyString.Remove (MyString.Length - 1);
				}
			}
			return MyString;
	}
}
