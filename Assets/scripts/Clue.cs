﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Clue {
  public int x;
  public int y;
  public string orientation; 
  public string text;
  public int serial;

  public bool IsVertical {
    get {
      return orientation == "|";
    }
  }
}
