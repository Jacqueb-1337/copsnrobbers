.class public Lcom/kamcord/android/KC_S;
.super Lcom/kamcord/android/KC_Z;

# interfaces
.implements Landroid/media/MediaPlayer$OnBufferingUpdateListener;
.implements Landroid/media/MediaPlayer$OnCompletionListener;
.implements Landroid/media/MediaPlayer$OnErrorListener;
.implements Landroid/media/MediaPlayer$OnInfoListener;
.implements Landroid/media/MediaPlayer$OnPreparedListener;
.implements Landroid/media/MediaPlayer$OnSeekCompleteListener;
.implements Landroid/media/MediaPlayer$OnVideoSizeChangedListener;
.implements Lcom/kamcord/android/CustomWebView$KC_a;


# annotations
.annotation build Landroid/annotation/SuppressLint;
    value = {
        "SetJavaScriptEnabled"
    }
.end annotation

.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_S$KC_b;,
        Lcom/kamcord/android/KC_S$KC_a;
    }
.end annotation


# instance fields
.field public M:Lcom/kamcord/android/KC_Q;

.field private U:Ljava/lang/ref/WeakReference;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/lang/ref/WeakReference",
            "<",
            "Lcom/kamcord/android/KamcordActivity;",
            ">;"
        }
    .end annotation
.end field

.field private V:La/a/a/c/KC_a;

.field private W:I

.field private X:I

.field private Y:Ljava/lang/String;

.field private Z:Ljava/lang/String;

.field private aa:Ljava/lang/String;

.field private ab:Landroid/media/MediaPlayer;

.field private ac:Landroid/os/Handler;

.field private ad:Z

.field private ae:I

.field private af:I

.field private ag:Z


