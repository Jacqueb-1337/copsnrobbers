.class final Lcom/kamcord/android/KC_S$KC_b;
.super Landroid/os/Handler;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_S;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = "KC_b"
.end annotation


# instance fields
.field private final a:Ljava/lang/ref/WeakReference;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/lang/ref/WeakReference",
            "<",
            "Lcom/kamcord/android/KC_S;",
            ">;"
        }
    .end annotation
.end field

.field private b:Z


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_S;)V
    .locals 1

    invoke-direct {p0}, Landroid/os/Handler;-><init>()V

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_S$KC_b;->b:Z

    new-instance v0, Ljava/lang/ref/WeakReference;

    invoke-direct {v0, p1}, Ljava/lang/ref/WeakReference;-><init>(Ljava/lang/Object;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_S$KC_b;->a:Ljava/lang/ref/WeakReference;

    return-void
.end method

.method private a()V
    .locals 4

    iget-object v0, p0, Lcom/kamcord/android/KC_S$KC_b;->a:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_S;

    iget-boolean v1, p0, Lcom/kamcord/android/KC_S$KC_b;->b:Z

    if-eqz v1, :cond_2

    iget-object v1, v0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    if-eqz v1, :cond_2

    iget-object v1, v0, Lcom/kamcord/android/KC_S;->O:Lcom/kamcord/android/AspectRatioTextureView;

    invoke-virtual {v1}, Lcom/kamcord/android/AspectRatioTextureView;->getSurfaceTexture()Landroid/graphics/SurfaceTexture;

    move-result-object v1

    if-eqz v1, :cond_2

    :try_start_0
    invoke-static {v0}, Lcom/kamcord/android/KC_S;->b(Lcom/kamcord/android/KC_S;)Landroid/media/MediaPlayer;

    move-result-object v1

    new-instance v2, Landroid/view/Surface;

    iget-object v3, v0, Lcom/kamcord/android/KC_S;->O:Lcom/kamcord/android/AspectRatioTextureView;

    invoke-virtual {v3}, Lcom/kamcord/android/AspectRatioTextureView;->getSurfaceTexture()Landroid/graphics/SurfaceTexture;

    move-result-object v3

    invoke-direct {v2, v3}, Landroid/view/Surface;-><init>(Landroid/graphics/SurfaceTexture;)V

    invoke-virtual {v1, v2}, Landroid/media/MediaPlayer;->setSurface(Landroid/view/Surface;)V
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    iget v1, v0, Lcom/kamcord/android/KC_S;->P:I

    if-lez v1, :cond_0

    iget-object v1, v0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    iget v2, v0, Lcom/kamcord/android/KC_S;->P:I

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KC_Q;->seekTo(I)V

    :cond_0
    invoke-static {v0}, Lcom/kamcord/android/KC_S;->c(Lcom/kamcord/android/KC_S;)Z

    move-result v1

    if-nez v1, :cond_1

    iget-object v1, v0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_Q;->start()V

    :cond_1
    const/4 v1, 0x0

    invoke-static {v0, v1}, Lcom/kamcord/android/KC_S;->a(Lcom/kamcord/android/KC_S;Z)Z

    invoke-static {v0}, Lcom/kamcord/android/KC_S;->a(Lcom/kamcord/android/KC_S;)Ljava/lang/ref/WeakReference;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v1}, Lcom/kamcord/android/KamcordActivity;->getTabBar()Lcom/kamcord/android/KC_q;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/android/KC_q;->c()La/a/a/c/KC_a;

    move-result-object v1

    invoke-static {v0}, Lcom/kamcord/android/KC_S;->d(Lcom/kamcord/android/KC_S;)La/a/a/c/KC_a;

    move-result-object v2

    if-eq v1, v2, :cond_3

    iget-object v0, v0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Q;->pause()V

    :goto_1
    const/4 v0, 0x4

    invoke-static {v0}, Lcom/kamcord/android/KC_m;->a(I)V

    :cond_2
    return-void

    :catch_0
    move-exception v1

    const-string v1, "Couldn\'t set the MediaPlayer surface, audio playback only, for now."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    goto :goto_0

    :cond_3
    iget-object v0, v0, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->a()V

    goto :goto_1
.end method


# virtual methods
.method public final handleMessage(Landroid/os/Message;)V
    .locals 2

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "handleMessage with msg.what: "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget v1, p1, Landroid/os/Message;->what:I

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget v0, p1, Landroid/os/Message;->what:I

    packed-switch v0, :pswitch_data_0

    :goto_0
    return-void

    :pswitch_0
    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_S$KC_b;->b:Z

    goto :goto_0

    :pswitch_1
    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/KC_S$KC_b;->b:Z

    :pswitch_2
    invoke-direct {p0}, Lcom/kamcord/android/KC_S$KC_b;->a()V

    goto :goto_0

    nop

    :pswitch_data_0
    .packed-switch 0x0
        :pswitch_0
        :pswitch_1
        :pswitch_2
    .end packed-switch
.end method
