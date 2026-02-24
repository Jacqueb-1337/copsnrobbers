.class public Lcom/kamcord/android/KC_F;
.super Lcom/kamcord/android/KC_ab;

# interfaces
.implements Lcom/kamcord/android/CustomWebView$KC_a;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_F$KC_a;
    }
.end annotation


# instance fields
.field private N:Lcom/kamcord/android/KamcordActivity;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/KC_ab;-><init>()V

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_F;)Lcom/kamcord/android/KamcordActivity;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_F;->N:Lcom/kamcord/android/KamcordActivity;

    return-object v0
.end method


# virtual methods
.method public final a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 3

    invoke-super {p0, p1, p2, p3}, Lcom/kamcord/android/KC_ab;->a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/KC_F;->M:Lcom/kamcord/android/CustomWebView;

    new-instance v2, Lcom/kamcord/android/KC_F$KC_a;

    invoke-direct {v2, p0}, Lcom/kamcord/android/KC_F$KC_a;-><init>(Lcom/kamcord/android/KC_F;)V

    invoke-virtual {v1, v2}, Lcom/kamcord/android/CustomWebView;->setWebViewClient(Landroid/webkit/WebViewClient;)V

    iget-object v1, p0, Lcom/kamcord/android/KC_F;->M:Lcom/kamcord/android/CustomWebView;

    invoke-virtual {v1, p0}, Lcom/kamcord/android/CustomWebView;->addPullToRefreshListener(Lcom/kamcord/android/CustomWebView$KC_a;)V

    iget-object v1, p0, Lcom/kamcord/android/KC_F;->M:Lcom/kamcord/android/CustomWebView;

    const v2, 0x3ecccccd    # 0.4f

    invoke-virtual {v1, v2}, Lcom/kamcord/android/CustomWebView;->setRefreshDistanceHeightFraction(F)V

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v1

    if-eqz v1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/KC_F;->M:Lcom/kamcord/android/CustomWebView;

    const-string v2, "profile"

    invoke-static {v1, v2}, Lcom/kamcord/android/KC_d;->a(Lcom/kamcord/android/CustomWebView;Ljava/lang/String;)Z

    :cond_0
    return-object v0
.end method

.method public final a(Landroid/app/Activity;)V
    .locals 0

    invoke-super {p0, p1}, Lcom/kamcord/android/KC_ab;->a(Landroid/app/Activity;)V

    check-cast p1, Lcom/kamcord/android/KamcordActivity;

    iput-object p1, p0, Lcom/kamcord/android/KC_F;->N:Lcom/kamcord/android/KamcordActivity;

    return-void
.end method

.method public final a_()V
    .locals 0

    invoke-virtual {p0}, Lcom/kamcord/android/KC_F;->reload()V

    return-void
.end method

.method public reload()V
    .locals 3

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_0

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_F;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-virtual {v1}, La/a/a/a/KC_f;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    :goto_0
    return-void

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_F;->M:Lcom/kamcord/android/CustomWebView;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/CustomWebView;->setIsRefreshable(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_F;->M:Lcom/kamcord/android/CustomWebView;

    const-string v1, "profile"

    invoke-static {v0, v1}, Lcom/kamcord/android/KC_d;->a(Lcom/kamcord/android/CustomWebView;Ljava/lang/String;)Z

    goto :goto_0
.end method
