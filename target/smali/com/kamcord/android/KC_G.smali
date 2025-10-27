.class public final Lcom/kamcord/android/KC_G;
.super Lcom/kamcord/android/KC_ab;

# interfaces
.implements Lcom/kamcord/android/CustomWebView$KC_a;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_G$KC_a;
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

.method static synthetic a(Lcom/kamcord/android/KC_G;)Lcom/kamcord/android/KamcordActivity;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_G;->N:Lcom/kamcord/android/KamcordActivity;

    return-object v0
.end method


# virtual methods
.method public final a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 6

    invoke-super {p0, p1, p2, p3}, Lcom/kamcord/android/KC_ab;->a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;

    move-result-object v0

    invoke-virtual {p0}, Lcom/kamcord/android/KC_G;->g()Landroid/os/Bundle;

    move-result-object v1

    const-string v2, "apiKey"

    invoke-virtual {v1, v2}, Landroid/os/Bundle;->containsKey(Ljava/lang/String;)Z

    move-result v2

    if-eqz v2, :cond_0

    const-string v2, "targetUser"

    invoke-virtual {v1, v2}, Landroid/os/Bundle;->containsKey(Ljava/lang/String;)Z

    move-result v2

    if-eqz v2, :cond_0

    const-string v2, "apiKey"

    invoke-virtual {v1, v2}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    const-string v3, "targetUser"

    invoke-virtual {v1, v3}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    new-instance v3, Ljava/util/ArrayList;

    invoke-direct {v3}, Ljava/util/ArrayList;-><init>()V

    new-instance v4, Lorg/apache/http/message/BasicNameValuePair;

    const-string v5, "target_user"

    invoke-direct {v4, v5, v1}, Lorg/apache/http/message/BasicNameValuePair;-><init>(Ljava/lang/String;Ljava/lang/String;)V

    invoke-virtual {v3, v4}, Ljava/util/ArrayList;->add(Ljava/lang/Object;)Z

    iget-object v1, p0, Lcom/kamcord/android/KC_G;->M:Lcom/kamcord/android/CustomWebView;

    new-instance v4, Lcom/kamcord/android/KC_G$KC_a;

    invoke-direct {v4, p0}, Lcom/kamcord/android/KC_G$KC_a;-><init>(Lcom/kamcord/android/KC_G;)V

    invoke-virtual {v1, v4}, Lcom/kamcord/android/CustomWebView;->setWebViewClient(Landroid/webkit/WebViewClient;)V

    iget-object v1, p0, Lcom/kamcord/android/KC_G;->M:Lcom/kamcord/android/CustomWebView;

    invoke-virtual {v1, p0}, Lcom/kamcord/android/CustomWebView;->addPullToRefreshListener(Lcom/kamcord/android/CustomWebView$KC_a;)V

    iget-object v1, p0, Lcom/kamcord/android/KC_G;->M:Lcom/kamcord/android/CustomWebView;

    const v4, 0x3ecccccd    # 0.4f

    invoke-virtual {v1, v4}, Lcom/kamcord/android/CustomWebView;->setRefreshDistanceHeightFraction(F)V

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v1

    if-eqz v1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/KC_G;->M:Lcom/kamcord/android/CustomWebView;

    invoke-static {v1, v2, v3}, Lcom/kamcord/android/KC_d;->a(Lcom/kamcord/android/CustomWebView;Ljava/lang/String;Ljava/util/List;)Z

    :cond_0
    return-object v0
.end method

.method public final a(Landroid/app/Activity;)V
    .locals 0

    invoke-super {p0, p1}, Lcom/kamcord/android/KC_ab;->a(Landroid/app/Activity;)V

    check-cast p1, Lcom/kamcord/android/KamcordActivity;

    iput-object p1, p0, Lcom/kamcord/android/KC_G;->N:Lcom/kamcord/android/KamcordActivity;

    return-void
.end method

.method public final a_()V
    .locals 0

    invoke-virtual {p0}, Lcom/kamcord/android/KC_G;->reload()V

    return-void
.end method

.method public final reload()V
    .locals 3

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_0

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    iget-object v1, p0, Lcom/kamcord/android/KC_G;->N:Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v1}, Lcom/kamcord/android/KamcordActivity;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    :goto_0
    return-void

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_G;->M:Lcom/kamcord/android/CustomWebView;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/CustomWebView;->setIsRefreshable(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_G;->M:Lcom/kamcord/android/CustomWebView;

    const-string v1, "profile"

    invoke-static {v0, v1}, Lcom/kamcord/android/KC_d;->a(Lcom/kamcord/android/CustomWebView;Ljava/lang/String;)Z

    goto :goto_0
.end method
