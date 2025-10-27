.class final Lcom/kamcord/android/KC_w$KC_a;
.super Landroid/view/animation/AlphaAnimation;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_w;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_a"
.end annotation


# direct methods
.method public constructor <init>(Lcom/kamcord/android/KC_w;FF)V
    .locals 2

    invoke-direct {p0, p2, p3}, Landroid/view/animation/AlphaAnimation;-><init>(FF)V

    const-wide/16 v0, 0xfa

    invoke-virtual {p0, v0, v1}, Lcom/kamcord/android/KC_w$KC_a;->setDuration(J)V

    new-instance v0, Landroid/view/animation/DecelerateInterpolator;

    invoke-direct {v0}, Landroid/view/animation/DecelerateInterpolator;-><init>()V

    invoke-virtual {p0, v0}, Lcom/kamcord/android/KC_w$KC_a;->setInterpolator(Landroid/view/animation/Interpolator;)V

    return-void
.end method
