using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Board {
  public Vector2 positionP;
  public Vector2 positionV;
  public string[] cells;
  public Clue[] clues;

  public int RowCount {
    get {
      return cells.Length;
    }
  }

  public int ColumnCount {
    get {
      return cells[0].Length;
    }
  }

  public char this[int c, int r] {
    get {
      return cells[r][c];
    }
  }
}