# direct methods
.method public constructor <init>()V
    .locals 2

    const/4 v1, 0x0

    invoke-direct {p0}, Lcom/kamcord/android/KC_Z;-><init>()V

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->V:La/a/a/c/KC_a;

    new-instance v0, Lcom/kamcord/android/KC_S$KC_b;

    invoke-direct {v0, p0}, Lcom/kamcord/android/KC_S$KC_b;-><init>(Lcom/kamcord/android/KC_S;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->ac:Landroid/os/Handler;

    iput-boolean v1, p0, Lcom/kamcord/android/KC_S;->ad:Z

    iput v1, p0, Lcom/kamcord/android/KC_S;->ae:I

    iput v1, p0, Lcom/kamcord/android/KC_S;->af:I

    iput-boolean v1, p0, Lcom/kamcord/android/KC_S;->ag:Z

    return-void
.end method

.method private D()Z
    .locals 7

    const/4 v1, 0x1

    const/4 v6, 0x0

    const/4 v2, 0x0

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->T:Lorg/json/JSONObject;

    const-string v3, "videos"

    invoke-virtual {v0, v3}, Lorg/json/JSONObject;->has(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_0

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_S;->T:Lorg/json/JSONObject;

    const-string v3, "start_pos"

    invoke-virtual {v0, v3}, Lorg/json/JSONObject;->getInt(Ljava/lang/String;)I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/KC_S;->W:I

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->T:Lorg/json/JSONObject;

    const-string v3, "videos"

    invoke-virtual {v0, v3}, Lorg/json/JSONObject;->getJSONArray(Ljava/lang/String;)Lorg/json/JSONArray;

    move-result-object v0

    invoke-virtual {v0}, Lorg/json/JSONArray;->length()I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/KC_S;->X:I

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->T:Lorg/json/JSONObject;

    const-string v3, "videos"

    invoke-virtual {v0, v3}, Lorg/json/JSONObject;->getJSONArray(Ljava/lang/String;)Lorg/json/JSONArray;

    move-result-object v0

    iget v3, p0, Lcom/kamcord/android/KC_S;->W:I

    invoke-virtual {v0, v3}, Lorg/json/JSONArray;->getJSONObject(I)Lorg/json/JSONObject;

    move-result-object v0

    const-string v3, "title"

    invoke-virtual {v0, v3}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    iput-object v3, p0, Lcom/kamcord/android/KC_S;->Y:Ljava/lang/String;

    const-string v3, "video_url"

    invoke-virtual {v0, v3}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    invoke-static {v3}, Lcom/kamcord/android/KC_S;->a(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    iput-object v3, p0, Lcom/kamcord/android/KC_S;->Z:Ljava/lang/String;

    const-string v3, "video_id"

    invoke-virtual {v0, v3}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->aa:Ljava/lang/String;

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    new-instance v3, Lcom/kamcord/android/KC_S$1;

    invoke-direct {v3, p0}, Lcom/kamcord/android/KC_S$1;-><init>(Lcom/kamcord/android/KC_S;)V

    new-instance v4, Lcom/kamcord/android/KC_S$2;

    invoke-direct {v4, p0}, Lcom/kamcord/android/KC_S$2;-><init>(Lcom/kamcord/android/KC_S;)V

    new-instance v5, Lcom/kamcord/android/KC_S$3;

    invoke-direct {v5, p0}, Lcom/kamcord/android/KC_S$3;-><init>(Lcom/kamcord/android/KC_S;)V

    invoke-virtual {v0, v3, v4, v5}, Lcom/kamcord/android/KC_w;->a(Landroid/view/View$OnClickListener;Landroid/view/View$OnClickListener;Landroid/view/View$OnClickListener;)V
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    iget-object v0, p0, Lcom/kamcord/android/KC_S;->T:Lorg/json/JSONObject;

    const-string v3, "videos"

    invoke-virtual {v0, v3}, Lorg/json/JSONObject;->has(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    new-instance v3, Ljava/lang/StringBuilder;

    invoke-direct {v3}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v4, p0, Lcom/kamcord/android/KC_S;->Y:Ljava/lang/String;

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, " - "

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    iget v4, p0, Lcom/kamcord/android/KC_S;->W:I

    add-int/lit8 v4, v4, 0x1

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, "/"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    iget v4, p0, Lcom/kamcord/android/KC_S;->X:I

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v0, v3}, Lcom/kamcord/android/KC_w;->a(Ljava/lang/String;)V

    :goto_1
    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KC_w;->a(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->R:Landroid/view/View;

    const-string v3, "id"

    const-string v4, "intermediateBottom"

    invoke-static {v3, v4}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v3

    invoke-virtual {v0, v3}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/CustomWebView;

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->S:Lcom/kamcord/android/CustomWebView;

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->S:Lcom/kamcord/android/CustomWebView;

    new-instance v3, Lcom/kamcord/android/KC_S$KC_a;

    invoke-direct {v3, p0}, Lcom/kamcord/android/KC_S$KC_a;-><init>(Lcom/kamcord/android/KC_S;)V

    invoke-virtual {v0, v3}, Lcom/kamcord/android/CustomWebView;->setWebViewClient(Landroid/webkit/WebViewClient;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->S:Lcom/kamcord/android/CustomWebView;

    invoke-virtual {v0}, Lcom/kamcord/android/CustomWebView;->getSettings()Landroid/webkit/WebSettings;

    move-result-object v0

    invoke-virtual {v0, v1}, Landroid/webkit/WebSettings;->setJavaScriptEnabled(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->S:Lcom/kamcord/android/CustomWebView;

    invoke-virtual {v0, v2}, Lcom/kamcord/android/CustomWebView;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->S:Lcom/kamcord/android/CustomWebView;

    invoke-virtual {v0}, Lcom/kamcord/android/CustomWebView;->a()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->S:Lcom/kamcord/android/CustomWebView;

    invoke-virtual {v0, p0}, Lcom/kamcord/android/CustomWebView;->addPullToRefreshListener(Lcom/kamcord/android/CustomWebView$KC_a;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->S:Lcom/kamcord/android/CustomWebView;

    const v2, 0x3f19999a    # 0.6f

    invoke-virtual {v0, v2}, Lcom/kamcord/android/CustomWebView;->setRefreshDistanceHeightFraction(F)V

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_2

    new-instance v2, Lcom/kamcord/android/KC_x;

    invoke-direct {v2}, Lcom/kamcord/android/KC_x;-><init>()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->U:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v0

    invoke-virtual {v2, v0, v6}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    :goto_2
    move v0, v1

    :goto_3
    return v0

    :cond_0
    const/4 v0, -0x1

    :try_start_1
    iput v0, p0, Lcom/kamcord/android/KC_S;->W:I

    const/4 v0, 0x0

    iput v0, p0, Lcom/kamcord/android/KC_S;->X:I

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->T:Lorg/json/JSONObject;

    const-string v3, "title"

    invoke-virtual {v0, v3}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->Y:Ljava/lang/String;

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->T:Lorg/json/JSONObject;

    const-string v3, "video_url"

    invoke-virtual {v0, v3}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/KC_S;->a(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->Z:Ljava/lang/String;

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->T:Lorg/json/JSONObject;

    const-string v3, "video_id"

    invoke-virtual {v0, v3}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->aa:Ljava/lang/String;

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    const/4 v3, 0x0

    const/4 v4, 0x0

    new-instance v5, Lcom/kamcord/android/KC_S$4;

    invoke-direct {v5, p0}, Lcom/kamcord/android/KC_S$4;-><init>(Lcom/kamcord/android/KC_S;)V

    invoke-virtual {v0, v3, v4, v5}, Lcom/kamcord/android/KC_w;->a(Landroid/view/View$OnClickListener;Landroid/view/View$OnClickListener;Landroid/view/View$OnClickListener;)V
    :try_end_1
    .catch Lorg/json/JSONException; {:try_start_1 .. :try_end_1} :catch_0

    goto/16 :goto_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Lorg/json/JSONException;->printStackTrace()V

    new-instance v0, Lcom/kamcord/android/KC_j;

    const-string v1, "kamcordNoVideoAvailable"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, Lcom/kamcord/android/KC_j;-><init>(Ljava/lang/String;)V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_S;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-virtual {v1}, La/a/a/a/KC_f;->getSupportFragmentManager()La/a/a/a/KC_h;

    move-result-object v1

    const-string v3, ""

    invoke-virtual {v0, v1, v3}, La/a/a/a/KC_d;->a(La/a/a/a/KC_h;Ljava/lang/String;)V

    move v0, v2

    goto :goto_3

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    iget-object v3, p0, Lcom/kamcord/android/KC_S;->Y:Ljava/lang/String;

    invoke-virtual {v0, v3}, Lcom/kamcord/android/KC_w;->a(Ljava/lang/String;)V

    goto/16 :goto_1

    :cond_2
    invoke-direct {p0}, Lcom/kamcord/android/KC_S;->F()V

    goto :goto_2
.end method

.method private E()V
    .locals 3

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_0

    new-instance v1, Lcom/kamcord/android/KC_x;

    invoke-direct {v1}, Lcom/kamcord/android/KC_x;-><init>()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->U:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v0

    const/4 v2, 0x0

    invoke-virtual {v1, v0, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    :goto_0
    return-void

    :cond_0
    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_S;->T:Lorg/json/JSONObject;

    const-string v1, "videos"

    invoke-virtual {v0, v1}, Lorg/json/JSONObject;->getJSONArray(Ljava/lang/String;)Lorg/json/JSONArray;

    move-result-object v0

    iget v1, p0, Lcom/kamcord/android/KC_S;->W:I

    invoke-virtual {v0, v1}, Lorg/json/JSONArray;->getJSONObject(I)Lorg/json/JSONObject;

    move-result-object v0

    const-string v1, "title"

    invoke-virtual {v0, v1}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->Y:Ljava/lang/String;

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->T:Lorg/json/JSONObject;

    const-string v1, "videos"

    invoke-virtual {v0, v1}, Lorg/json/JSONObject;->getJSONArray(Ljava/lang/String;)Lorg/json/JSONArray;

    move-result-object v0

    iget v1, p0, Lcom/kamcord/android/KC_S;->W:I

    invoke-virtual {v0, v1}, Lorg/json/JSONArray;->getJSONObject(I)Lorg/json/JSONObject;

    move-result-object v0

    const-string v1, "video_url"

    invoke-virtual {v0, v1}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->Z:Ljava/lang/String;

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->T:Lorg/json/JSONObject;

    const-string v1, "videos"

    invoke-virtual {v0, v1}, Lorg/json/JSONObject;->getJSONArray(Ljava/lang/String;)Lorg/json/JSONArray;

    move-result-object v0

    iget v1, p0, Lcom/kamcord/android/KC_S;->W:I

    invoke-virtual {v0, v1}, Lorg/json/JSONArray;->getJSONObject(I)Lorg/json/JSONObject;

    move-result-object v0

    const-string v1, "video_id"

    invoke-virtual {v0, v1}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->aa:Ljava/lang/String;

    const-string v0, "  playing video..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "    index: "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget v1, p0, Lcom/kamcord/android/KC_S;->W:I

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "    title: "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v1, p0, Lcom/kamcord/android/KC_S;->Y:Ljava/lang/String;

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "    url: "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v1, p0, Lcom/kamcord/android/KC_S;->Z:Ljava/lang/String;

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "    id: "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v1, p0, Lcom/kamcord/android/KC_S;->aa:Ljava/lang/String;

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    :goto_1
    :try_start_1
    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->f()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Q;->d()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    iget-object v1, p0, Lcom/kamcord/android/KC_S;->Z:Ljava/lang/String;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_Q;->a(Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Q;->b()V
    :try_end_1
    .catch Ljava/io/IOException; {:try_start_1 .. :try_end_1} :catch_1
    .catch Ljava/lang/IllegalStateException; {:try_start_1 .. :try_end_1} :catch_2

    :goto_2
    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v2, p0, Lcom/kamcord/android/KC_S;->Y:Ljava/lang/String;

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " - "

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    iget v2, p0, Lcom/kamcord/android/KC_S;->W:I

    add-int/lit8 v2, v2, 0x1

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "/"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    iget v2, p0, Lcom/kamcord/android/KC_S;->X:I

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_w;->a(Ljava/lang/String;)V

    invoke-direct {p0}, Lcom/kamcord/android/KC_S;->F()V

    goto/16 :goto_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Lorg/json/JSONException;->printStackTrace()V

    goto :goto_1

    :catch_1
    move-exception v0

    invoke-virtual {v0}, Ljava/io/IOException;->printStackTrace()V

    new-instance v0, Lcom/kamcord/android/KC_j;

    const-string v1, "kamcordNoVideoAvailable"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, Lcom/kamcord/android/KC_j;-><init>(Ljava/lang/String;)V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_S;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-virtual {v1}, La/a/a/a/KC_f;->getSupportFragmentManager()La/a/a/a/KC_h;

    move-result-object v1

    const-string v2, ""

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_j;->a(La/a/a/a/KC_h;Ljava/lang/String;)V

    goto :goto_2

    :catch_2
    move-exception v0

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->U:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->onBackPressed()V

    goto :goto_2
.end method

.method private F()V
    .locals 4

    new-instance v0, Ljava/util/ArrayList;

    invoke-direct {v0}, Ljava/util/ArrayList;-><init>()V

    new-instance v1, Lorg/apache/http/message/BasicNameValuePair;

    const-string v2, "video_id"

    iget-object v3, p0, Lcom/kamcord/android/KC_S;->aa:Ljava/lang/String;

    invoke-direct {v1, v2, v3}, Lorg/apache/http/message/BasicNameValuePair;-><init>(Ljava/lang/String;Ljava/lang/String;)V

    invoke-virtual {v0, v1}, Ljava/util/ArrayList;->add(Ljava/lang/Object;)Z

    iget-object v1, p0, Lcom/kamcord/android/KC_S;->S:Lcom/kamcord/android/CustomWebView;

    const-string v2, "intermediateBottom"

    invoke-static {v1, v2, v0}, Lcom/kamcord/android/KC_d;->a(Lcom/kamcord/android/CustomWebView;Ljava/lang/String;Ljava/util/List;)Z

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_S;)Ljava/lang/ref/WeakReference;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->U:Ljava/lang/ref/WeakReference;

    return-object v0
.end method

.method static synthetic a(Lcom/kamcord/android/KC_S;Z)Z
    .locals 1

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_S;->ad:Z

    return v0
.end method

.method static synthetic b(Lcom/kamcord/android/KC_S;)Landroid/media/MediaPlayer;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->ab:Landroid/media/MediaPlayer;

    return-object v0
.end method

.method static synthetic c(Lcom/kamcord/android/KC_S;)Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/KC_S;->ad:Z

    return v0
.end method

.method static synthetic d(Lcom/kamcord/android/KC_S;)La/a/a/c/KC_a;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->V:La/a/a/c/KC_a;

    return-object v0
.end method


# virtual methods
.method public final declared-synchronized B()V
    .locals 2

    monitor-enter p0

    :try_start_0
    iget v0, p0, Lcom/kamcord/android/KC_S;->W:I

    iget v1, p0, Lcom/kamcord/android/KC_S;->X:I

    add-int/lit8 v1, v1, -0x1

    if-ge v0, v1, :cond_1

    iget v0, p0, Lcom/kamcord/android/KC_S;->W:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KC_S;->W:I

    invoke-direct {p0}, Lcom/kamcord/android/KC_S;->E()V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :cond_0
    :goto_0
    monitor-exit p0

    return-void

    :cond_1
    :try_start_1
    iget-boolean v0, p0, Lcom/kamcord/android/KC_S;->ag:Z

    if-nez v0, :cond_0

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/KC_S;->ag:Z

    invoke-virtual {p0}, Lcom/kamcord/android/KC_S;->h()La/a/a/a/KC_f;

    move-result-object v0

    invoke-virtual {v0}, La/a/a/a/KC_f;->onBackPressed()V
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 4

    const/high16 v3, 0x1000000

    invoke-super {p0, p1, p2, p3}, Lcom/kamcord/android/KC_Z;->a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;

    move-result-object v1

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->U:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getTabBar()Lcom/kamcord/android/KC_q;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/KC_q;->c()La/a/a/c/KC_a;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->V:La/a/a/c/KC_a;

    invoke-direct {p0}, Lcom/kamcord/android/KC_S;->D()Z

    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v2, 0xb

    if-lt v0, v2, :cond_0

    invoke-virtual {p0}, Lcom/kamcord/android/KC_S;->h()La/a/a/a/KC_f;

    move-result-object v0

    invoke-virtual {v0}, La/a/a/a/KC_f;->getWindow()Landroid/view/Window;

    move-result-object v0

    invoke-virtual {v0, v3, v3}, Landroid/view/Window;->setFlags(II)V

    :cond_0
    return-object v1
.end method

.method public final a(Landroid/app/Activity;)V
    .locals 1

    invoke-super {p0, p1}, Lcom/kamcord/android/KC_Z;->a(Landroid/app/Activity;)V

    new-instance v0, Ljava/lang/ref/WeakReference;

    check-cast p1, Lcom/kamcord/android/KamcordActivity;

    invoke-direct {v0, p1}, Ljava/lang/ref/WeakReference;-><init>(Ljava/lang/Object;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->U:Ljava/lang/ref/WeakReference;

    return-void
.end method

.method public final a_()V
    .locals 4

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_0

    new-instance v1, Lcom/kamcord/android/KC_x;

    invoke-direct {v1}, Lcom/kamcord/android/KC_x;-><init>()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->U:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v0

    const/4 v2, 0x0

    invoke-virtual {v1, v0, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    :goto_0
    return-void

    :cond_0
    new-instance v0, Ljava/util/ArrayList;

    invoke-direct {v0}, Ljava/util/ArrayList;-><init>()V

    new-instance v1, Lorg/apache/http/message/BasicNameValuePair;

    const-string v2, "video_id"

    iget-object v3, p0, Lcom/kamcord/android/KC_S;->aa:Ljava/lang/String;

    invoke-direct {v1, v2, v3}, Lorg/apache/http/message/BasicNameValuePair;-><init>(Ljava/lang/String;Ljava/lang/String;)V

    invoke-virtual {v0, v1}, Ljava/util/ArrayList;->add(Ljava/lang/Object;)Z

    iget-object v1, p0, Lcom/kamcord/android/KC_S;->S:Lcom/kamcord/android/CustomWebView;

    const/4 v2, 0x0

    invoke-virtual {v1, v2}, Lcom/kamcord/android/CustomWebView;->setIsRefreshable(Z)V

    iget-object v1, p0, Lcom/kamcord/android/KC_S;->S:Lcom/kamcord/android/CustomWebView;

    const-string v2, "intermediateBottom"

    invoke-static {v1, v2, v0}, Lcom/kamcord/android/KC_d;->a(Lcom/kamcord/android/CustomWebView;Ljava/lang/String;Ljava/util/List;)Z

    goto :goto_0
.end method

.method public final declared-synchronized b()V
    .locals 1

    monitor-enter p0

    :try_start_0
    iget v0, p0, Lcom/kamcord/android/KC_S;->W:I

    if-lez v0, :cond_0

    iget v0, p0, Lcom/kamcord/android/KC_S;->W:I

    add-int/lit8 v0, v0, -0x1

    iput v0, p0, Lcom/kamcord/android/KC_S;->W:I

    invoke-direct {p0}, Lcom/kamcord/android/KC_S;->E()V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :cond_0
    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method protected final c(Z)V
    .locals 6

    const/4 v4, 0x0

    const/4 v5, -0x1

    new-instance v1, Landroid/widget/LinearLayout$LayoutParams;

    invoke-direct {v1, v5, v4}, Landroid/widget/LinearLayout$LayoutParams;-><init>(II)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->U:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    const-string v2, "id"

    const-string v3, "kamcord_main"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KamcordActivity;->findViewById(I)Landroid/view/View;

    move-result-object v2

    invoke-virtual {v2}, Landroid/view/View;->getLayoutParams()Landroid/view/ViewGroup$LayoutParams;

    move-result-object v3

    if-eqz p1, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->U:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getTabBar()Lcom/kamcord/android/KC_q;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/KC_q;->a()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->S:Lcom/kamcord/android/CustomWebView;

    const/16 v4, 0x8

    invoke-virtual {v0, v4}, Lcom/kamcord/android/CustomWebView;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    const/4 v4, 0x1

    invoke-virtual {v0, v4}, Lcom/kamcord/android/KC_w;->a(Z)V

    iput v5, v1, Landroid/widget/LinearLayout$LayoutParams;->width:I

    iput v5, v1, Landroid/widget/LinearLayout$LayoutParams;->height:I

    iget v0, v3, Landroid/view/ViewGroup$LayoutParams;->width:I

    iput v0, p0, Lcom/kamcord/android/KC_S;->ae:I

    iget v0, v3, Landroid/view/ViewGroup$LayoutParams;->height:I

    iput v0, p0, Lcom/kamcord/android/KC_S;->af:I

    iput v5, v3, Landroid/view/ViewGroup$LayoutParams;->width:I

    iput v5, v3, Landroid/view/ViewGroup$LayoutParams;->height:I

    :cond_0
    :goto_0
    invoke-virtual {v2, v3}, Landroid/view/View;->setLayoutParams(Landroid/view/ViewGroup$LayoutParams;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->Q:Landroid/widget/FrameLayout;

    invoke-virtual {v0, v1}, Landroid/widget/FrameLayout;->setLayoutParams(Landroid/view/ViewGroup$LayoutParams;)V

    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_S;->U:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getTabBar()Lcom/kamcord/android/KC_q;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/KC_q;->b()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->S:Lcom/kamcord/android/CustomWebView;

    invoke-virtual {v0, v4}, Lcom/kamcord/android/CustomWebView;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0, v4}, Lcom/kamcord/android/KC_w;->a(Z)V

    iput v5, v1, Landroid/widget/LinearLayout$LayoutParams;->width:I

    iput v4, v1, Landroid/widget/LinearLayout$LayoutParams;->height:I

    const/high16 v0, 0x40400000    # 3.0f

    iput v0, v1, Landroid/widget/LinearLayout$LayoutParams;->weight:F

    iget v0, p0, Lcom/kamcord/android/KC_S;->ae:I

    if-eqz v0, :cond_0

    iget v0, p0, Lcom/kamcord/android/KC_S;->af:I

    if-eqz v0, :cond_0

    iget v0, p0, Lcom/kamcord/android/KC_S;->ae:I

    iput v0, v3, Landroid/view/ViewGroup$LayoutParams;->width:I

    iget v0, p0, Lcom/kamcord/android/KC_S;->af:I

    iput v0, v3, Landroid/view/ViewGroup$LayoutParams;->height:I

    iput v4, p0, Lcom/kamcord/android/KC_S;->ae:I

    iput v4, p0, Lcom/kamcord/android/KC_S;->af:I

    goto :goto_0
.end method

.method public final e()V
    .locals 1

    invoke-super {p0}, Lcom/kamcord/android/KC_Z;->e()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->U:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getTabBar()Lcom/kamcord/android/KC_q;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/KC_q;->b()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->c()Z

    move-result v0

    if-eqz v0, :cond_0

    const/4 v0, 0x0

    invoke-virtual {p0, v0}, Lcom/kamcord/android/KC_S;->c(Z)V

    :cond_0
    return-void
.end method

.method public final o()V
    .locals 2

    invoke-super {p0}, Lcom/kamcord/android/KC_Z;->o()V

    new-instance v0, Landroid/media/MediaPlayer;

    invoke-direct {v0}, Landroid/media/MediaPlayer;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->ab:Landroid/media/MediaPlayer;

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->ab:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnBufferingUpdateListener(Landroid/media/MediaPlayer$OnBufferingUpdateListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->ab:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnCompletionListener(Landroid/media/MediaPlayer$OnCompletionListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->ab:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnErrorListener(Landroid/media/MediaPlayer$OnErrorListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->ab:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnInfoListener(Landroid/media/MediaPlayer$OnInfoListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->ab:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnPreparedListener(Landroid/media/MediaPlayer$OnPreparedListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->ab:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnSeekCompleteListener(Landroid/media/MediaPlayer$OnSeekCompleteListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->ab:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnVideoSizeChangedListener(Landroid/media/MediaPlayer$OnVideoSizeChangedListener;)V

    new-instance v0, Lcom/kamcord/android/KC_Q;

    iget-object v1, p0, Lcom/kamcord/android/KC_S;->ab:Landroid/media/MediaPlayer;

    invoke-direct {v0, v1}, Lcom/kamcord/android/KC_Q;-><init>(Landroid/media/MediaPlayer;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    iget-object v1, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_w;->a(Lcom/kamcord/android/KC_E;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_w;->setEnabled(Z)V

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_S;->Q:Landroid/widget/FrameLayout;

    invoke-static {v0}, La/a/a/c/KC_a;->a(Landroid/view/ViewGroup;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    iget-object v1, p0, Lcom/kamcord/android/KC_S;->Z:Ljava/lang/String;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_Q;->a(Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Q;->b()V
    :try_end_0
    .catch Ljava/lang/IllegalStateException; {:try_start_0 .. :try_end_0} :catch_0
    .catch Ljava/io/IOException; {:try_start_0 .. :try_end_0} :catch_1

    :goto_0
    return-void

    :catch_0
    move-exception v0

    const-string v1, "Media player source could not be set."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/IllegalStateException;->printStackTrace()V

    goto :goto_0

    :catch_1
    move-exception v0

    const-string v1, "Video data could not be read."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/io/IOException;->printStackTrace()V

    goto :goto_0
.end method

.method public onBufferingUpdate(Landroid/media/MediaPlayer;I)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0, p2}, Lcom/kamcord/android/KC_Q;->a(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->e()I

    return-void
.end method

.method public onCompletion(Landroid/media/MediaPlayer;)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Q;->f()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->T:Lorg/json/JSONObject;

    const-string v1, "videos"

    invoke-virtual {v0, v1}, Lorg/json/JSONObject;->has(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-virtual {p0}, Lcom/kamcord/android/KC_S;->B()V

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->g()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->a()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->c()Z

    move-result v0

    if-eqz v0, :cond_0

    const/4 v0, 0x0

    invoke-virtual {p0, v0}, Lcom/kamcord/android/KC_S;->c(Z)V

    goto :goto_0
.end method

.method public onError(Landroid/media/MediaPlayer;II)Z
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0, p2}, Lcom/kamcord/android/KC_Q;->b(I)Z

    move-result v0

    return v0
.end method

.method public onInfo(Landroid/media/MediaPlayer;II)Z
    .locals 1

    packed-switch p2, :pswitch_data_0

    :goto_0
    const/4 v0, 0x0

    return v0

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/KC_S;->Q:Landroid/widget/FrameLayout;

    invoke-static {v0}, La/a/a/c/KC_a;->a(Landroid/view/ViewGroup;)V

    goto :goto_0

    :pswitch_1
    iget-object v0, p0, Lcom/kamcord/android/KC_S;->Q:Landroid/widget/FrameLayout;

    invoke-static {v0}, La/a/a/c/KC_a;->b(Landroid/view/ViewGroup;)V

    goto :goto_0

    nop

    :pswitch_data_0
    .packed-switch 0x2bd
        :pswitch_0
        :pswitch_1
    .end packed-switch
.end method

.method public onPrepared(Landroid/media/MediaPlayer;)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->Q:Landroid/widget/FrameLayout;

    invoke-static {v0}, La/a/a/c/KC_a;->b(Landroid/view/ViewGroup;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Q;->e()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->ac:Landroid/os/Handler;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Landroid/os/Handler;->sendEmptyMessage(I)Z

    return-void
.end method

.method public onSeekComplete(Landroid/media/MediaPlayer;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Q;->g()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->e()I

    return-void
.end method

.method public onSurfaceTextureAvailable(Landroid/graphics/SurfaceTexture;II)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->ac:Landroid/os/Handler;

    const/4 v1, 0x2

    invoke-virtual {v0, v1}, Landroid/os/Handler;->sendEmptyMessage(I)Z

    return-void
.end method

.method public onSurfaceTextureDestroyed(Landroid/graphics/SurfaceTexture;)Z
    .locals 1

    const/4 v0, 0x0

    return v0
.end method

.method public onSurfaceTextureSizeChanged(Landroid/graphics/SurfaceTexture;II)V
    .locals 0

    return-void
.end method

.method public onSurfaceTextureUpdated(Landroid/graphics/SurfaceTexture;)V
    .locals 0

    return-void
.end method

.method public onVideoSizeChanged(Landroid/media/MediaPlayer;II)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->O:Lcom/kamcord/android/AspectRatioTextureView;

    invoke-virtual {v0, p2, p3}, Lcom/kamcord/android/AspectRatioTextureView;->setAspectRatio(II)V

    return-void
.end method

.method public final p()V
    .locals 2

    const/4 v1, 0x0

    invoke-super {p0}, Lcom/kamcord/android/KC_Z;->p()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->f()V

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Q;->getCurrentPosition()I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/KC_S;->P:I

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Q;->d()V

    iget-object v0, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Q;->c()V

    iput-object v1, p0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    iput-object v1, p0, Lcom/kamcord/android/KC_S;->ab:Landroid/media/MediaPlayer;

    :cond_1
    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/KC_S;->ad:Z

    return-void
.end method

.method public reload()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/KC_S;->F()V

    return-void
.end method
