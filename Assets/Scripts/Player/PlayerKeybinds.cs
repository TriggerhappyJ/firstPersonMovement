using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKeybinds : MonoBehaviour
{
    [Header("Core Movement Keys")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    
    [Header("Movement Action Keys")] 
    public KeyCode runKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode slideKey = KeyCode.LeftControl;
    public KeyCode jumpKey = KeyCode.Space;
    
    [Header("Wall Run Special Keys")]
    public KeyCode wallUpwardsKey = KeyCode.LeftShift;
    public KeyCode wallDownwardsKey = KeyCode.LeftControl;

    [Header("Ability Keys")] 
    public KeyCode speedBoostKey = KeyCode.E;
    public KeyCode dashKey = KeyCode.Q;
    public KeyCode swingKey = KeyCode.Mouse0;

    public KeyCode quitKey = KeyCode.Escape;
}
