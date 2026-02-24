.class public final Lcom/kamcord/android/KC_u;
.super Lcom/kamcord/android/KC_ab;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_u$KC_a;,
        Lcom/kamcord/android/KC_u$KC_b;
    }
.end annotation


# instance fields
.field private N:Landroid/app/Activity;

.field private O:Lcom/kamcord/a/a/e/KC_c;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/KC_ab;-><init>()V

    return-void
.end method


# virtual methods
.method public final a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 9
    .annotation build Landroid/annotation/SuppressLint;
        value = {
            "SetJavaScriptEnabled"
        }
    .end annotation

    invoke-super {p0, p1, p2, p3}, Lcom/kamcord/android/KC_ab;->a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;

    move-result-object v6

    iget-object v0, p0, Lcom/kamcord/android/KC_u;->M:Lcom/kamcord/android/CustomWebView;

    const/4 v1, -0x1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/CustomWebView;->setBackgroundColor(I)V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_u;->g()Landroid/os/Bundle;

    move-result-object v7

    if-eqz v7, :cond_1

    const-string v0, "loginUrl"

    invoke-virtual {v7, v0}, Landroid/os/Bundle;->containsKey(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_1

    const-string v0, "oauth"

    invoke-virtual {v7, v0}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    if-eqz v0, :cond_0

    const-string v1, "2.0"

    invoke-virtual {v0, v1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v1

    if-eqz v1, :cond_2

    new-instance v1, Lcom/kamcord/a/a/a/KC_a;

    invoke-direct {v1}, Lcom/kamcord/a/a/a/KC_a;-><init>()V

    const-string v0, "class"

    invoke-virtual {v7, v0}, Landroid/os/Bundle;->getSerializable(Ljava/lang/String;)Ljava/io/Serializable;

    move-result-object v0

    check-cast v0, Ljava/lang/Class;

    invoke-virtual {v1, v0}, Lcom/kamcord/a/a/a/KC_a;->a(Ljava/lang/Class;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "apiKey"

    invoke-virtual {v7, v1}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->b(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "apiSecret"

    invoke-virtual {v7, v1}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->c(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "scope"

    invoke-virtual {v7, v1}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->d(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "callback"

    invoke-virtual {v7, v1}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->a(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/a/a/a/KC_a;->a()Lcom/kamcord/a/a/e/KC_c;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_u;->O:Lcom/kamcord/a/a/e/KC_c;

    iget-object v0, p0, Lcom/kamcord/android/KC_u;->M:Lcom/kamcord/android/CustomWebView;

    new-instance v1, Lcom/kamcord/android/KC_u$KC_b;

    const-string v2, "callback"

    invoke-virtual {v7, v2}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    iget-object v3, p0, Lcom/kamcord/android/KC_u;->O:Lcom/kamcord/a/a/e/KC_c;

    iget-object v4, p0, Lcom/kamcord/android/KC_u;->N:Landroid/app/Activity;

    invoke-direct {v1, p0, v2, v3, v4}, Lcom/kamcord/android/KC_u$KC_b;-><init>(Lcom/kamcord/android/KC_u;Ljava/lang/String;Lcom/kamcord/a/a/e/KC_c;Landroid/app/Activity;)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/CustomWebView;->setWebViewClient(Landroid/webkit/WebViewClient;)V

    :cond_0
    :goto_0
    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_4

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_u;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-virtual {v1}, La/a/a/a/KC_f;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_u;->b()V

    :cond_1
    :goto_1
    return-object v6

    :cond_2
    const-string v1, "1.0"

    invoke-virtual {v0, v1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v1

    if-eqz v1, :cond_3

    new-instance v1, Lcom/kamcord/a/a/a/KC_a;

    invoke-direct {v1}, Lcom/kamcord/a/a/a/KC_a;-><init>()V

    const-string v0, "class"

    invoke-virtual {v7, v0}, Landroid/os/Bundle;->getSerializable(Ljava/lang/String;)Ljava/io/Serializable;

    move-result-object v0

    check-cast v0, Ljava/lang/Class;

    invoke-virtual {v1, v0}, Lcom/kamcord/a/a/a/KC_a;->a(Ljava/lang/Class;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "apiKey"

    invoke-virtual {v7, v1}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->b(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "apiSecret"

    invoke-virtual {v7, v1}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->c(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "callback"

    invoke-virtual {v7, v1}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->a(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/a/a/a/KC_a;->a()Lcom/kamcord/a/a/e/KC_c;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_u;->O:Lcom/kamcord/a/a/e/KC_c;

    const-string v0, "token"

    invoke-virtual {v7, v0}, Landroid/os/Bundle;->getSerializable(Ljava/lang/String;)Ljava/io/Serializable;

    move-result-object v4

    check-cast v4, Lcom/kamcord/a/a/d/KC_j;

    iget-object v8, p0, Lcom/kamcord/android/KC_u;->M:Lcom/kamcord/android/CustomWebView;

    new-instance v0, Lcom/kamcord/android/KC_u$KC_a;

    const-string v1, "callback"

    invoke-virtual {v7, v1}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    iget-object v3, p0, Lcom/kamcord/android/KC_u;->O:Lcom/kamcord/a/a/e/KC_c;

    iget-object v5, p0, Lcom/kamcord/android/KC_u;->N:Landroid/app/Activity;

    move-object v1, p0

    invoke-direct/range {v0 .. v5}, Lcom/kamcord/android/KC_u$KC_a;-><init>(Lcom/kamcord/android/KC_u;Ljava/lang/String;Lcom/kamcord/a/a/e/KC_c;Lcom/kamcord/a/a/d/KC_j;Landroid/app/Activity;)V

    invoke-virtual {v8, v0}, Lcom/kamcord/android/CustomWebView;->setWebViewClient(Landroid/webkit/WebViewClient;)V

    goto :goto_0

    :cond_3
    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Error: unknown oauth version "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    goto/16 :goto_0

    :cond_4
    iget-object v0, p0, Lcom/kamcord/android/KC_u;->M:Lcom/kamcord/android/CustomWebView;

    invoke-virtual {v0}, Lcom/kamcord/android/CustomWebView;->getWebView()Landroid/webkit/WebView;

    move-result-object v0

    invoke-static {v0}, La/a/a/c/KC_a;->a(Landroid/view/ViewGroup;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_u;->M:Lcom/kamcord/android/CustomWebView;

    const-string v1, "loginUrl"

    invoke-virtual {v7, v1}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/CustomWebView;->loadUrl(Ljava/lang/String;)V

    goto/16 :goto_1
.end method

.method public final a(Landroid/app/Activity;)V
    .locals 0

    invoke-super {p0, p1}, Lcom/kamcord/android/KC_ab;->a(Landroid/app/Activity;)V

    iput-object p1, p0, Lcom/kamcord/android/KC_u;->N:Landroid/app/Activity;

    return-void
.end method

.method final b()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_u;->O:Lcom/kamcord/a/a/e/KC_c;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_u;->O:Lcom/kamcord/a/a/e/KC_c;

    invoke-interface {v0}, Lcom/kamcord/a/a/e/KC_c;->a()V

    :cond_0
    return-void
.end method
