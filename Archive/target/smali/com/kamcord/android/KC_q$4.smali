.class final Lcom/kamcord/android/KC_q$4;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_q;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KamcordActivity;

.field private synthetic b:F

.field private synthetic c:Lcom/kamcord/android/KC_q;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_q;Lcom/kamcord/android/KamcordActivity;F)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_q$4;->c:Lcom/kamcord/android/KC_q;

    iput-object p2, p0, Lcom/kamcord/android/KC_q$4;->a:Lcom/kamcord/android/KamcordActivity;

    iput p3, p0, Lcom/kamcord/android/KC_q$4;->b:F

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 5

    const/16 v4, 0x8

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_q$4;->c:Lcom/kamcord/android/KC_q;

    invoke-static {v0}, Lcom/kamcord/android/KC_q;->a(Lcom/kamcord/android/KC_q;)Landroid/widget/RelativeLayout;

    move-result-object v0

    const-string v1, "id"

    const-string v2, "tabbar_progress"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/RelativeLayout;->findViewById(I)Landroid/view/View;

    move-result-object v0

    invoke-virtual {v0}, Landroid/view/View;->getLayoutParams()Landroid/view/ViewGroup$LayoutParams;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/android/KC_q$4;->a:Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v2}, Lcom/kamcord/android/KamcordActivity;->getRequestedOrientation()I

    move-result v2

    const/4 v3, 0x1

    if-eq v2, v3, :cond_0

    iget-object v2, p0, Lcom/kamcord/android/KC_q$4;->a:Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v2}, Lcom/kamcord/android/KamcordActivity;->getRequestedOrientation()I

    move-result v2

    const/16 v3, 0x9

    if-eq v2, v3, :cond_0

    iget-object v2, p0, Lcom/kamcord/android/KC_q$4;->a:Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v2}, Lcom/kamcord/android/KamcordActivity;->getResources()Landroid/content/res/Resources;

    move-result-object v2

    invoke-virtual {v2}, Landroid/content/res/Resources;->getConfiguration()Landroid/content/res/Configuration;

    move-result-object v2

    iget v2, v2, Landroid/content/res/Configuration;->smallestScreenWidthDp:I

    const/16 v3, 0x258

    if-lt v2, v3, :cond_1

    :cond_0
    const/4 v2, 0x0

    invoke-virtual {v0, v2}, Landroid/view/View;->setVisibility(I)V

    iget-object v2, p0, Lcom/kamcord/android/KC_q$4;->c:Lcom/kamcord/android/KC_q;

    invoke-static {v2}, Lcom/kamcord/android/KC_q;->a(Lcom/kamcord/android/KC_q;)Landroid/widget/RelativeLayout;

    move-result-object v2

    invoke-virtual {v2}, Landroid/widget/RelativeLayout;->getWidth()I

    move-result v2

    int-to-float v2, v2

    iget v3, p0, Lcom/kamcord/android/KC_q$4;->b:F

    mul-float/2addr v2, v3

    float-to-int v2, v2

    iput v2, v1, Landroid/view/ViewGroup$LayoutParams;->width:I

    :goto_0
    invoke-virtual {v0, v1}, Landroid/view/View;->setLayoutParams(Landroid/view/ViewGroup$LayoutParams;)V

    :goto_1
    return-void

    :cond_1
    iget-object v2, p0, Lcom/kamcord/android/KC_q$4;->a:Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v2}, Lcom/kamcord/android/KamcordActivity;->getRequestedOrientation()I

    move-result v2

    if-eqz v2, :cond_2

    iget-object v2, p0, Lcom/kamcord/android/KC_q$4;->a:Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v2}, Lcom/kamcord/android/KamcordActivity;->getRequestedOrientation()I

    move-result v2

    if-ne v2, v4, :cond_3

    :cond_2
    const/4 v2, 0x0

    invoke-virtual {v0, v2}, Landroid/view/View;->setVisibility(I)V

    iget-object v2, p0, Lcom/kamcord/android/KC_q$4;->c:Lcom/kamcord/android/KC_q;

    invoke-static {v2}, Lcom/kamcord/android/KC_q;->a(Lcom/kamcord/android/KC_q;)Landroid/widget/RelativeLayout;

    move-result-object v2

    invoke-virtual {v2}, Landroid/widget/RelativeLayout;->getHeight()I

    move-result v2

    int-to-float v2, v2

    iget v3, p0, Lcom/kamcord/android/KC_q$4;->b:F

    mul-float/2addr v2, v3

    float-to-int v2, v2

    iput v2, v1, Landroid/view/ViewGroup$LayoutParams;->height:I
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    goto :goto_1

    :cond_3
    const/16 v2, 0x8

    :try_start_1
    invoke-virtual {v0, v2}, Landroid/view/View;->setVisibility(I)V
    :try_end_1
    .catch Ljava/lang/Throwable; {:try_start_1 .. :try_end_1} :catch_0

    goto :goto_0
.end method
