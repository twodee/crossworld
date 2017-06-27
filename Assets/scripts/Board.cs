using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Board {
  public Vector2 positionP;
  public Vector2 positionV;
  public string[] cells;
  public Clue[] clues;

  public Clue getClue(int x, int y, bool isVertical) {
    foreach (Clue clue in clues) {
      if (clue.x == x && clue.y == y && clue.IsVertical == isVertical) {
        return clue;
      }
    }
    return null;
  }

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
