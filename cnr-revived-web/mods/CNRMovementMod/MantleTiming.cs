using System;
namespace CNRMovementMod {
 // Data from archived CODM Pawn and ClimbComponent initializers.
 // Exact height comparisons are unresolved; ascending upper bounds are an adaptation.
 public static class MantleTiming {
  static readonly float[] upper={.75f,1.05f,1.55f,1.8f,10f};
  static readonly float[] climb={.6f,.8f,1f,1.2f,1.4f};
  // Standard vault timing is runtime-configured or initialized outside the recovered ctor.
  // Use the verified ADV vault variant as an explicit fallback, not a standard timing claim.
  static readonly float[] vault={.6f,.8f,.8f,.8f,.8f};
  public static int HeightIndex(float height){
   if(float.IsNaN(height)||height<=0||height>2.3f)return -1;
   for(int i=0;i<upper.Length;i++)if(height<=upper[i])return i;
   return -1;
  }
  public static bool VaultHeight(float height){int i=HeightIndex(height);return i>=0&&i<=1;}
  public static float Duration(MantlePolicy.Kind kind,float height){
   int i=HeightIndex(height);if(i<0||kind==MantlePolicy.Kind.None)return 0;
   return kind==MantlePolicy.Kind.Vault?vault[i]:climb[i];
  }
 }
}
