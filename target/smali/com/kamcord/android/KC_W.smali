.class public final Lcom/kamcord/android/KC_W;
.super Ljava/lang/Object;


# instance fields
.field private a:Ljava/lang/String;

.field private b:Ljava/lang/String;

.field private c:Ljava/lang/String;

.field private d:Ljava/lang/String;

.field private e:Ljava/lang/String;

.field private f:Ljava/lang/String;

.field private g:Z

.field private h:D

.field private i:Ljava/util/HashSet;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/HashSet",
            "<",
            "Ljava/lang/String;",
            ">;"
        }
    .end annotation
.end field

.field private j:Lorg/json/JSONObject;

.field private k:Ljava/lang/String;

.field private l:Ljava/lang/String;

.field private m:Ljava/lang/String;

.field private n:Z


# direct methods
.method public constructor <init>()V
    .locals 4

    const/4 v3, 0x0

    const/4 v2, 0x0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getDefaultVideoTitle()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_W;->a:Ljava/lang/String;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getDefaultYoutubeDescription()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_W;->c:Ljava/lang/String;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getDefaultFacebookDescription()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_W;->d:Ljava/lang/String;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getDefaultTwitterDescription()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_W;->e:Ljava/lang/String;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getDefaultYoutubeKeywords()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_W;->f:Ljava/lang/String;

    iput-boolean v3, p0, Lcom/kamcord/android/KC_W;->g:Z

    const-wide/16 v0, 0x0

    iput-wide v0, p0, Lcom/kamcord/android/KC_W;->h:D

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_W;->i:Ljava/util/HashSet;

    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0}, Lorg/json/JSONObject;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_W;->j:Lorg/json/JSONObject;

    iput-object v2, p0, Lcom/kamcord/android/KC_W;->k:Ljava/lang/String;

    iput-object v2, p0, Lcom/kamcord/android/KC_W;->l:Ljava/lang/String;

    iput-object v2, p0, Lcom/kamcord/android/KC_W;->m:Ljava/lang/String;

    iput-boolean v3, p0, Lcom/kamcord/android/KC_W;->n:Z

    return-void
.end method


# virtual methods
.method public final a()Landroid/content/Intent;
    .locals 4

    new-instance v0, Landroid/content/Intent;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getApplicationContext()Landroid/content/Context;

    move-result-object v1

    const-class v2, Lcom/kamcord/android/UploadService;

    invoke-direct {v0, v1, v2}, Landroid/content/Intent;-><init>(Landroid/content/Context;Ljava/lang/Class;)V

    const-string v1, "title"

    iget-object v2, p0, Lcom/kamcord/android/KC_W;->a:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    iget-object v1, p0, Lcom/kamcord/android/KC_W;->b:Ljava/lang/String;

    if-eqz v1, :cond_0

    const-string v1, "message"

    iget-object v2, p0, Lcom/kamcord/android/KC_W;->b:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    :cond_0
    const-string v1, "description"

    iget-object v2, p0, Lcom/kamcord/android/KC_W;->c:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v1, "facebook_description"

    iget-object v2, p0, Lcom/kamcord/android/KC_W;->d:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v1, "twitter_description"

    iget-object v2, p0, Lcom/kamcord/android/KC_W;->e:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v1, "keywords"

    iget-object v2, p0, Lcom/kamcord/android/KC_W;->f:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v1, "has_voice_enabled"

    iget-boolean v2, p0, Lcom/kamcord/android/KC_W;->g:Z

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Z)Landroid/content/Intent;

    const-string v1, "video_duration"

    iget-wide v2, p0, Lcom/kamcord/android/KC_W;->h:D

    invoke-virtual {v0, v1, v2, v3}, Landroid/content/Intent;->putExtra(Ljava/lang/String;D)Landroid/content/Intent;

    const-string v1, "shared_on"

    iget-object v2, p0, Lcom/kamcord/android/KC_W;->i:Ljava/util/HashSet;

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/io/Serializable;)Landroid/content/Intent;

    const-string v1, "developer_metadata_obj"

    iget-object v2, p0, Lcom/kamcord/android/KC_W;->j:Lorg/json/JSONObject;

    invoke-virtual {v2}, Lorg/json/JSONObject;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v1, "video_directory_path"

    iget-object v2, p0, Lcom/kamcord/android/KC_W;->k:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v1, "local_video_id"

    iget-object v2, p0, Lcom/kamcord/android/KC_W;->l:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    iget-object v1, p0, Lcom/kamcord/android/KC_W;->m:Ljava/lang/String;

    if-eqz v1, :cond_1

    const-string v1, "server_video_id"

    iget-object v2, p0, Lcom/kamcord/android/KC_W;->m:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    :cond_1
    const-string v1, "is_reshare"

    iget-boolean v2, p0, Lcom/kamcord/android/KC_W;->n:Z

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Z)Landroid/content/Intent;

    return-object v0
.end method

.method public final a(D)Lcom/kamcord/android/KC_W;
    .locals 1

    iput-wide p1, p0, Lcom/kamcord/android/KC_W;->h:D

    return-object p0
.end method

.method public final a(Ljava/lang/String;)Lcom/kamcord/android/KC_W;
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_W;->b:Ljava/lang/String;

    return-object p0
.end method

.method public final a(Lorg/json/JSONObject;)Lcom/kamcord/android/KC_W;
    .locals 0

    if-eqz p1, :cond_0

    iput-object p1, p0, Lcom/kamcord/android/KC_W;->j:Lorg/json/JSONObject;

    :cond_0
    return-object p0
.end method

.method public final a(Z)Lcom/kamcord/android/KC_W;
    .locals 0

    iput-boolean p1, p0, Lcom/kamcord/android/KC_W;->g:Z

    return-object p0
.end method

.method public final b(Ljava/lang/String;)Lcom/kamcord/android/KC_W;
    .locals 1

    if-eqz p1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_W;->i:Ljava/util/HashSet;

    invoke-virtual {v0, p1}, Ljava/util/HashSet;->add(Ljava/lang/Object;)Z

    :cond_0
    return-object p0
.end method

.method public final b(Z)Lcom/kamcord/android/KC_W;
    .locals 1

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/KC_W;->n:Z

    return-object p0
.end method

.method public final c(Ljava/lang/String;)Lcom/kamcord/android/KC_W;
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_W;->k:Ljava/lang/String;

    return-object p0
.end method

.method public final d(Ljava/lang/String;)Lcom/kamcord/android/KC_W;
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_W;->l:Ljava/lang/String;

    return-object p0
.end method

.method public final e(Ljava/lang/String;)Lcom/kamcord/android/KC_W;
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_W;->m:Ljava/lang/String;

    return-object p0
.end method
