.class public final Lcom/kamcord/android/KC_L;
.super Lcom/kamcord/android/KC_Z;

# interfaces
.implements Landroid/media/MediaPlayer$OnBufferingUpdateListener;
.implements Landroid/media/MediaPlayer$OnCompletionListener;
.implements Landroid/media/MediaPlayer$OnErrorListener;
.implements Landroid/media/MediaPlayer$OnInfoListener;
.implements Landroid/media/MediaPlayer$OnPreparedListener;
.implements Landroid/media/MediaPlayer$OnSeekCompleteListener;
.implements Landroid/media/MediaPlayer$OnVideoSizeChangedListener;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_L$KC_a;
    }
.end annotation


# instance fields
.field private M:Landroid/os/Handler;

.field private U:Ljava/lang/String;

.field private V:Ljava/lang/String;

.field private W:Landroid/media/MediaPlayer;

.field private X:Landroid/media/MediaPlayer;

.field private Y:Lcom/kamcord/android/KC_K;

.field private Z:Z


# direct methods
.method public constructor <init>()V
    .locals 1

    const/4 v0, 0x0

    invoke-direct {p0}, Lcom/kamcord/android/KC_Z;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_L;->V:Ljava/lang/String;

    iput-object v0, p0, Lcom/kamcord/android/KC_L;->X:Landroid/media/MediaPlayer;

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_L;->Z:Z

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_L;)Landroid/media/MediaPlayer;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->W:Landroid/media/MediaPlayer;

    return-object v0
.end method

.method static synthetic a(Lcom/kamcord/android/KC_L;Z)Z
    .locals 1

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_L;->Z:Z

    return v0
.end method

.method static synthetic b(Lcom/kamcord/android/KC_L;)Lcom/kamcord/android/KC_K;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    return-object v0
.end method

.method static synthetic c(Lcom/kamcord/android/KC_L;)Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/KC_L;->Z:Z

    return v0
.end method


