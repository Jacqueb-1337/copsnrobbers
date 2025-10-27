.class public Lcom/kamcord/android/ReplayActivity;
.super La/a/a/a/KC_f;


# instance fields
.field private e:Z

.field private f:Lcom/kamcord/android/KC_Z;


# direct methods
.method public constructor <init>()V
    .locals 1

    invoke-direct {p0}, La/a/a/a/KC_f;-><init>()V

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/ReplayActivity;->e:Z

    return-void
.end method


# virtual methods
.method public getBackPressed()Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/ReplayActivity;->e:Z

    return v0
.end method

.method public onBackPressed()V
    .locals 1

    invoke-super {p0}, La/a/a/a/KC_f;->onBackPressed()V

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/ReplayActivity;->e:Z

    return-void
.end method

.method public onCreate(Landroid/os/Bundle;)V
    .locals 6

    const/4 v3, 0x1

    const/4 v2, -0x1

    invoke-super {p0, p1}, La/a/a/a/KC_f;->onCreate(Landroid/os/Bundle;)V

    new-instance v1, Landroid/widget/LinearLayout;

    invoke-direct {v1, p0}, Landroid/widget/LinearLayout;-><init>(Landroid/content/Context;)V

    invoke-virtual {v1, v3}, Landroid/widget/LinearLayout;->setOrientation(I)V

    const v0, 0x12d591

    invoke-virtual {v1, v0}, Landroid/widget/LinearLayout;->setId(I)V

    invoke-virtual {v1, v3}, Landroid/widget/LinearLayout;->setGravity(I)V

    new-instance v0, Landroid/view/ViewGroup$LayoutParams;

    invoke-direct {v0, v2, v2}, Landroid/view/ViewGroup$LayoutParams;-><init>(II)V

    invoke-virtual {p0, v1, v0}, Lcom/kamcord/android/ReplayActivity;->setContentView(Landroid/view/View;Landroid/view/ViewGroup$LayoutParams;)V

    if-eqz p1, :cond_0

    :goto_0
    return-void

    :cond_0
    invoke-virtual {p0}, Lcom/kamcord/android/ReplayActivity;->getIntent()Landroid/content/Intent;

    move-result-object v0

    invoke-virtual {v0}, Landroid/content/Intent;->getExtras()Landroid/os/Bundle;

    move-result-object v0

    new-instance v2, Lorg/json/JSONObject;

    invoke-direct {v2}, Lorg/json/JSONObject;-><init>()V

    :try_start_0
    const-string v3, "title"

    const-string v4, "title"

    const-string v5, ""

    invoke-virtual {v0, v4, v5}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v2, v3, v4}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v3, "video_url"

    const-string v4, "video_url"

    const-string v5, ""

    invoke-virtual {v0, v4, v5}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v2, v3, v4}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v3, "video_id"

    const-string v4, "video_id"

    const-string v5, ""

    invoke-virtual {v0, v4, v5}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v2, v3, v4}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v3, "voice_path"

    invoke-virtual {v0, v3}, Landroid/os/Bundle;->containsKey(Ljava/lang/String;)Z

    move-result v3

    if-eqz v3, :cond_1

    const-string v3, "voice_path"

    const-string v4, "voice_path"

    const/4 v5, 0x0

    invoke-virtual {v0, v4, v5}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v2, v3, v0}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    :cond_1
    :goto_1
    new-instance v0, Landroid/os/Bundle;

    invoke-direct {v0}, Landroid/os/Bundle;-><init>()V

    const-string v3, "jsonString"

    invoke-virtual {v2}, Lorg/json/JSONObject;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v3, v2}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    new-instance v2, Lcom/kamcord/android/KC_L;

    invoke-direct {v2}, Lcom/kamcord/android/KC_L;-><init>()V

    iput-object v2, p0, Lcom/kamcord/android/ReplayActivity;->f:Lcom/kamcord/android/KC_Z;

    iget-object v2, p0, Lcom/kamcord/android/ReplayActivity;->f:Lcom/kamcord/android/KC_Z;

    invoke-virtual {v2, v0}, Lcom/kamcord/android/KC_Z;->f(Landroid/os/Bundle;)V

    invoke-virtual {p0}, Lcom/kamcord/android/ReplayActivity;->getSupportFragmentManager()La/a/a/a/KC_h;

    move-result-object v0

    invoke-virtual {v0}, La/a/a/a/KC_h;->a()La/a/a/a/KC_m;

    move-result-object v0

    invoke-virtual {v1}, Landroid/widget/LinearLayout;->getId()I

    move-result v1

    iget-object v2, p0, Lcom/kamcord/android/ReplayActivity;->f:Lcom/kamcord/android/KC_Z;

    invoke-virtual {v0, v1, v2}, La/a/a/a/KC_m;->a(ILa/a/a/a/KC_e;)La/a/a/a/KC_m;

    move-result-object v0

    invoke-virtual {v0}, La/a/a/a/KC_m;->a()I

    goto :goto_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Lorg/json/JSONException;->printStackTrace()V

    goto :goto_1
.end method
