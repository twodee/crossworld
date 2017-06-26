using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CellController : MonoBehaviour {
  private Text label;

  void Awake() {
    label = transform.Find("canvas/text").GetComponent<Text>();
  }

  public string Label {
    get {
      return label.text;
    }

    set {
      label.text = value;
    }
  }
}
