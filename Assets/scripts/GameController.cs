using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameController : MonoBehaviour {
  public TextAsset boardFile;
  public GameObject letter;
  public GameObject hole;
  public AmpersandController ampersand;
  public StarController star;

  public static GameController SINGLETON;

  public const float GAP = 0.02f;
  private const float DEPTH = 1.0f;

  private int nPlayersThrough;
  public CellController[,] cells;

  void Start() {
    nPlayersThrough = 0;
    SINGLETON = this;

    Transform boardParent = GameObject.Find("/board").transform;

    Board board = JsonUtility.FromJson<Board>(boardFile.text);
    List<string> horizontalClues = new List<string>();
    List<string> verticalClues = new List<string>();

    cells = new CellController[board.ColumnCount, board.RowCount];

    int iClue = 0;
    for (int r = 0; r < board.RowCount; ++r) {
      for (int c = 0; c < board.ColumnCount; ++c) {
        GameObject reference = board[c, r] == '.' ? hole : letter;
        GameObject instance = Instantiate(reference, new Vector3(c * (1 + GAP), (board.RowCount - r) * (1 + GAP), DEPTH), Quaternion.identity);
        instance.transform.SetParent(boardParent);
        instance.name = "cell_" + c + "_" + r;

        if (board[c, r] != '.') {
          CellController cell = instance.GetComponent<CellController>();  
          cells[c, r] = cell;
          cell.Row = r;
          cell.Column = c;
          if (System.Char.IsUpper(board[c, r])) {
            cell.Label = "" + board[c, r];
          } else {
            cell.Label = "";
          }

          bool isVerticalStart = (r == 0 || board[c, r - 1] == '.') && r + 1 < board.RowCount && board[c, r + 1] != '.';
          bool isHorizontalStart = (c == 0 || board[c - 1, r] == '.') && c + 1 < board.ColumnCount && board[c + 1, r] != '.';

          GameObject address = instance.transform.Find("canvas/address").gameObject;
          if (isVerticalStart || isHorizontalStart) {
            address.GetComponent<Text>().text = "" + iClue;
            instance.layer = Utilities.BLANK_HEAD_LAYER;

            if (isVerticalStart) {
              Clue clue = board.getClue(c, r, true);
              verticalClues.Add(iClue + ". " + clue.text);
            }

            if (isHorizontalStart) {
              Clue clue = board.getClue(c, r, false);
              horizontalClues.Add(iClue + ". " + clue.text);
            }

            ++iClue;
          } else {
            address.SetActive(false);
            instance.layer = Utilities.BLANK_TAIL_LAYER;
          }
        }
      }

      Text clueBox = GameObject.Find("canvas/cluescroller/viewport/content/text").GetComponent<Text>();
      clueBox.text = "Horizontal\n" + String.Join("\n", horizontalClues.ToArray()) + "\n\n" + "Vertical\n" + String.Join("\n", verticalClues.ToArray());

      Text codeBox = GameObject.Find("canvas/codescroller/viewport/content/text").GetComponent<Text>();
      codeBox.text = "";
    }

    for (int r = 0; r < board.RowCount; ++r) {
      GameObject instance = Instantiate(hole, new Vector3(-1 * (1 + GAP), (board.RowCount - r) * (1 + GAP), DEPTH), Quaternion.identity);
      instance.transform.SetParent(boardParent);
      instance.name = "edge_left_" + r;
      instance = Instantiate(hole, new Vector3(board.ColumnCount * (1 + GAP), (board.RowCount - r) * (1 + GAP), DEPTH), Quaternion.identity);
      instance.transform.SetParent(boardParent);
      instance.name = "edge_right_" + r;
    }

    for (int c = 0; c < board.ColumnCount; ++c) {
      GameObject instance = Instantiate(hole, new Vector3(c * (1 + GAP), 0, DEPTH), Quaternion.identity);
      instance.transform.SetParent(boardParent);
      instance.name = "edge_bottom_" + c;

      instance = Instantiate(hole, new Vector3(c * (1 + GAP), (board.RowCount + 1) * (1 + GAP), DEPTH), Quaternion.identity);
      instance.transform.SetParent(boardParent);
      instance.name = "edge_top_" + c;
      instance.GetComponent<BoxCollider2D>().isTrigger = true;
      instance.tag = "topEdge";
    }

    ampersand = GameObject.Find("/players/ampersand").GetComponent<AmpersandController>();
    star = GameObject.Find("/players/star").GetComponent<StarController>();

    ampersand.gameObject.transform.position = board.positionP;
    star.gameObject.transform.position = board.positionV;

    transform.position = new Vector3(board.ColumnCount / 2, (board.RowCount + 1) / 2, transform.position.z);
  }

  public void Log(string message) {
    Text codeBox = GameObject.Find("canvas/codescroller/viewport/content/text").GetComponent<Text>();
    codeBox.text += "\n" + message;
    StartCoroutine(ScrollToBottom());
  }

  IEnumerator ScrollToBottom() {
    yield return new WaitForSeconds(0.2f);
    ScrollRect scroller = GameObject.Find("canvas/codescroller").GetComponent<ScrollRect>();
    scroller.verticalNormalizedPosition = 0.0f;
  }

  public void Through(string declaration) {
    Log(declaration);
    ++nPlayersThrough;
    if (nPlayersThrough == 2) {
      GameObject[] tops = GameObject.FindGameObjectsWithTag("topEdge");
      foreach (GameObject top in tops) {
        top.GetComponent<BoxCollider2D>().isTrigger = false;
      }
    }
  }
}
