namespace CNRMovementMod {
public sealed class JumpBuffer {
 bool pending; int lastFrame=-1;
 public void Press(int frame){if(frame==lastFrame)return;lastFrame=frame;pending=true;}
 public bool Consume(bool grounded){if(!grounded||!pending)return false;pending=false;return true;}
 public void Reset(){pending=false;lastFrame=-1;}
}
}
