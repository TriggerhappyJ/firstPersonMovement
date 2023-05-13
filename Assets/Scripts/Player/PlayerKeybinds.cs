using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKeybinds : MonoBehaviour
{
    [Header("Main Movement Keys")] 
    public KeyCode runKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode slideKey = KeyCode.LeftControl;
    public KeyCode jumpKey = KeyCode.Space;
    
    [Header("Wall Run Special Keys")]
    public KeyCode wallUpwardsKey = KeyCode.LeftShift;
    public KeyCode wallDownwardsKey = KeyCode.LeftControl;
}
