.class final Lcom/kamcord/android/core/KC_a;
.super Ljava/lang/Object;

# interfaces
.implements Lcom/kamcord/android/core/KC_j;


# instance fields
.field private a:Lcom/kamcord/android/AudioSource;

.field private b:Landroid/media/MediaCodec;

.field private c:I

.field private d:I

.field private e:I

.field private f:Lcom/kamcord/android/core/KC_l;

.field private g:I

.field private volatile h:Z

.field private i:Lcom/kamcord/android/core/KC_w;


# direct methods
.method constructor <init>()V
    .locals 1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    new-instance v0, Lcom/kamcord/android/core/KC_w;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_w;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_a;->i:Lcom/kamcord/android/core/KC_w;

    return-void
.end method

.method private static a(Ljava/nio/ByteBuffer;I)V
    .locals 1

    invoke-virtual {p0}, Ljava/nio/ByteBuffer;->position()I

    move-result v0

    sub-int v0, p1, v0

    new-array v0, v0, [B

    invoke-virtual {p0, v0}, Ljava/nio/ByteBuffer;->put([B)Ljava/nio/ByteBuffer;

    return-void
.end method


# virtual methods
.method public final a()V
    .locals 2

    const/4 v0, 0x0

    iput v0, p0, Lcom/kamcord/android/core/KC_a;->g:I

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_a;->a:Lcom/kamcord/android/AudioSource;

    invoke-interface {v0}, Lcom/kamcord/android/AudioSource;->start()V
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    return-void

    :catch_0
    move-exception v0

    const-string v1, "Custom AudioSource exception on start."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    goto :goto_0
.end method

.method public final a(D)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_a;->i:Lcom/kamcord/android/core/KC_w;

    invoke-virtual {v0, p1, p2}, Lcom/kamcord/android/core/KC_w;->b(D)V

    return-void
.end method

.method public final a(J)V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_a;->f:Lcom/kamcord/android/core/KC_l;

    new-instance v1, Lcom/kamcord/android/core/KC_F;

    invoke-direct {v1, p1, p2}, Lcom/kamcord/android/core/KC_F;-><init>(J)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/core/KC_l;->a(Lcom/kamcord/android/core/KC_F;)V

    return-void
.end method

.method public final a(Lcom/kamcord/android/core/KC_F;)V
    .locals 20

    move-object/from16 v0, p0

    iget-object v3, v0, Lcom/kamcord/android/core/KC_a;->b:Landroid/media/MediaCodec;

    monitor-enter v3

    :try_start_0
    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/core/KC_a;->b:Landroid/media/MediaCodec;

    invoke-virtual {v2}, Landroid/media/MediaCodec;->getInputBuffers()[Ljava/nio/ByteBuffer;

    move-result-object v11

    monitor-exit v3
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    const/4 v9, 0x0

    const/4 v2, 0x0

    move-object/from16 v0, p0

    iget v3, v0, Lcom/kamcord/android/core/KC_a;->d:I

    move-object/from16 v0, p0

    iget v4, v0, Lcom/kamcord/android/core/KC_a;->e:I

    mul-int v12, v3, v4

    :try_start_1
    move-object/from16 v0, p0

    iget-object v3, v0, Lcom/kamcord/android/core/KC_a;->a:Lcom/kamcord/android/AudioSource;

    invoke-interface {v3}, Lcom/kamcord/android/AudioSource;->getNumAudioSamplesReady()I
    :try_end_1
    .catch Ljava/lang/RuntimeException; {:try_start_1 .. :try_end_1} :catch_0
    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_1

    move-result v13

    move-object/from16 v0, p0

    iget-boolean v3, v0, Lcom/kamcord/android/core/KC_a;->h:Z

    if-eqz v3, :cond_3

    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/core/KC_a;->a:Lcom/kamcord/android/AudioSource;

    invoke-interface {v2}, Lcom/kamcord/android/AudioSource;->skip()V

    :cond_0
    :goto_0
    return-void

    :catchall_0
    move-exception v2

    monitor-exit v3

    throw v2

    :catch_0
    move-exception v2

    const-string v3, "Custom AudioSource runtime exception on start."

    invoke-static {v3}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v2}, Ljava/lang/RuntimeException;->printStackTrace()V

    goto :goto_0

    :catch_1
    move-exception v2

    const-string v3, "Custom AudioSource exception on start."

    invoke-static {v3}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v2}, Ljava/lang/Exception;->printStackTrace()V

    goto :goto_0

    :goto_1
    if-ge v9, v13, :cond_0

    const/16 v2, 0xa

    if-ge v10, v2, :cond_0

    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/core/KC_a;->i:Lcom/kamcord/android/core/KC_w;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_w;->a()V

    move-object/from16 v0, p0

    iget-object v14, v0, Lcom/kamcord/android/core/KC_a;->b:Landroid/media/MediaCodec;

    monitor-enter v14

    :try_start_2
    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/core/KC_a;->b:Landroid/media/MediaCodec;

    const-wide/16 v4, 0x3e8

    invoke-virtual {v2, v4, v5}, Landroid/media/MediaCodec;->dequeueInputBuffer(J)I

    move-result v3

    if-ltz v3, :cond_2

    aget-object v6, v11, v3

    sub-int v2, v13, v9

    mul-int v4, v2, v12

    invoke-virtual {v6}, Ljava/nio/ByteBuffer;->capacity()I

    move-result v2

    if-le v4, v2, :cond_1

    :goto_2
    div-int v15, v2, v12
    :try_end_2
    .catchall {:try_start_2 .. :try_end_2} :catchall_1

    mul-int v5, v15, v12

    :try_start_3
    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/core/KC_a;->a:Lcom/kamcord/android/AudioSource;

    invoke-interface {v2, v6, v15}, Lcom/kamcord/android/AudioSource;->obtainAudioSamples(Ljava/nio/ByteBuffer;I)V
    :try_end_3
    .catch Ljava/lang/RuntimeException; {:try_start_3 .. :try_end_3} :catch_2
    .catch Ljava/lang/Exception; {:try_start_3 .. :try_end_3} :catch_3
    .catchall {:try_start_3 .. :try_end_3} :catchall_1

    :goto_3
    :try_start_4
    invoke-virtual/range {p0 .. p0}, Lcom/kamcord/android/core/KC_a;->c()J

    move-result-wide v16

    const-wide/16 v6, 0x3e8

    div-long v6, v16, v6

    move-wide/from16 v0, v16

    long-to-double v0, v0

    move-wide/from16 v16, v0

    const-wide v18, 0x3e112e0be826d695L    # 1.0E-9

    mul-double v16, v16, v18

    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/core/KC_a;->b:Landroid/media/MediaCodec;

    const/4 v4, 0x0

    const/4 v8, 0x0

    invoke-virtual/range {v2 .. v8}, Landroid/media/MediaCodec;->queueInputBuffer(IIIJI)V

    move-object/from16 v0, p0

    iget v2, v0, Lcom/kamcord/android/core/KC_a;->g:I

    add-int/2addr v2, v15

    move-object/from16 v0, p0

    iput v2, v0, Lcom/kamcord/android/core/KC_a;->g:I

    add-int v2, v9, v15

    move-object/from16 v0, p0

    iget-object v3, v0, Lcom/kamcord/android/core/KC_a;->i:Lcom/kamcord/android/core/KC_w;

    move-wide/from16 v0, v16

    invoke-virtual {v3, v0, v1}, Lcom/kamcord/android/core/KC_w;->a(D)V

    :goto_4
    add-int/lit8 v3, v10, 0x1

    monitor-exit v14

    move v10, v3

    move v9, v2

    goto :goto_1

    :catch_2
    move-exception v2

    const-string v4, "Custom AudioSource runtime exception while obtaining samples."

    invoke-static {v4}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v2}, Ljava/lang/RuntimeException;->printStackTrace()V

    invoke-static {v6, v5}, Lcom/kamcord/android/core/KC_a;->a(Ljava/nio/ByteBuffer;I)V
    :try_end_4
    .catchall {:try_start_4 .. :try_end_4} :catchall_1

    goto :goto_3

    :catchall_1
    move-exception v2

    monitor-exit v14

    throw v2

    :catch_3
    move-exception v2

    :try_start_5
    const-string v4, "Custom AudioSource exception while obtaining samples."

    invoke-static {v4}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v2}, Ljava/lang/Exception;->printStackTrace()V

    invoke-static {v6, v5}, Lcom/kamcord/android/core/KC_a;->a(Ljava/nio/ByteBuffer;I)V
    :try_end_5
    .catchall {:try_start_5 .. :try_end_5} :catchall_1

    goto :goto_3

    :cond_1
    move v2, v4

    goto :goto_2

    :cond_2
    move v2, v9

    goto :goto_4

    :cond_3
    move v10, v2

    goto/16 :goto_1
