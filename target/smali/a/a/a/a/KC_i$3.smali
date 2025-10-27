.class final La/a/a/a/KC_i$3;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements Landroid/view/animation/Animation$AnimationListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = La/a/a/a/KC_i;->a(La/a/a/a/KC_e;IIIZ)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:La/a/a/a/KC_e;

.field private synthetic b:La/a/a/a/KC_i;


# direct methods
.method constructor <init>(La/a/a/a/KC_i;La/a/a/a/KC_e;)V
    .locals 0

    .prologue
    .line 1022
    iput-object p1, p0, La/a/a/a/KC_i$3;->b:La/a/a/a/KC_i;

    iput-object p2, p0, La/a/a/a/KC_i$3;->a:La/a/a/a/KC_e;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onAnimationEnd(Landroid/view/animation/Animation;)V
    .locals 6
    .param p1, "animation"    # Landroid/view/animation/Animation;

    .prologue
    const/4 v3, 0x0

    .line 1025
    iget-object v0, p0, La/a/a/a/KC_i$3;->a:La/a/a/a/KC_e;

    iget-object v0, v0, La/a/a/a/KC_e;->b:Landroid/view/View;

    if-eqz v0, :cond_0

    .line 1026
    iget-object v0, p0, La/a/a/a/KC_i$3;->a:La/a/a/a/KC_e;

    const/4 v1, 0x0

    iput-object v1, v0, La/a/a/a/KC_e;->b:Landroid/view/View;

    .line 1027
    iget-object v0, p0, La/a/a/a/KC_i$3;->b:La/a/a/a/KC_i;

    iget-object v1, p0, La/a/a/a/KC_i$3;->a:La/a/a/a/KC_e;

    iget-object v2, p0, La/a/a/a/KC_i$3;->a:La/a/a/a/KC_e;

    iget v2, v2, La/a/a/a/KC_e;->c:I

    move v4, v3

    move v5, v3

    invoke-virtual/range {v0 .. v5}, La/a/a/a/KC_i;->a(La/a/a/a/KC_e;IIIZ)V

    .line 1030
    :cond_0
    return-void
.end method

.method public final onAnimationRepeat(Landroid/view/animation/Animation;)V
    .locals 0
    .param p1, "animation"    # Landroid/view/animation/Animation;

    .prologue
    .line 1033
    return-void
.end method

.method public final onAnimationStart(Landroid/view/animation/Animation;)V
    .locals 0
    .param p1, "animation"    # Landroid/view/animation/Animation;

    .prologue
    .line 1036
    return-void
.end method
