.class public Lcom/kamcord/android/KC_ab;
.super La/a/a/a/KC_e;


# annotations
.annotation build Landroid/annotation/SuppressLint;
    value = {
        "ValidFragment"
    }
.end annotation

.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_ab$KC_a;
    }
.end annotation


# instance fields
.field M:Lcom/kamcord/android/CustomWebView;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, La/a/a/a/KC_e;-><init>()V

    return-void
.end method


# virtual methods
.method public a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 4
    .annotation build Landroid/annotation/SuppressLint;
        value = {
            "SetJavaScriptEnabled"
        }
    .end annotation

    const-string v0, "layout"

    const-string v1, "z_kamcord_fragment_web"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    const/4 v1, 0x0

    invoke-virtual {p1, v0, p2, v1}, Landroid/view/LayoutInflater;->inflate(ILandroid/view/ViewGroup;Z)Landroid/view/View;

    move-result-object v1

    const-string v0, "id"

    const-string v2, "webView"

    invoke-static {v0, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {v1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/CustomWebView;

    iput-object v0, p0, Lcom/kamcord/android/KC_ab;->M:Lcom/kamcord/android/CustomWebView;

    iget-object v0, p0, Lcom/kamcord/android/KC_ab;->M:Lcom/kamcord/android/CustomWebView;

    invoke-virtual {v0}, Lcom/kamcord/android/CustomWebView;->getSettings()Landroid/webkit/WebSettings;

    move-result-object v0

    const/4 v2, 0x1

    invoke-virtual {v0, v2}, Landroid/webkit/WebSettings;->setJavaScriptEnabled(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_ab;->M:Lcom/kamcord/android/CustomWebView;

    new-instance v2, Lcom/kamcord/android/KC_ab$KC_a;

    invoke-direct {v2}, Lcom/kamcord/android/KC_ab$KC_a;-><init>()V

    invoke-virtual {v0, v2}, Lcom/kamcord/android/CustomWebView;->setWebViewClient(Landroid/webkit/WebViewClient;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_ab;->M:Lcom/kamcord/android/CustomWebView;

    const-string v2, "kamcordActivityBackground"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getColor(Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v0, v2}, Lcom/kamcord/android/CustomWebView;->setBackgroundColor(I)V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_ab;->g()Landroid/os/Bundle;

    move-result-object v0

    if-eqz v0, :cond_0

    const-string v2, "url"

    invoke-virtual {v0, v2}, Landroid/os/Bundle;->containsKey(Ljava/lang/String;)Z

    move-result v2

    if-eqz v2, :cond_0

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v2

    if-nez v2, :cond_1

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_ab;->h()La/a/a/a/KC_f;

    move-result-object v2

    invoke-virtual {v2}, La/a/a/a/KC_f;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v2

    const/4 v3, 0x0

    invoke-virtual {v0, v2, v3}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    :cond_0
    :goto_0
    return-object v1

    :cond_1
    iget-object v2, p0, Lcom/kamcord/android/KC_ab;->M:Lcom/kamcord/android/CustomWebView;

    invoke-virtual {v2}, Lcom/kamcord/android/CustomWebView;->getWebView()Landroid/webkit/WebView;

    move-result-object v2

    invoke-static {v2}, La/a/a/c/KC_a;->a(Landroid/view/ViewGroup;)V

    iget-object v2, p0, Lcom/kamcord/android/KC_ab;->M:Lcom/kamcord/android/CustomWebView;

    const-string v3, "url"

    invoke-virtual {v0, v3}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v2, v0}, Lcom/kamcord/android/CustomWebView;->loadUrl(Ljava/lang/String;)V

    goto :goto_0
.end method

.method public final e()V
    .locals 1

    invoke-super {p0}, La/a/a/a/KC_e;->e()V

    iget-object v0, p0, Lcom/kamcord/android/KC_ab;->M:Lcom/kamcord/android/CustomWebView;

    invoke-virtual {v0}, Lcom/kamcord/android/CustomWebView;->destroy()V

    return-void
.end method