.end method

.method public final a(Lcom/kamcord/android/core/KC_h;Lcom/kamcord/android/AudioSource;)Z
    .locals 3

    const/4 v0, 0x0

    iget-object v1, p1, Lcom/kamcord/android/core/KC_h;->a:Landroid/media/MediaCodec;

    iput-object v1, p0, Lcom/kamcord/android/core/KC_a;->b:Landroid/media/MediaCodec;

    iput-object p2, p0, Lcom/kamcord/android/core/KC_a;->a:Lcom/kamcord/android/AudioSource;

    :try_start_0
    iget-object v1, p0, Lcom/kamcord/android/core/KC_a;->a:Lcom/kamcord/android/AudioSource;

    invoke-interface {v1}, Lcom/kamcord/android/AudioSource;->getSampleRate()I

    move-result v1

    iput v1, p0, Lcom/kamcord/android/core/KC_a;->c:I

    iget-object v1, p0, Lcom/kamcord/android/core/KC_a;->a:Lcom/kamcord/android/AudioSource;

    invoke-interface {v1}, Lcom/kamcord/android/AudioSource;->getNumChannels()I

    move-result v1

    iput v1, p0, Lcom/kamcord/android/core/KC_a;->d:I

    iget-object v1, p0, Lcom/kamcord/android/core/KC_a;->a:Lcom/kamcord/android/AudioSource;

    invoke-interface {v1}, Lcom/kamcord/android/AudioSource;->getNumBytesPerChannel()I

    move-result v1

    iput v1, p0, Lcom/kamcord/android/core/KC_a;->e:I
    :try_end_0
    .catch Ljava/lang/RuntimeException; {:try_start_0 .. :try_end_0} :catch_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_1

    const/4 v0, 0x1

    :goto_0
    return v0

    :catch_0
    move-exception v1

    const-string v2, "Custom AudioSource runtime-exception on init."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v1}, Ljava/lang/RuntimeException;->printStackTrace()V

    goto :goto_0

    :catch_1
    move-exception v1

    const-string v2, "Custom AudioSource exception on init."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v1}, Ljava/lang/Exception;->printStackTrace()V

    goto :goto_0
