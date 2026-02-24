.class final Lcom/kamcord/android/KC_q$6;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_q;->a_(Ljava/lang/String;)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field final synthetic a:Lcom/kamcord/android/KC_q;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_q;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_q$6;->a:Lcom/kamcord/android/KC_q;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 4

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_q$6;->a:Lcom/kamcord/android/KC_q;

    const/high16 v1, 0x3f800000    # 1.0f

    invoke-static {v0, v1}, Lcom/kamcord/android/KC_q;->a(Lcom/kamcord/android/KC_q;F)V

    iget-object v0, p0, Lcom/kamcord/android/KC_q$6;->a:Lcom/kamcord/android/KC_q;

    invoke-static {v0}, Lcom/kamcord/android/KC_q;->b(Lcom/kamcord/android/KC_q;)Landroid/animation/ValueAnimator;

    move-result-object v0

    const-wide/16 v2, 0xc8

    invoke-virtual {v0, v2, v3}, Landroid/animation/ValueAnimator;->setDuration(J)Landroid/animation/ValueAnimator;

    iget-object v0, p0, Lcom/kamcord/android/KC_q$6;->a:Lcom/kamcord/android/KC_q;

    invoke-static {v0}, Lcom/kamcord/android/KC_q;->b(Lcom/kamcord/android/KC_q;)Landroid/animation/ValueAnimator;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/KC_q$6$1;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_q$6$1;-><init>(Lcom/kamcord/android/KC_q$6;)V

    invoke-virtual {v0, v1}, Landroid/animation/ValueAnimator;->addListener(Landroid/animation/Animator$AnimatorListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_q$6;->a:Lcom/kamcord/android/KC_q;

    invoke-static {v0}, Lcom/kamcord/android/KC_q;->b(Lcom/kamcord/android/KC_q;)Landroid/animation/ValueAnimator;

    move-result-object v0

    invoke-virtual {v0}, Landroid/animation/ValueAnimator;->start()V
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    return-void

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    goto :goto_0
.end method
