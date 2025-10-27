.class abstract Lcom/kamcord/android/core/KC_J;
.super Lcom/kamcord/android/core/KC_I;

# interfaces
.implements Lcom/kamcord/android/core/KC_j;


# instance fields
.field b:Lcom/kamcord/android/core/KC_x;

.field protected c:Lcom/kamcord/android/core/KC_z;

.field private d:Landroid/media/MediaCodec;

.field private e:Landroid/media/MediaFormat;

.field private f:I

.field private g:I


# direct methods
.method constructor <init>(Lcom/kamcord/android/core/KC_x;)V
    .locals 1

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_I;-><init>()V

    iput-object p1, p0, Lcom/kamcord/android/core/KC_J;->b:Lcom/kamcord/android/core/KC_x;

    new-instance v0, Lcom/kamcord/android/core/KC_z;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_z;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_J;->c:Lcom/kamcord/android/core/KC_z;

    new-instance v0, Lcom/kamcord/android/core/KC_w;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_w;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_J;->a:Lcom/kamcord/android/core/KC_w;

    return-void
.end method


# virtual methods
.method public final a()V
    .locals 0

    return-void
.end method

.method public final a(Lcom/kamcord/android/core/KC_F;)V
    .locals 20

    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/core/KC_J;->e:Landroid/media/MediaFormat;

    const-string v3, "color-format"

    invoke-virtual {v2, v3}, Landroid/media/MediaFormat;->getInteger(Ljava/lang/String;)I

    move-result v2

    packed-switch v2, :pswitch_data_0

    :pswitch_0
    const-string v2, "Video codec input given unsupported color format."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    const/4 v2, 0x0

    move v15, v2

    :goto_0
    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/core/KC_J;->a:Lcom/kamcord/android/core/KC_w;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_w;->a()V

    move-object/from16 v0, p0

    iget-object v0, v0, Lcom/kamcord/android/core/KC_J;->d:Landroid/media/MediaCodec;

    move-object/from16 v16, v0

    monitor-enter v16

    :try_start_0
    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/core/KC_J;->d:Landroid/media/MediaCodec;

    invoke-virtual {v2}, Landroid/media/MediaCodec;->getInputBuffers()[Ljava/nio/ByteBuffer;

    move-result-object v2

    move-object/from16 v0, p0

    iget-object v3, v0, Lcom/kamcord/android/core/KC_J;->d:Landroid/media/MediaCodec;

    const-wide/16 v4, 0x3e8

    invoke-virtual {v3, v4, v5}, Landroid/media/MediaCodec;->dequeueInputBuffer(J)I

    move-result v17

    if-ltz v17, :cond_0

    aget-object v3, v2, v17

    move-object/from16 v0, p0

    iget v2, v0, Lcom/kamcord/android/core/KC_J;->f:I

    move-object/from16 v0, p0

    iget v4, v0, Lcom/kamcord/android/core/KC_J;->g:I

    if-ge v2, v4, :cond_1

    const/4 v2, 0x1

    move v14, v2

    :goto_1
    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/core/KC_J;->c:Lcom/kamcord/android/core/KC_z;

    iget v2, v2, Lcom/kamcord/android/core/KC_z;->b:I

    move-object/from16 v0, p0

    iget v4, v0, Lcom/kamcord/android/core/KC_J;->f:I

    move-object/from16 v0, p0

    iget v5, v0, Lcom/kamcord/android/core/KC_J;->g:I

    move-object/from16 v0, p0

    iget-object v6, v0, Lcom/kamcord/android/core/KC_J;->e:Landroid/media/MediaFormat;

    const-string v7, "color-format"

    invoke-virtual {v6, v7}, Landroid/media/MediaFormat;->getInteger(Ljava/lang/String;)I

    move-result v6

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v7

    invoke-virtual {v7}, Lcom/kamcord/android/a/KC_a;->i()Z

    move-result v7

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v8

    invoke-virtual {v8}, Lcom/kamcord/android/a/KC_a;->g()Z

    move-result v8

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v9

    invoke-virtual {v9, v14}, Lcom/kamcord/android/a/KC_a;->a(Z)I

    move-result v9

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v10

    invoke-virtual {v10, v14}, Lcom/kamcord/android/a/KC_a;->b(Z)I

    move-result v10

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v11

    move-object/from16 v0, p0

    iget v12, v0, Lcom/kamcord/android/core/KC_J;->f:I

    invoke-virtual {v11, v14, v12}, Lcom/kamcord/android/a/KC_a;->a(ZI)I

    move-result v11

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v12

    move-object/from16 v0, p0

    iget v13, v0, Lcom/kamcord/android/core/KC_J;->f:I

    invoke-virtual {v12, v14, v13}, Lcom/kamcord/android/a/KC_a;->b(ZI)I

    move-result v12

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v13

    move-object/from16 v0, p0

    iget v0, v0, Lcom/kamcord/android/core/KC_J;->f:I

    move/from16 v18, v0

    move/from16 v0, v18

    invoke-virtual {v13, v14, v0}, Lcom/kamcord/android/a/KC_a;->c(ZI)I

    move-result v13

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v18

    move-object/from16 v0, p0

    iget v0, v0, Lcom/kamcord/android/core/KC_J;->f:I

    move/from16 v19, v0

    move-object/from16 v0, v18

    move/from16 v1, v19

    invoke-virtual {v0, v14, v1}, Lcom/kamcord/android/a/KC_a;->d(ZI)I

    move-result v14

    invoke-static/range {v2 .. v14}, Lcom/kamcord/android/core/KamcordNative;->copyTextureToBuffer(ILjava/nio/ByteBuffer;IIIZZIIIIII)V

    move-object/from16 v0, p1

    iget-wide v2, v0, Lcom/kamcord/android/core/KC_F;->b:J

    move-object/from16 v0, p0

    invoke-virtual {v0, v2, v3}, Lcom/kamcord/android/core/KC_J;->a(J)J

    move-result-wide v10

    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/core/KC_J;->d:Landroid/media/MediaCodec;

    const/4 v4, 0x0

    const-wide/16 v6, 0x3e8

    div-long v6, v10, v6

    const/4 v8, 0x1

    move/from16 v3, v17

    move v5, v15

    invoke-virtual/range {v2 .. v8}, Landroid/media/MediaCodec;->queueInputBuffer(IIIJI)V

    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/core/KC_J;->a:Lcom/kamcord/android/core/KC_w;

    long-to-double v4, v10

    const-wide v6, 0x3e112e0be826d695L    # 1.0E-9

    mul-double/2addr v4, v6

    invoke-virtual {v2, v4, v5}, Lcom/kamcord/android/core/KC_w;->a(D)V

    :cond_0
    monitor-exit v16
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    return-void

    :pswitch_1
    move-object/from16 v0, p0

    iget v2, v0, Lcom/kamcord/android/core/KC_J;->f:I

    move-object/from16 v0, p0

    iget v3, v0, Lcom/kamcord/android/core/KC_J;->g:I

    mul-int/2addr v2, v3

    mul-int/lit8 v3, v2, 0x3

    div-int/lit8 v3, v3, 0x2

    rem-int/lit8 v2, v2, 0x2

    add-int/2addr v2, v3

    move v15, v2

    goto/16 :goto_0

    :cond_1
    const/4 v2, 0x0

    move v14, v2

    goto/16 :goto_1

    :catchall_0
    move-exception v2

    monitor-exit v16

    throw v2

    nop

    :pswitch_data_0
    .packed-switch 0x13
        :pswitch_1
        :pswitch_0
        :pswitch_1
    .end packed-switch
.end method

.method public final a(Lcom/kamcord/android/core/KC_h;Lcom/kamcord/android/core/KC_f;)Z
    .locals 11

    const/4 v10, 0x1

    const/high16 v9, 0x3f000000    # 0.5f

    const/high16 v8, 0x3f800000    # 1.0f

    const/4 v7, 0x0

    const/4 v6, 0x0

    iget-object v0, p1, Lcom/kamcord/android/core/KC_h;->a:Landroid/media/MediaCodec;

    iput-object v0, p0, Lcom/kamcord/android/core/KC_J;->d:Landroid/media/MediaCodec;

    iget-object v0, p1, Lcom/kamcord/android/core/KC_h;->b:Landroid/media/MediaFormat;

    iput-object v0, p0, Lcom/kamcord/android/core/KC_J;->e:Landroid/media/MediaFormat;

    iget-object v0, p1, Lcom/kamcord/android/core/KC_h;->c:Lcom/kamcord/android/core/KC_f;

    iget v0, v0, Lcom/kamcord/android/core/KC_f;->a:I

    iput v0, p0, Lcom/kamcord/android/core/KC_J;->f:I

    iget-object v0, p1, Lcom/kamcord/android/core/KC_h;->c:Lcom/kamcord/android/core/KC_f;

    iget v0, v0, Lcom/kamcord/android/core/KC_f;->b:I

    iput v0, p0, Lcom/kamcord/android/core/KC_J;->g:I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_J;->c:Lcom/kamcord/android/core/KC_z;

    iget-object v1, p1, Lcom/kamcord/android/core/KC_h;->c:Lcom/kamcord/android/core/KC_f;

    new-instance v2, Lcom/kamcord/android/core/KC_p;

    const/16 v3, 0x1908

    invoke-direct {v2, v6, v3, v6}, Lcom/kamcord/android/core/KC_p;-><init>(III)V

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/core/KC_z;->a(Lcom/kamcord/android/core/KC_f;Lcom/kamcord/android/core/KC_p;)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_J;->c:Lcom/kamcord/android/core/KC_z;

    iget-object v1, p1, Lcom/kamcord/android/core/KC_h;->c:Lcom/kamcord/android/core/KC_f;

    invoke-static {v6}, Lcom/kamcord/android/core/KC_s;->a(I)Lcom/kamcord/android/core/KC_v;

    move-result-object v2

    const/4 v3, 0x4

    new-array v3, v3, [F

    const/16 v4, 0xc22

    invoke-static {v4, v3, v6}, Landroid/opengl/GLES20;->glGetFloatv(I[FI)V

    const v4, 0x8d40

    iget v5, v0, Lcom/kamcord/android/core/KC_z;->a:I

    invoke-static {v4, v5}, Lcom/kamcord/android/core/KamcordNative;->glBindFramebuffer(II)V

    invoke-static {v8, v9, v9, v8}, Landroid/opengl/GLES20;->glClearColor(FFFF)V

    const/16 v4, 0x4000

    invoke-static {v4}, Landroid/opengl/GLES20;->glClear(I)V

    invoke-static {}, Landroid/opengl/GLES20;->glFinish()V

    iget v0, v0, Lcom/kamcord/android/core/KC_z;->b:I

    iget v4, v1, Lcom/kamcord/android/core/KC_f;->a:I

    iget v1, v1, Lcom/kamcord/android/core/KC_f;->b:I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v5

    invoke-virtual {v5}, Lcom/kamcord/android/a/KC_a;->g()Z

    move-result v5

    invoke-static {v0, v4, v1, v5}, Lcom/kamcord/android/core/KamcordNative;->computeGraphicBufferStride(IIIZ)V

    invoke-static {v7, v7, v7, v8}, Landroid/opengl/GLES20;->glClearColor(FFFF)V

    const/16 v0, 0x4000

    invoke-static {v0}, Landroid/opengl/GLES20;->glClear(I)V

    invoke-static {v2, v6}, Lcom/kamcord/android/core/KC_s;->a(Lcom/kamcord/android/core/KC_v;Z)V

    aget v0, v3, v6

    aget v1, v3, v10

    const/4 v2, 0x2

    aget v2, v3, v2

    const/4 v4, 0x3

    aget v3, v3, v4

    invoke-static {v0, v1, v2, v3}, Landroid/opengl/GLES20;->glClearColor(FFFF)V

    return v10
.end method

.method public final b()V
    .locals 0

    return-void
.end method

.method public final c()[I
    .locals 1

    const/4 v0, 0x2

    new-array v0, v0, [I

    fill-array-data v0, :array_0

    return-object v0

    nop

    :array_0
    .array-data 4
        0x13
        0x15
    .end array-data
.end method

.method d()V
    .locals 0

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_J;->h()V

    return-void
.end method

.method e()V
    .locals 0

    return-void
.end method

.method public final f()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_J;->c:Lcom/kamcord/android/core/KC_z;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_z;->a()V

    return-void
.end method

.method public final g()Z
    .locals 1

    const/4 v0, 0x1

    return v0
.end method
