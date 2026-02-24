.class final Lcom/kamcord/android/KC_q$2;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_q;->a(F)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:F

.field private synthetic b:Lcom/kamcord/android/KC_q;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_q;F)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_q$2;->b:Lcom/kamcord/android/KC_q;

    iput p2, p0, Lcom/kamcord/android/KC_q$2;->a:F

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 3

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_q$2;->b:Lcom/kamcord/android/KC_q;

    invoke-static {v0}, Lcom/kamcord/android/KC_q;->a(Lcom/kamcord/android/KC_q;)Landroid/widget/RelativeLayout;

    move-result-object v0

    const-string v1, "id"

    const-string v2, "tabbar_progress"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/RelativeLayout;->findViewById(I)Landroid/view/View;

    move-result-object v0

    const/high16 v1, 0x3f800000    # 1.0f

    invoke-virtual {v0, v1}, Landroid/view/View;->setAlpha(F)V

    iget-object v0, p0, Lcom/kamcord/android/KC_q$2;->b:Lcom/kamcord/android/KC_q;

    iget v1, p0, Lcom/kamcord/android/KC_q$2;->a:F

    invoke-static {v0, v1}, Lcom/kamcord/android/KC_q;->a(Lcom/kamcord/android/KC_q;F)V

    iget-object v0, p0, Lcom/kamcord/android/KC_q$2;->b:Lcom/kamcord/android/KC_q;

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
