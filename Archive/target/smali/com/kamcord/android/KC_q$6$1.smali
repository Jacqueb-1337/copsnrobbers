.class final Lcom/kamcord/android/KC_q$6$1;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/animation/Animator$AnimatorListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_q$6;->run()V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_q$6;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_q$6;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_q$6$1;->a:Lcom/kamcord/android/KC_q$6;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onAnimationCancel(Landroid/animation/Animator;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_q$6$1;->a:Lcom/kamcord/android/KC_q$6;

    iget-object v0, v0, Lcom/kamcord/android/KC_q$6;->a:Lcom/kamcord/android/KC_q;

    invoke-static {v0}, Lcom/kamcord/android/KC_q;->d(Lcom/kamcord/android/KC_q;)V

    return-void
.end method

.method public final onAnimationEnd(Landroid/animation/Animator;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_q$6$1;->a:Lcom/kamcord/android/KC_q$6;

    iget-object v0, v0, Lcom/kamcord/android/KC_q$6;->a:Lcom/kamcord/android/KC_q;

    invoke-static {v0}, Lcom/kamcord/android/KC_q;->d(Lcom/kamcord/android/KC_q;)V

    return-void
.end method

.method public final onAnimationRepeat(Landroid/animation/Animator;)V
    .locals 0

    return-void
.end method

.method public final onAnimationStart(Landroid/animation/Animator;)V
    .locals 0

    return-void
.end method