# virtual methods
.method public final a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 4

    invoke-super {p0, p1, p2, p3}, Lcom/kamcord/android/KC_Z;->a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;

    move-result-object v0

    :try_start_0
    iget-object v1, p0, Lcom/kamcord/android/KC_L;->T:Lorg/json/JSONObject;

    const-string v2, "video_url"

    invoke-virtual {v1, v2}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-static {v1}, Lcom/kamcord/android/KC_L;->a(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    iput-object v1, p0, Lcom/kamcord/android/KC_L;->U:Ljava/lang/String;

    iget-object v1, p0, Lcom/kamcord/android/KC_L;->T:Lorg/json/JSONObject;

    const-string v2, "voice_path"

    invoke-virtual {v1, v2}, Lorg/json/JSONObject;->has(Ljava/lang/String;)Z

    move-result v1

    if-eqz v1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/KC_L;->T:Lorg/json/JSONObject;

    const-string v2, "voice_path"

    invoke-virtual {v1, v2}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    iput-object v1, p0, Lcom/kamcord/android/KC_L;->V:Ljava/lang/String;
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    :cond_0
    iget-object v1, p0, Lcom/kamcord/android/KC_L;->S:Lcom/kamcord/android/CustomWebView;

    const/16 v2, 0x8

    invoke-virtual {v1, v2}, Lcom/kamcord/android/CustomWebView;->setVisibility(I)V

    iget-object v1, p0, Lcom/kamcord/android/KC_L;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_w;->h()V

    :goto_0
    return-object v0

    :catch_0
    move-exception v1

    invoke-virtual {v1}, Lorg/json/JSONException;->printStackTrace()V

    new-instance v1, Lcom/kamcord/android/KC_j;

    const-string v2, "kamcordNoVideoAvailable"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    invoke-direct {v1, v2}, Lcom/kamcord/android/KC_j;-><init>(Ljava/lang/String;)V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_L;->h()La/a/a/a/KC_f;

    move-result-object v2

    invoke-virtual {v2}, La/a/a/a/KC_f;->getSupportFragmentManager()La/a/a/a/KC_h;

    move-result-object v2

    const-string v3, ""

    invoke-virtual {v1, v2, v3}, La/a/a/a/KC_d;->a(La/a/a/a/KC_h;Ljava/lang/String;)V

    goto :goto_0
.end method

.method public final o()V
    .locals 3

    invoke-super {p0}, Lcom/kamcord/android/KC_Z;->o()V

    new-instance v0, Lcom/kamcord/android/KC_L$KC_a;

    invoke-direct {v0, p0}, Lcom/kamcord/android/KC_L$KC_a;-><init>(Lcom/kamcord/android/KC_L;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_L;->M:Landroid/os/Handler;

    new-instance v0, Landroid/media/MediaPlayer;

    invoke-direct {v0}, Landroid/media/MediaPlayer;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_L;->W:Landroid/media/MediaPlayer;

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->W:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnBufferingUpdateListener(Landroid/media/MediaPlayer$OnBufferingUpdateListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->W:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnCompletionListener(Landroid/media/MediaPlayer$OnCompletionListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->W:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnErrorListener(Landroid/media/MediaPlayer$OnErrorListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->W:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnInfoListener(Landroid/media/MediaPlayer$OnInfoListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->W:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnPreparedListener(Landroid/media/MediaPlayer$OnPreparedListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->W:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnSeekCompleteListener(Landroid/media/MediaPlayer$OnSeekCompleteListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->W:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnVideoSizeChangedListener(Landroid/media/MediaPlayer$OnVideoSizeChangedListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->V:Ljava/lang/String;

    if-eqz v0, :cond_0

    new-instance v0, Landroid/media/MediaPlayer;

    invoke-direct {v0}, Landroid/media/MediaPlayer;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_L;->X:Landroid/media/MediaPlayer;

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->X:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnBufferingUpdateListener(Landroid/media/MediaPlayer$OnBufferingUpdateListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->X:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnCompletionListener(Landroid/media/MediaPlayer$OnCompletionListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->X:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnErrorListener(Landroid/media/MediaPlayer$OnErrorListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->X:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnInfoListener(Landroid/media/MediaPlayer$OnInfoListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->X:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnPreparedListener(Landroid/media/MediaPlayer$OnPreparedListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->X:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnSeekCompleteListener(Landroid/media/MediaPlayer$OnSeekCompleteListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->X:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p0}, Landroid/media/MediaPlayer;->setOnVideoSizeChangedListener(Landroid/media/MediaPlayer$OnVideoSizeChangedListener;)V

    :cond_0
    new-instance v0, Lcom/kamcord/android/KC_K;

    iget-object v1, p0, Lcom/kamcord/android/KC_L;->W:Landroid/media/MediaPlayer;

    iget-object v2, p0, Lcom/kamcord/android/KC_L;->X:Landroid/media/MediaPlayer;

    invoke-direct {v0, v1, v2}, Lcom/kamcord/android/KC_K;-><init>(Landroid/media/MediaPlayer;Landroid/media/MediaPlayer;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->N:Lcom/kamcord/android/KC_w;

    iget-object v1, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_w;->a(Lcom/kamcord/android/KC_E;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->N:Lcom/kamcord/android/KC_w;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_w;->setEnabled(Z)V

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    iget-object v1, p0, Lcom/kamcord/android/KC_L;->U:Ljava/lang/String;

    iget-object v2, p0, Lcom/kamcord/android/KC_L;->V:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_K;->a(Ljava/lang/String;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_K;->d()V
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

.method public final onBufferingUpdate(Landroid/media/MediaPlayer;I)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->e()I

    return-void
.end method

.method public final onCompletion(Landroid/media/MediaPlayer;)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_K;->f()V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_L;->h()La/a/a/a/KC_f;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/ReplayActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/ReplayActivity;->getBackPressed()Z

    move-result v1

    if-nez v1, :cond_0

    invoke-virtual {v0}, Lcom/kamcord/android/ReplayActivity;->onBackPressed()V

    :cond_0
    return-void
.end method

.method public final onError(Landroid/media/MediaPlayer;II)Z
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    invoke-virtual {v0, p2}, Lcom/kamcord/android/KC_K;->a(I)Z

    move-result v0

    return v0
.end method

.method public final onInfo(Landroid/media/MediaPlayer;II)Z
    .locals 1

    packed-switch p2, :pswitch_data_0

    :goto_0
    const/4 v0, 0x0

    return v0

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Q:Landroid/widget/FrameLayout;

    invoke-static {v0}, La/a/a/c/KC_a;->a(Landroid/view/ViewGroup;)V

    goto :goto_0

    :pswitch_1
    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Q:Landroid/widget/FrameLayout;

    invoke-static {v0}, La/a/a/c/KC_a;->b(Landroid/view/ViewGroup;)V

    goto :goto_0

    nop

    :pswitch_data_0
    .packed-switch 0x2bd
        :pswitch_0
        :pswitch_1
    .end packed-switch
.end method

.method public final onPrepared(Landroid/media/MediaPlayer;)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    invoke-virtual {v0, p1}, Lcom/kamcord/android/KC_K;->a(Landroid/media/MediaPlayer;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_K;->e()Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->M:Landroid/os/Handler;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Landroid/os/Handler;->sendEmptyMessage(I)Z

    :cond_0
    return-void
.end method

.method public final onSeekComplete(Landroid/media/MediaPlayer;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_K;->g()V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->e()I

    return-void
.end method

.method public final onSurfaceTextureAvailable(Landroid/graphics/SurfaceTexture;II)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->M:Landroid/os/Handler;

    const/4 v1, 0x2

    invoke-virtual {v0, v1}, Landroid/os/Handler;->sendEmptyMessage(I)Z

    return-void
.end method

.method public final onSurfaceTextureDestroyed(Landroid/graphics/SurfaceTexture;)Z
    .locals 1

    const/4 v0, 0x0

    return v0
.end method

.method public final onSurfaceTextureSizeChanged(Landroid/graphics/SurfaceTexture;II)V
    .locals 0

    return-void
.end method

.method public final onSurfaceTextureUpdated(Landroid/graphics/SurfaceTexture;)V
    .locals 0

    return-void
.end method

.method public final onVideoSizeChanged(Landroid/media/MediaPlayer;II)V
    .locals 0

    return-void
.end method

.method public final p()V
    .locals 2

    const/4 v1, 0x0

    invoke-super {p0}, Lcom/kamcord/android/KC_Z;->p()V

    iput-object v1, p0, Lcom/kamcord/android/KC_L;->M:Landroid/os/Handler;

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->N:Lcom/kamcord/android/KC_w;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->f()V

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_K;->getCurrentPosition()I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/KC_L;->P:I

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_K;->c()V

    iget-object v0, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_K;->b()V

    iput-object v1, p0, Lcom/kamcord/android/KC_L;->Y:Lcom/kamcord/android/KC_K;

    iput-object v1, p0, Lcom/kamcord/android/KC_L;->W:Landroid/media/MediaPlayer;

    iput-object v1, p0, Lcom/kamcord/android/KC_L;->X:Landroid/media/MediaPlayer;

    :cond_1
    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/KC_L;->Z:Z

    return-void
.end method
