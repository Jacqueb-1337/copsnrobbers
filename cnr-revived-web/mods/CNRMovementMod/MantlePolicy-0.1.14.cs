using System;
namespace CNRMovementMod {
// Geometry policy only. The Unity adapter must supply measured clearance results.
// No collision bypass, teleport or live gameplay integration.
public static class MantlePolicy {
 public enum Kind { None, ClimbUp, Vault }
 public struct Candidate {
  public float Height, Width, ForwardOffset;
  public bool ApproachClear, TopClear, CeilingClear, DestinationClear, LandingClear, PathClear;
  public bool VaultEligible; // caller must classify height band; reference bins unresolved.
 }
 public const float MaximumReferenceHeight=2.3f, MaximumReferenceGap=.45f;
 public static float MaximumRise(float unitScale,float standingHeight,bool airborne=false){return Math.Min((airborne?MaximumReferenceHeight:2f)*unitScale,standingHeight*(airborne?1.65f:1.35f));}
 public static bool WithinReach(float gap,float rise,float unitScale,float standingHeight,bool airborne=false){return unitScale>0&&standingHeight>0&&gap>=0&&gap<=MaximumReferenceGap*unitScale&&rise>0&&rise<=MaximumRise(unitScale,standingHeight,airborne);}
 public static Kind Choose(Candidate c,float unitScale){
  if(unitScale<=0||c.Height<=0||c.Height>MaximumReferenceHeight*unitScale||c.Width<.4f*unitScale||
     !c.ApproachClear||!c.TopClear||!c.CeilingClear||!c.DestinationClear||!c.PathClear)return Kind.None;
  if(c.VaultEligible&&c.LandingClear&&c.ForwardOffset>=0&&c.ForwardOffset<=1.2f*unitScale)return Kind.Vault;
  return c.Width>=.5f*unitScale?Kind.ClimbUp:Kind.None;
 }
}
}

