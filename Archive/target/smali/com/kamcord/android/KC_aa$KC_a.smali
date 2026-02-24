.class final Lcom/kamcord/android/KC_aa$KC_a;
.super Lcom/kamcord/android/KC_ab$KC_a;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_aa;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_a"
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_aa;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_aa;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_aa$KC_a;->a:Lcom/kamcord/android/KC_aa;

    invoke-direct {p0}, Lcom/kamcord/android/KC_ab$KC_a;-><init>()V

    return-void
.end method


# virtual methods
.method public final onPageFinished(Landroid/webkit/WebView;Ljava/lang/String;)V
    .locals 2

    invoke-super {p0, p1, p2}, Lcom/kamcord/android/KC_ab$KC_a;->onPageFinished(Landroid/webkit/WebView;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_aa$KC_a;->a:Lcom/kamcord/android/KC_aa;

    iget-object v0, v0, Lcom/kamcord/android/KC_aa;->M:Lcom/kamcord/android/CustomWebView;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/CustomWebView;->setIsRefreshable(Z)V

    return-void
.end method

.method public final shouldOverrideUrlLoading(Landroid/webkit/WebView;Ljava/lang/String;)Z
    .locals 6

    const/4 v5, 0x3

    const/4 v3, 0x2

    const/4 v4, 0x1

    const-string v0, "^(kc-(.*?)://)(.*)"

    invoke-static {v0}, Ljava/util/regex/Pattern;->compile(Ljava/lang/String;)Ljava/util/regex/Pattern;

    move-result-object v0

    invoke-virtual {v0, p2}, Ljava/util/regex/Pattern;->matcher(Ljava/lang/CharSequence;)Ljava/util/regex/Matcher;

    move-result-object v0

    invoke-virtual {v0}, Ljava/util/regex/Matcher;->find()Z

    move-result v1

    if-eqz v1, :cond_0

    invoke-virtual {v0, v4}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/String;->length()I

    move-result v1

    if-lez v1, :cond_0

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Link of type kc-"

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, v3}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " clicked."

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "  with content "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, v5}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-virtual {v0, v3}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v1

    const-string v2, "player"

    invoke-virtual {v1, v2}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v1

    if-eqz v1, :cond_0

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v1

    if-nez v1, :cond_1

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    iget-object v1, p0, Lcom/kamcord/android/KC_aa$KC_a;->a:Lcom/kamcord/android/KC_aa;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_aa;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-virtual {v1}, La/a/a/a/KC_f;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    :cond_0
    :goto_0
    return v4

    :cond_1
    const/4 v1, 0x3

    :try_start_0
    invoke-virtual {v0, v1}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v0

    const-string v1, "UTF-8"

    invoke-static {v0, v1}, Ljava/net/URLDecoder;->decode(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    new-instance v1, Lorg/json/JSONObject;

    invoke-direct {v1, v0}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    new-instance v2, Lcom/kamcord/android/KC_S;

    invoke-direct {v2}, Lcom/kamcord/android/KC_S;-><init>()V

    new-instance v0, Landroid/os/Bundle;

    invoke-direct {v0}, Landroid/os/Bundle;-><init>()V

    const-string v3, "jsonString"

    invoke-virtual {v1}, Lorg/json/JSONObject;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v3, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    invoke-virtual {v2, v0}, Lcom/kamcord/android/KC_S;->f(Landroid/os/Bundle;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_aa$KC_a;->a:Lcom/kamcord/android/KC_aa;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_aa;->h()La/a/a/a/KC_f;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getTabFragmentManager()Lcom/kamcord/android/KC_T;

    move-result-object v0

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KC_T;->a(La/a/a/a/KC_e;)V
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    goto :goto_0
.end method
