using UnityEngine;
using System.Collections;

public class Utilities {
  public static int GROUND_LAYER = LayerMask.NameToLayer("ground");
  public static int GROUND_MASK = 1 << GROUND_LAYER;
  public static int PLAYER_R_LAYER = LayerMask.NameToLayer("playerR");
  public static int PLAYER_R_MASK = 1 << PLAYER_R_LAYER;
  public static int PLAYER_D_LAYER = LayerMask.NameToLayer("playerD");
  public static int PLAYER_D_MASK = 1 << PLAYER_D_LAYER;
  public static int BLANK_HEAD_LAYER = LayerMask.NameToLayer("blankHead");
  public static int BLANK_HEAD_MASK = 1 << BLANK_HEAD_LAYER;
  public static int BLANK_PAUSED_LAYER = LayerMask.NameToLayer("blankPaused");
  public static int BLANK_PAUSED_MASK = 1 << BLANK_PAUSED_LAYER;
  public static int BLANK_TAIL_LAYER = LayerMask.NameToLayer("blankTail");
  public static int BLANK_TAIL_MASK = 1 << BLANK_TAIL_LAYER;
}
