.class final Lcom/kamcord/android/KC_N$KC_b;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_N;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_b"
.end annotation


# instance fields
.field private a:Landroid/app/Activity;

.field private synthetic b:Lcom/kamcord/android/KC_N;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_N;Landroid/app/Activity;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_N$KC_b;->b:Lcom/kamcord/android/KC_N;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-object p2, p0, Lcom/kamcord/android/KC_N$KC_b;->a:Landroid/app/Activity;

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/view/View;)V
    .locals 5

    const/4 v4, 0x0

    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0}, Lorg/json/JSONObject;-><init>()V

    :try_start_0
    const-string v1, "Video Length"

    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_b;->b:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-wide v2, v2, Lcom/kamcord/android/core/KC_H;->h:D

    double-to-int v2, v2

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;I)Lorg/json/JSONObject;
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    new-instance v0, Landroid/content/Intent;

    iget-object v1, p0, Lcom/kamcord/android/KC_N$KC_b;->a:Landroid/app/Activity;

    const-class v2, Lcom/kamcord/android/ReplayActivity;

    invoke-direct {v0, v1, v2}, Landroid/content/Intent;-><init>(Landroid/content/Context;Ljava/lang/Class;)V

    const-string v1, "video_url"

    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_b;->b:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    new-instance v3, Ljava/lang/StringBuilder;

    invoke-direct {v3}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v2, v2, Lcom/kamcord/android/core/KC_H;->b:Ljava/lang/String;

    invoke-virtual {v3, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "/video.mp4"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    iget-object v1, p0, Lcom/kamcord/android/KC_N$KC_b;->b:Lcom/kamcord/android/KC_N;

    iget-object v1, v1, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    invoke-virtual {v1}, Lcom/kamcord/android/core/KC_H;->b()Z

    move-result v1

    if-eqz v1, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v1

    const-string v2, "voice_overlay_enabled"

    invoke-interface {v1, v2, v4}, Landroid/content/SharedPreferences;->getBoolean(Ljava/lang/String;Z)Z

    move-result v1

    if-eqz v1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/KC_N$KC_b;->b:Lcom/kamcord/android/KC_N;

    iget-object v1, v1, Lcom/kamcord/android/KC_N;->R:Landroid/widget/ToggleButton;

    invoke-virtual {v1}, Landroid/widget/ToggleButton;->isChecked()Z

    move-result v1

    if-eqz v1, :cond_0

    const-string v1, "voice_path"

    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_b;->b:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_H;->a()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    :cond_0
    const-string v1, "title"

    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_b;->b:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v2, v2, Lcom/kamcord/android/core/KC_H;->f:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v1, "video_id"

    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_b;->b:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v2, v2, Lcom/kamcord/android/core/KC_H;->i:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    iget-object v1, p0, Lcom/kamcord/android/KC_N$KC_b;->a:Landroid/app/Activity;

    invoke-virtual {v1, v0, v4}, Landroid/app/Activity;->startActivityForResult(Landroid/content/Intent;I)V

    return-void

    :catch_0
    move-exception v0

    goto :goto_0
.end method
