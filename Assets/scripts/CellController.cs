using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CellController : MonoBehaviour {
  private Text label;
  private Text address;
  private int c;
  private int r;

  void Awake() {
    label = transform.Find("canvas/text").GetComponent<Text>();
    address = transform.Find("canvas/address").GetComponent<Text>();
  }

  public int Column {
    get {
      return c;
    }
    set {
      c = value;
    }
  }

  public int Row {
    get {
      return r;
    }
    set {
      r = value;
    }
  }

  void OnMouseDown() {
    GameController.SINGLETON.ampersand.OnClick();
  }

  public string Label {
    get {
      return label.text;
    }

    set {
      label.text = value;
    }
  }

  public string Address {
    get {
      return address.text;
    }
  }
}