.end method

.method public final b()V
    .locals 2

    const/4 v0, 0x0

    iput v0, p0, Lcom/kamcord/android/core/KC_a;->g:I

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_a;->a:Lcom/kamcord/android/AudioSource;

    invoke-interface {v0}, Lcom/kamcord/android/AudioSource;->stop()V
    :try_end_0
    .catch Ljava/lang/RuntimeException; {:try_start_0 .. :try_end_0} :catch_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_1

    :goto_0
    return-void

    :catch_0
    move-exception v0

    const-string v1, "Custom AudioSource runtime-exception on stop."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/RuntimeException;->printStackTrace()V

    goto :goto_0

    :catch_1
    move-exception v0

    const-string v1, "Custom AudioSource exception on stop."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    goto :goto_0
.end method

.method public final c()J
    .locals 4

    const-wide v0, 0x41cdcd6500000000L    # 1.0E9

    iget v2, p0, Lcom/kamcord/android/core/KC_a;->g:I

    int-to-double v2, v2

    mul-double/2addr v0, v2

    iget v2, p0, Lcom/kamcord/android/core/KC_a;->c:I

    int-to-double v2, v2

    div-double/2addr v0, v2

    double-to-long v0, v0

    return-wide v0
.end method

.method public final d()V
    .locals 1

    new-instance v0, Lcom/kamcord/android/core/KC_l;

    invoke-direct {v0, p0}, Lcom/kamcord/android/core/KC_l;-><init>(Lcom/kamcord/android/core/KC_j;)V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_a;->f:Lcom/kamcord/android/core/KC_l;

    iget-object v0, p0, Lcom/kamcord/android/core/KC_a;->f:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->start()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_a;->f:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->e()V

    return-void
.end method

.method public final e()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_a;->f:Lcom/kamcord/android/core/KC_l;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_a;->f:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->isAlive()Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_a;->f:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->f()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_a;->f:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->d()V

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_a;->f:Lcom/kamcord/android/core/KC_l;

    :cond_0
    return-void
.end method

.method public final f()V
    .locals 1

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_a;->h:Z

    return-void
.end method

.method public final g()V
    .locals 1

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_a;->h:Z

    return-void
.end method
