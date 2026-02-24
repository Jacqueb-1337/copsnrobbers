.class final Lcom/kamcord/android/KC_F$KC_a;
.super Lcom/kamcord/android/KC_ab$KC_a;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_F;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_a"
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_F;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_F;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_F$KC_a;->a:Lcom/kamcord/android/KC_F;

    invoke-direct {p0}, Lcom/kamcord/android/KC_ab$KC_a;-><init>()V

    return-void
.end method


# virtual methods
.method public final onPageFinished(Landroid/webkit/WebView;Ljava/lang/String;)V
    .locals 2

    invoke-super {p0, p1, p2}, Lcom/kamcord/android/KC_ab$KC_a;->onPageFinished(Landroid/webkit/WebView;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_F$KC_a;->a:Lcom/kamcord/android/KC_F;

    iget-object v0, v0, Lcom/kamcord/android/KC_F;->M:Lcom/kamcord/android/CustomWebView;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/CustomWebView;->setIsRefreshable(Z)V

    return-void
.end method

.method public final shouldOverrideUrlLoading(Landroid/webkit/WebView;Ljava/lang/String;)Z
    .locals 11

    const/4 v2, 0x0

    const/4 v6, 0x3

    const/4 v1, 0x0

    const/4 v3, 0x1

    const/4 v7, 0x2

    const-string v0, "^(kc-(.*?)://)(.*)"

    invoke-static {v0}, Ljava/util/regex/Pattern;->compile(Ljava/lang/String;)Ljava/util/regex/Pattern;

    move-result-object v0

    invoke-virtual {v0, p2}, Ljava/util/regex/Pattern;->matcher(Ljava/lang/CharSequence;)Ljava/util/regex/Matcher;

    move-result-object v0

    invoke-virtual {v0}, Ljava/util/regex/Matcher;->find()Z

    move-result v4

    if-eqz v4, :cond_0

    invoke-virtual {v0, v3}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/String;->length()I

    move-result v4

    if-lez v4, :cond_0

    new-instance v4, Ljava/lang/StringBuilder;

    const-string v5, "Link of type kc-"

    invoke-direct {v4, v5}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, v7}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    const-string v5, " clicked."

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-static {v4}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v4, Ljava/lang/StringBuilder;

    const-string v5, "  with content "

    invoke-direct {v4, v5}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, v6}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-static {v4}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-virtual {v0, v7}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v4

    const-string v5, "create"

    invoke-virtual {v4, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v4

    if-eqz v4, :cond_1

    new-instance v0, Lcom/kamcord/android/KC_h;

    invoke-direct {v0}, Lcom/kamcord/android/KC_h;-><init>()V

    move v10, v1

    move-object v1, v0

    move v0, v10

    :goto_0
    if-eqz v1, :cond_0

    if-eqz v0, :cond_9

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_9

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    iget-object v1, p0, Lcom/kamcord/android/KC_F$KC_a;->a:Lcom/kamcord/android/KC_F;

    invoke-static {v1}, Lcom/kamcord/android/KC_F;->a(Lcom/kamcord/android/KC_F;)Lcom/kamcord/android/KamcordActivity;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/android/KamcordActivity;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    :cond_0
    :goto_1
    return v3

    :cond_1
    invoke-virtual {v0, v7}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v4

    const-string v5, "signin"

    invoke-virtual {v4, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v4

    if-eqz v4, :cond_2

    new-instance v0, Lcom/kamcord/android/KC_P;

    invoke-direct {v0}, Lcom/kamcord/android/KC_P;-><init>()V

    move v10, v1

    move-object v1, v0

    move v0, v10

    goto :goto_0

    :cond_2
    invoke-virtual {v0, v7}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v4

    const-string v5, "settings"

    invoke-virtual {v4, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v4

    if-eqz v4, :cond_3

    new-instance v0, Lcom/kamcord/android/KC_I;

    invoke-direct {v0}, Lcom/kamcord/android/KC_I;-><init>()V

    move v10, v1

    move-object v1, v0

    move v0, v10

    goto :goto_0

    :cond_3
    invoke-virtual {v0, v7}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v4

    const-string v5, "player"

    invoke-virtual {v4, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v4

    if-eqz v4, :cond_4

    const/4 v1, 0x3

    :try_start_0
    invoke-virtual {v0, v1}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v0

    const-string v1, "UTF-8"

    invoke-static {v0, v1}, Ljava/net/URLDecoder;->decode(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    new-instance v4, Lorg/json/JSONObject;

    invoke-direct {v4, v0}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    new-instance v1, Lcom/kamcord/android/KC_S;

    invoke-direct {v1}, Lcom/kamcord/android/KC_S;-><init>()V
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0
    .catch Ljava/io/UnsupportedEncodingException; {:try_start_0 .. :try_end_0} :catch_1

    :try_start_1
    new-instance v0, Landroid/os/Bundle;

    invoke-direct {v0}, Landroid/os/Bundle;-><init>()V

    const-string v5, "jsonString"

    invoke-virtual {v4}, Lorg/json/JSONObject;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v0, v5, v4}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    invoke-virtual {v1, v0}, La/a/a/a/KC_e;->f(Landroid/os/Bundle;)V
    :try_end_1
    .catch Lorg/json/JSONException; {:try_start_1 .. :try_end_1} :catch_3
    .catch Ljava/io/UnsupportedEncodingException; {:try_start_1 .. :try_end_1} :catch_2

    move v0, v3

    goto :goto_0

    :catch_0
    move-exception v0

    move-object v1, v2

    :goto_2
    const-string v4, "Error parsing json from kc-player link."

    invoke-static {v4}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Lorg/json/JSONException;->printStackTrace()V

    move v0, v3

    goto/16 :goto_0

    :catch_1
    move-exception v0

    move-object v1, v2

    :goto_3
    invoke-virtual {v0}, Ljava/io/UnsupportedEncodingException;->printStackTrace()V

    move v0, v3

    goto/16 :goto_0

    :cond_4
    invoke-virtual {v0, v7}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v4

    const-string v5, "web"

    invoke-virtual {v4, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v4

    if-eqz v4, :cond_b

    const-string v4, "^.*?/api/(.*?)\\?(.*)"

    invoke-static {v4}, Ljava/util/regex/Pattern;->compile(Ljava/lang/String;)Ljava/util/regex/Pattern;

    move-result-object v4

    invoke-virtual {v0, v6}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v4, v0}, Ljava/util/regex/Pattern;->matcher(Ljava/lang/CharSequence;)Ljava/util/regex/Matcher;

    move-result-object v4

    invoke-virtual {v4}, Ljava/util/regex/Matcher;->find()Z

    move-result v0

    if-eqz v0, :cond_a

    invoke-virtual {v4, v3}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/String;->length()I

    move-result v0

    if-lez v0, :cond_a

    invoke-virtual {v4, v7}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/String;->length()I

    move-result v0

    if-lez v0, :cond_a

    invoke-virtual {v4, v3}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v5

    const-string v0, ""

    const-string v6, "profile/"

    invoke-virtual {v6, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v6

    if-eqz v6, :cond_6

    const-string v0, "profile"

    :cond_5
    :goto_4
    invoke-virtual {v4, v7}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v4

    const-string v5, "&"

    invoke-virtual {v4, v5}, Ljava/lang/String;->split(Ljava/lang/String;)[Ljava/lang/String;

    move-result-object v5

    array-length v6, v5

    move v4, v1

    :goto_5
    if-ge v4, v6, :cond_a

    aget-object v7, v5, v4

    const-string v8, "="

    invoke-virtual {v7, v8}, Ljava/lang/String;->split(Ljava/lang/String;)[Ljava/lang/String;

    move-result-object v7

    const-string v8, "target_user"

    aget-object v9, v7, v1

    invoke-virtual {v8, v9}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v8

    if-eqz v8, :cond_8

    new-instance v1, Lcom/kamcord/android/KC_G;

    invoke-direct {v1}, Lcom/kamcord/android/KC_G;-><init>()V

    new-instance v4, Landroid/os/Bundle;

    invoke-direct {v4}, Landroid/os/Bundle;-><init>()V

    const-string v5, "apiKey"

    invoke-virtual {v4, v5, v0}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "targetUser"

    aget-object v5, v7, v3

    invoke-virtual {v4, v0, v5}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    invoke-virtual {v1, v4}, La/a/a/a/KC_e;->f(Landroid/os/Bundle;)V

    move v0, v3

    goto/16 :goto_0

    :cond_6
    const-string v6, "follow/followinglist/"

    invoke-virtual {v6, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v6

    if-eqz v6, :cond_7

    const-string v0, "following"

    goto :goto_4

    :cond_7
    const-string v6, "follow/followerlist/"

    invoke-virtual {v6, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v5

    if-eqz v5, :cond_5

    const-string v0, "followers"

    goto :goto_4

    :cond_8
    add-int/lit8 v4, v4, 0x1

    goto :goto_5

    :cond_9
    iget-object v0, p0, Lcom/kamcord/android/KC_F$KC_a;->a:Lcom/kamcord/android/KC_F;

    invoke-static {v0}, Lcom/kamcord/android/KC_F;->a(Lcom/kamcord/android/KC_F;)Lcom/kamcord/android/KamcordActivity;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getTabFragmentManager()Lcom/kamcord/android/KC_T;

    move-result-object v0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_T;->a(La/a/a/a/KC_e;)V

    goto/16 :goto_1

    :catch_2
    move-exception v0

    goto/16 :goto_3

    :catch_3
    move-exception v0

    goto/16 :goto_2

    :cond_a
    move v0, v3

    move-object v1, v2

    goto/16 :goto_0

    :cond_b
    move v0, v1

    move-object v1, v2

    goto/16 :goto_0
.end method
