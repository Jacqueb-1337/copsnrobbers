.class final Lcom/kamcord/android/core/KC_T;
.super Ljava/lang/Object;


# instance fields
.field a:Lcom/kamcord/android/core/KC_I;

.field b:Lcom/kamcord/android/core/KC_a;

.field c:Lcom/kamcord/android/core/KC_U;

.field private d:Lcom/kamcord/android/AudioSource;

.field private e:Landroid/media/MediaFormat;

.field private f:Landroid/media/MediaFormat;

.field private g:Landroid/media/MediaCodec;

.field private h:Landroid/media/MediaCodec;

.field private i:Lcom/kamcord/android/core/KC_S;

.field private j:J

.field private volatile k:Z

.field private volatile l:Z

.field private volatile m:Z

.field private volatile n:Z

.field private o:Lcom/kamcord/android/core/KC_e;

.field private p:Landroid/media/MediaFormat;

.field private q:Landroid/media/MediaFormat;

.field private r:Z


# direct methods
.method constructor <init>()V
    .locals 1

    const/4 v0, 0x0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_T;->k:Z

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_T;->l:Z

    return-void
.end method

.method private a(Ljava/lang/String;ZII[ILcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/core/KC_h;
    .locals 12

    if-nez p2, :cond_1

    const/4 v1, 0x1

    move v6, v1

    :goto_0
    if-eqz p2, :cond_6

    new-instance v3, Ljava/util/ArrayList;

    invoke-direct {v3}, Ljava/util/ArrayList;-><init>()V

    invoke-static {}, Lcom/kamcord/android/core/KC_T;->k()[Landroid/media/MediaCodecInfo;

    move-result-object v4

    const/4 v1, 0x0

    :goto_1
    array-length v2, v4

    if-ge v1, v2, :cond_3

    aget-object v2, v4, v1

    invoke-virtual {v2}, Landroid/media/MediaCodecInfo;->isEncoder()Z

    move-result v5

    if-eqz v5, :cond_2

    invoke-virtual {v2}, Landroid/media/MediaCodecInfo;->getName()Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v2}, Landroid/media/MediaCodecInfo;->getSupportedTypes()[Ljava/lang/String;

    move-result-object v7

    const/4 v2, 0x0

    :goto_2
    array-length v8, v7

    if-ge v2, v8, :cond_2

    aget-object v8, v7, v2

    const-string v9, "audio"

    invoke-virtual {v8, v9}, Ljava/lang/String;->startsWith(Ljava/lang/String;)Z

    move-result v9

    if-eqz v9, :cond_0

    new-instance v9, Lcom/kamcord/android/core/KC_c;

    const/4 v10, 0x0

    const/4 v11, 0x0

    invoke-direct {v9, v5, v8, v10, v11}, Lcom/kamcord/android/core/KC_c;-><init>(Ljava/lang/String;Ljava/lang/String;ILcom/kamcord/android/core/KC_f;)V

    invoke-interface {v3, v9}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    :cond_0
    add-int/lit8 v2, v2, 0x1

    goto :goto_2

    :cond_1
    const/4 v1, 0x0

    move v6, v1

    goto :goto_0

    :cond_2
    add-int/lit8 v1, v1, 0x1

    goto :goto_1

    :cond_3
    move-object v1, v3

    :goto_3
    invoke-interface {v1}, Ljava/util/List;->iterator()Ljava/util/Iterator;

    move-result-object v7

    :cond_4
    :goto_4
    invoke-interface {v7}, Ljava/util/Iterator;->hasNext()Z

    move-result v1

    if-eqz v1, :cond_d

    invoke-interface {v7}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Lcom/kamcord/android/core/KC_c;

    const/4 v4, 0x0

    const/4 v2, 0x0

    if-eqz p2, :cond_7

    iget-object v3, v1, Lcom/kamcord/android/core/KC_c;->b:Ljava/lang/String;

    invoke-virtual {p1, v3}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v3

    if-eqz v3, :cond_7

    iget-object v2, v1, Lcom/kamcord/android/core/KC_c;->b:Ljava/lang/String;

    move/from16 v0, p4

    invoke-static {v2, p3, v0}, Landroid/media/MediaFormat;->createAudioFormat(Ljava/lang/String;II)Landroid/media/MediaFormat;

    move-result-object v2

    const-string v3, "bitrate"

    const v5, 0xbb80

    invoke-virtual {v2, v3, v5}, Landroid/media/MediaFormat;->setInteger(Ljava/lang/String;I)V

    const-string v3, "channel-count"

    move/from16 v0, p4

    invoke-virtual {v2, v3, v0}, Landroid/media/MediaFormat;->setInteger(Ljava/lang/String;I)V

    const-string v3, "aac-profile"

    const/4 v5, 0x2

    invoke-virtual {v2, v3, v5}, Landroid/media/MediaFormat;->setInteger(Ljava/lang/String;I)V

    move-object v5, v2

    :goto_5
    if-eqz v5, :cond_4

    if-eqz p2, :cond_b

    :try_start_0
    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v2

    invoke-virtual {v2}, Lcom/kamcord/android/a/KC_a;->e()Z

    move-result v2

    if-eqz v2, :cond_a

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v2

    invoke-virtual {v2}, Lcom/kamcord/android/a/KC_a;->j()Ljava/lang/String;

    move-result-object v2

    if-eqz v2, :cond_9

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v2

    invoke-virtual {v2}, Lcom/kamcord/android/a/KC_a;->j()Ljava/lang/String;

    move-result-object v2

    move-object v3, v2

    :goto_6
    iget-object v2, v1, Lcom/kamcord/android/core/KC_c;->a:Ljava/lang/String;

    invoke-static {v2}, Landroid/media/MediaCodec;->createByCodecName(Ljava/lang/String;)Landroid/media/MediaCodec;
    :try_end_0
    .catch Ljava/lang/RuntimeException; {:try_start_0 .. :try_end_0} :catch_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_1

    move-result-object v2

    :try_start_1
    sput-object v3, Lcom/kamcord/android/KC_Y;->a:Ljava/lang/String;
    :try_end_1
    .catch Ljava/lang/RuntimeException; {:try_start_1 .. :try_end_1} :catch_4
    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_2

    move-object v3, v2

    :goto_7
    const/4 v2, 0x0

    const/4 v4, 0x0

    const/4 v8, 0x1

    :try_start_2
    invoke-virtual {v3, v5, v2, v4, v8}, Landroid/media/MediaCodec;->configure(Landroid/media/MediaFormat;Landroid/view/Surface;Landroid/media/MediaCrypto;I)V

    if-eqz v6, :cond_5

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v4, "Created video codec with a resolution of "

    invoke-direct {v2, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v4, v1, Lcom/kamcord/android/core/KC_c;->d:Lcom/kamcord/android/core/KC_f;

    iget v4, v4, Lcom/kamcord/android/core/KC_f;->a:I

    invoke-virtual {v2, v4}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v4, "x"

    invoke-virtual {v2, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    iget-object v4, v1, Lcom/kamcord/android/core/KC_c;->d:Lcom/kamcord/android/core/KC_f;

    iget v4, v4, Lcom/kamcord/android/core/KC_f;->b:I

    invoke-virtual {v2, v4}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    :cond_5
    new-instance v2, Lcom/kamcord/android/core/KC_h;

    iget-object v1, v1, Lcom/kamcord/android/core/KC_c;->d:Lcom/kamcord/android/core/KC_f;

    invoke-direct {v2, v3, v5, v1}, Lcom/kamcord/android/core/KC_h;-><init>(Landroid/media/MediaCodec;Landroid/media/MediaFormat;Lcom/kamcord/android/core/KC_f;)V
    :try_end_2
    .catch Ljava/lang/RuntimeException; {:try_start_2 .. :try_end_2} :catch_5
    .catch Ljava/lang/Exception; {:try_start_2 .. :try_end_2} :catch_3

    move-object v1, v2

    :goto_8
    return-object v1

    :cond_6
    move-object/from16 v0, p6

    invoke-direct {p0, v0}, Lcom/kamcord/android/core/KC_T;->a(Lcom/kamcord/android/core/KC_f;)Ljava/util/List;

    move-result-object v1

    goto/16 :goto_3

    :cond_7
    if-eqz v6, :cond_e

    move-object/from16 v0, p5

    invoke-virtual {v1, p1, v0}, Lcom/kamcord/android/core/KC_c;->a(Ljava/lang/String;[I)Z

    move-result v3

    if-eqz v3, :cond_e

    iget-object v8, v1, Lcom/kamcord/android/core/KC_c;->a:Ljava/lang/String;

    iget-object v5, v1, Lcom/kamcord/android/core/KC_c;->b:Ljava/lang/String;

    iget-object v2, v1, Lcom/kamcord/android/core/KC_c;->d:Lcom/kamcord/android/core/KC_f;

    iget v3, v2, Lcom/kamcord/android/core/KC_f;->a:I

    iget-object v2, v1, Lcom/kamcord/android/core/KC_c;->d:Lcom/kamcord/android/core/KC_f;

    iget v2, v2, Lcom/kamcord/android/core/KC_f;->b:I

    invoke-static {v5, v3, v2}, Landroid/media/MediaFormat;->createVideoFormat(Ljava/lang/String;II)Landroid/media/MediaFormat;

    move-result-object v5

    const-string v9, "bitrate"

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getVideoBitRate()I

    move-result v10

    invoke-virtual {v5, v9, v10}, Landroid/media/MediaFormat;->setInteger(Ljava/lang/String;I)V

    const-string v9, "frame-rate"

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getVideoFrameRate()F

    move-result v10

    invoke-virtual {v5, v9, v10}, Landroid/media/MediaFormat;->setFloat(Ljava/lang/String;F)V

    const-string v9, "i-frame-interval"

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getVideoFrameInterval()I

    move-result v10

    invoke-virtual {v5, v9, v10}, Landroid/media/MediaFormat;->setInteger(Ljava/lang/String;I)V

    const-string v9, "color-format"

    const v10, 0x7f000789

    invoke-virtual {v5, v9, v10}, Landroid/media/MediaFormat;->setInteger(Ljava/lang/String;I)V

    const-string v9, "OMX.Nvidia."

    invoke-virtual {v8, v9}, Ljava/lang/String;->startsWith(Ljava/lang/String;)Z

    move-result v8

    if-eqz v8, :cond_8

    add-int/lit8 v3, v3, 0xf

    div-int/lit8 v3, v3, 0x10

    shl-int/lit8 v3, v3, 0x4

    add-int/lit8 v2, v2, 0xf

    div-int/lit8 v2, v2, 0x10

    shl-int/lit8 v2, v2, 0x4

    :cond_8
    const-string v8, "stride"

    invoke-virtual {v5, v8, v3}, Landroid/media/MediaFormat;->setInteger(Ljava/lang/String;I)V

    const-string v3, "slice-height"

    invoke-virtual {v5, v3, v2}, Landroid/media/MediaFormat;->setInteger(Ljava/lang/String;I)V

    const-string v2, "color-format"

    iget v3, v1, Lcom/kamcord/android/core/KC_c;->c:I

    invoke-virtual {v5, v2, v3}, Landroid/media/MediaFormat;->setInteger(Ljava/lang/String;I)V

    goto/16 :goto_5

    :cond_9
    :try_start_3
    iget-object v2, v1, Lcom/kamcord/android/core/KC_c;->a:Ljava/lang/String;

    move-object v3, v2

    goto/16 :goto_6

    :cond_a
    const-string v2, "audio/mp4a-latm"

    invoke-static {v2}, Landroid/media/MediaCodec;->createEncoderByType(Ljava/lang/String;)Landroid/media/MediaCodec;

    move-result-object v3

    goto/16 :goto_7

    :cond_b
    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v2

    invoke-virtual {v2}, Lcom/kamcord/android/a/KC_a;->f()Z

    move-result v2

    if-eqz v2, :cond_c

    iget-object v2, v1, Lcom/kamcord/android/core/KC_c;->a:Ljava/lang/String;

    invoke-static {v2}, Landroid/media/MediaCodec;->createByCodecName(Ljava/lang/String;)Landroid/media/MediaCodec;

    move-result-object v3

    goto/16 :goto_7

    :cond_c
    const-string v2, "video/avc"

    invoke-static {v2}, Landroid/media/MediaCodec;->createEncoderByType(Ljava/lang/String;)Landroid/media/MediaCodec;
    :try_end_3
    .catch Ljava/lang/RuntimeException; {:try_start_3 .. :try_end_3} :catch_0
    .catch Ljava/lang/Exception; {:try_start_3 .. :try_end_3} :catch_1

    move-result-object v3

    goto/16 :goto_7

    :catch_0
    move-exception v1

    move-object v1, v4

    :goto_9
    if-eqz v1, :cond_4

    invoke-virtual {v1}, Landroid/media/MediaCodec;->release()V

    goto/16 :goto_4

    :catch_1
    move-exception v1

    :goto_a
    if-eqz v4, :cond_4

    invoke-virtual {v4}, Landroid/media/MediaCodec;->release()V

    goto/16 :goto_4

    :cond_d
    const/4 v1, 0x0

    goto/16 :goto_8

    :catch_2
    move-exception v1

    move-object v4, v2

    goto :goto_a

    :catch_3
    move-exception v1

    move-object v4, v3

    goto :goto_a

    :catch_4
    move-exception v1

    move-object v1, v2

    goto :goto_9

    :catch_5
    move-exception v1

    move-object v1, v3

    goto :goto_9

    :cond_e
    move-object v5, v2

    goto/16 :goto_5
.end method

.method private a(Lcom/kamcord/android/core/KC_f;)Ljava/util/List;
    .locals 14
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Lcom/kamcord/android/core/KC_f;",
            ")",
            "Ljava/util/List",
            "<",
            "Lcom/kamcord/android/core/KC_c;",
            ">;"
        }
    .end annotation

    new-instance v4, Ljava/util/ArrayList;

    invoke-direct {v4}, Ljava/util/ArrayList;-><init>()V

    const/4 v0, 0x0

    if-eqz p1, :cond_0

    :try_start_0
    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v0

    invoke-virtual {v0, p1}, Lcom/kamcord/android/a/KC_a;->a(Lcom/kamcord/android/core/KC_f;)Ljava/util/List;
    :try_end_0
    .catch Ljava/lang/RuntimeException; {:try_start_0 .. :try_end_0} :catch_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_1

    move-result-object v0

    :cond_0
    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/android/a/KC_a;->k()Lcom/kamcord/android/core/KC_c;

    move-result-object v1

    if-eqz v1, :cond_2

    if-eqz v0, :cond_1

    invoke-static {v1, v0, v4}, Lcom/kamcord/android/core/KC_T;->a(Lcom/kamcord/android/core/KC_c;Ljava/util/List;Ljava/util/List;)V

    :cond_1
    move-object v0, v4

    :goto_0
    return-object v0

    :catch_0
    move-exception v0

    const/4 v0, 0x0

    goto :goto_0

    :catch_1
    move-exception v0

    const/4 v0, 0x0

    goto :goto_0

    :cond_2
    invoke-static {}, Lcom/kamcord/android/core/KC_T;->k()[Landroid/media/MediaCodecInfo;

    move-result-object v5

    const/4 v1, 0x0

    :goto_1
    array-length v2, v5

    if-ge v1, v2, :cond_5

    aget-object v6, v5, v1

    invoke-virtual {v6}, Landroid/media/MediaCodecInfo;->isEncoder()Z

    move-result v2

    if-eqz v2, :cond_4

    invoke-virtual {v6}, Landroid/media/MediaCodecInfo;->getName()Ljava/lang/String;

    move-result-object v7

    invoke-virtual {v6}, Landroid/media/MediaCodecInfo;->getSupportedTypes()[Ljava/lang/String;

    move-result-object v8

    const/4 v2, 0x0

    :goto_2
    array-length v3, v8

    if-ge v2, v3, :cond_4

    aget-object v9, v8, v2

    const-string v3, "audio"

    invoke-virtual {v9, v3}, Ljava/lang/String;->startsWith(Ljava/lang/String;)Z

    move-result v3

    if-nez v3, :cond_3

    :try_start_1
    invoke-virtual {v6, v9}, Landroid/media/MediaCodecInfo;->getCapabilitiesForType(Ljava/lang/String;)Landroid/media/MediaCodecInfo$CodecCapabilities;
    :try_end_1
    .catch Ljava/lang/RuntimeException; {:try_start_1 .. :try_end_1} :catch_2
    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_3

    move-result-object v3

    iget-object v10, v3, Landroid/media/MediaCodecInfo$CodecCapabilities;->colorFormats:[I

    if-eqz v0, :cond_3

    const/4 v3, 0x0

    :goto_3
    array-length v11, v10

    if-ge v3, v11, :cond_3

    new-instance v11, Lcom/kamcord/android/core/KC_c;

    aget v12, v10, v3

    const/4 v13, 0x0

    invoke-direct {v11, v7, v9, v12, v13}, Lcom/kamcord/android/core/KC_c;-><init>(Ljava/lang/String;Ljava/lang/String;ILcom/kamcord/android/core/KC_f;)V

    invoke-static {v11, v0, v4}, Lcom/kamcord/android/core/KC_T;->a(Lcom/kamcord/android/core/KC_c;Ljava/util/List;Ljava/util/List;)V

    add-int/lit8 v3, v3, 0x1

    goto :goto_3

    :catch_2
    move-exception v3

    :cond_3
    :goto_4
    add-int/lit8 v2, v2, 0x1

    goto :goto_2

    :cond_4
    add-int/lit8 v1, v1, 0x1

    goto :goto_1

    :cond_5
    move-object v0, v4

    goto :goto_0

    :catch_3
    move-exception v3

    goto :goto_4
.end method

.method private a(Landroid/media/MediaFormat;Landroid/media/MediaFormat;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->i:Lcom/kamcord/android/core/KC_S;

    invoke-virtual {v0, p1, p2}, Lcom/kamcord/android/core/KC_S;->a(Landroid/media/MediaFormat;Landroid/media/MediaFormat;)V

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_T;->r:Z

    return-void
.end method

.method private static a(Lcom/kamcord/android/core/KC_c;Ljava/util/List;Ljava/util/List;)V
    .locals 5
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Lcom/kamcord/android/core/KC_c;",
            "Ljava/util/List",
            "<",
            "Lcom/kamcord/android/core/KC_f;",
            ">;",
            "Ljava/util/List",
            "<",
            "Lcom/kamcord/android/core/KC_c;",
            ">;)V"
        }
    .end annotation

    invoke-interface {p1}, Ljava/util/List;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/core/KC_f;

    new-instance v2, Lcom/kamcord/android/core/KC_c;

    invoke-direct {v2, p0}, Lcom/kamcord/android/core/KC_c;-><init>(Lcom/kamcord/android/core/KC_c;)V

    iget-object v3, v2, Lcom/kamcord/android/core/KC_c;->a:Ljava/lang/String;

    const-string v4, "OMX.TI.DUCATI1.VIDEO.H264E"

    invoke-virtual {v3, v4}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v3

    if-eqz v3, :cond_0

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "Codec OMX.TI.DUCATI1.VIDEO.H264E.  Rounding down width = "

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget v4, v0, Lcom/kamcord/android/core/KC_f;->a:I

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-static {v3}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v3, Lcom/kamcord/android/core/KC_f;

    iget v4, v0, Lcom/kamcord/android/core/KC_f;->a:I

    and-int/lit8 v4, v4, -0x10

    iget v0, v0, Lcom/kamcord/android/core/KC_f;->b:I

    invoke-direct {v3, v4, v0}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    iput-object v3, v2, Lcom/kamcord/android/core/KC_c;->d:Lcom/kamcord/android/core/KC_f;

    :goto_1
    invoke-interface {p2, v2}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    goto :goto_0

    :cond_0
    iput-object v0, v2, Lcom/kamcord/android/core/KC_c;->d:Lcom/kamcord/android/core/KC_f;

    goto :goto_1

    :cond_1
    return-void
.end method

.method private static k()[Landroid/media/MediaCodecInfo;
    .locals 6

    const/4 v1, 0x0

    invoke-static {}, Landroid/media/MediaCodecList;->getCodecCount()I

    move-result v3

    move v2, v1

    move v0, v1

    :goto_0
    if-ge v2, v3, :cond_1

    invoke-static {v2}, Landroid/media/MediaCodecList;->getCodecInfoAt(I)Landroid/media/MediaCodecInfo;

    move-result-object v4

    invoke-virtual {v4}, Landroid/media/MediaCodecInfo;->isEncoder()Z

    move-result v4

    if-eqz v4, :cond_0

    add-int/lit8 v0, v0, 0x1

    :cond_0
    add-int/lit8 v2, v2, 0x1

    goto :goto_0

    :cond_1
    new-array v2, v0, [Landroid/media/MediaCodecInfo;

    move v0, v1

    :goto_1
    if-ge v1, v3, :cond_3

    invoke-static {v1}, Landroid/media/MediaCodecList;->getCodecInfoAt(I)Landroid/media/MediaCodecInfo;

    move-result-object v4

    invoke-virtual {v4}, Landroid/media/MediaCodecInfo;->isEncoder()Z

    move-result v5

    if-eqz v5, :cond_2

    aput-object v4, v2, v0

    add-int/lit8 v0, v0, 0x1

    :cond_2
    add-int/lit8 v1, v1, 0x1

    goto :goto_1

    :cond_3
    return-object v2
.end method

.method private l()V
    .locals 2

    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_T;->r:Z

    if-eqz v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->d:Lcom/kamcord/android/AudioSource;

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->p:Landroid/media/MediaFormat;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->q:Landroid/media/MediaFormat;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->p:Landroid/media/MediaFormat;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->q:Landroid/media/MediaFormat;

    invoke-direct {p0, v0, v1}, Lcom/kamcord/android/core/KC_T;->a(Landroid/media/MediaFormat;Landroid/media/MediaFormat;)V

    goto :goto_0

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->p:Landroid/media/MediaFormat;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->p:Landroid/media/MediaFormat;

    const/4 v1, 0x0

    invoke-direct {p0, v0, v1}, Lcom/kamcord/android/core/KC_T;->a(Landroid/media/MediaFormat;Landroid/media/MediaFormat;)V

    goto :goto_0
.end method

.method private m()V
    .locals 11

    const/4 v0, 0x0

    iget-object v2, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    monitor-enter v2

    :try_start_0
    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    invoke-virtual {v1}, Landroid/media/MediaCodec;->getOutputBuffers()[Ljava/nio/ByteBuffer;

    move-result-object v1

    new-instance v3, Landroid/media/MediaCodec$BufferInfo;

    invoke-direct {v3}, Landroid/media/MediaCodec$BufferInfo;-><init>()V

    monitor-exit v2
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    move v10, v0

    move-object v0, v1

    move v1, v10

    :goto_0
    const/16 v2, 0xa

    if-ge v1, v2, :cond_0

    iget-object v2, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    monitor-enter v2

    :try_start_1
    iget-object v4, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    const-wide/16 v6, 0xa

    invoke-virtual {v4, v3, v6, v7}, Landroid/media/MediaCodec;->dequeueOutputBuffer(Landroid/media/MediaCodec$BufferInfo;J)I

    move-result v4

    const/4 v5, -0x1

    if-ne v4, v5, :cond_1

    monitor-exit v2
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_1

    :cond_0
    :goto_1
    return-void

    :catchall_0
    move-exception v0

    monitor-exit v2

    throw v0

    :cond_1
    const/4 v5, -0x3

    if-ne v4, v5, :cond_2

    :try_start_2
    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    invoke-virtual {v0}, Landroid/media/MediaCodec;->getOutputBuffers()[Ljava/nio/ByteBuffer;

    move-result-object v0

    monitor-exit v2

    :goto_2
    add-int/lit8 v1, v1, 0x1

    goto :goto_0

    :cond_2
    const/4 v5, -0x2

    if-ne v4, v5, :cond_3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    invoke-virtual {v0}, Landroid/media/MediaCodec;->getOutputFormat()Landroid/media/MediaFormat;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_T;->q:Landroid/media/MediaFormat;

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_T;->l:Z

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_T;->l()V

    monitor-exit v2
    :try_end_2
    .catchall {:try_start_2 .. :try_end_2} :catchall_1

    goto :goto_1

    :catchall_1
    move-exception v0

    monitor-exit v2

    throw v0

    :cond_3
    if-ltz v4, :cond_7

    :try_start_3
    iget-boolean v5, p0, Lcom/kamcord/android/core/KC_T;->l:Z

    if-nez v5, :cond_4

    const/4 v5, 0x1

    iput-boolean v5, p0, Lcom/kamcord/android/core/KC_T;->l:Z

    iget-object v5, p0, Lcom/kamcord/android/core/KC_T;->f:Landroid/media/MediaFormat;

    iput-object v5, p0, Lcom/kamcord/android/core/KC_T;->q:Landroid/media/MediaFormat;

    iget-object v5, p0, Lcom/kamcord/android/core/KC_T;->q:Landroid/media/MediaFormat;

    if-eqz v5, :cond_4

    const/4 v5, 0x2

    new-array v5, v5, [B

    aget-object v6, v0, v4

    invoke-virtual {v6, v5}, Ljava/nio/ByteBuffer;->get([B)Ljava/nio/ByteBuffer;

    aget-object v6, v0, v4

    invoke-virtual {v6}, Ljava/nio/ByteBuffer;->rewind()Ljava/nio/Buffer;

    invoke-static {v5}, Ljava/nio/ByteBuffer;->wrap([B)Ljava/nio/ByteBuffer;

    move-result-object v5

    iget-object v6, p0, Lcom/kamcord/android/core/KC_T;->q:Landroid/media/MediaFormat;

    const-string v7, "csd-0"

    invoke-virtual {v6, v7, v5}, Landroid/media/MediaFormat;->setByteBuffer(Ljava/lang/String;Ljava/nio/ByteBuffer;)V

    :cond_4
    invoke-direct {p0}, Lcom/kamcord/android/core/KC_T;->l()V

    iget-boolean v5, p0, Lcom/kamcord/android/core/KC_T;->r:Z

    if-nez v5, :cond_5

    monitor-exit v2

    goto :goto_1

    :cond_5
    aget-object v5, v0, v4

    iget-object v6, p0, Lcom/kamcord/android/core/KC_T;->i:Lcom/kamcord/android/core/KC_S;

    invoke-virtual {v6, v5, v3}, Lcom/kamcord/android/core/KC_S;->b(Ljava/nio/ByteBuffer;Landroid/media/MediaCodec$BufferInfo;)V

    iget-object v5, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    if-eqz v5, :cond_6

    iget-object v5, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    iget-wide v6, v3, Landroid/media/MediaCodec$BufferInfo;->presentationTimeUs:J

    long-to-double v6, v6

    const-wide v8, 0x3eb0c6f7a0b5ed8dL    # 1.0E-6

    mul-double/2addr v6, v8

    invoke-virtual {v5, v6, v7}, Lcom/kamcord/android/core/KC_a;->a(D)V

    :cond_6
    iget-object v5, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    const/4 v6, 0x0

    invoke-virtual {v5, v4, v6}, Landroid/media/MediaCodec;->releaseOutputBuffer(IZ)V

    iget v4, v3, Landroid/media/MediaCodec$BufferInfo;->flags:I

    and-int/lit8 v4, v4, 0x4

    if-eqz v4, :cond_7

    monitor-exit v2
    :try_end_3
    .catchall {:try_start_3 .. :try_end_3} :catchall_1

    goto :goto_1

    :cond_7
    monitor-exit v2

    goto :goto_2
.end method


# virtual methods
.method final a()V
    .locals 4

    const/4 v3, 0x0

    iput-object v3, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    :try_start_0
    new-instance v0, Lcom/kamcord/android/core/KC_U;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_U;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v2, p0, Lcom/kamcord/android/core/KC_T;->i:Lcom/kamcord/android/core/KC_S;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_S;->d()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "/voice.wav"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    iput-object v1, v0, Lcom/kamcord/android/core/KC_U;->c:Ljava/lang/String;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_U;->a()V
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    return-void

    :catch_0
    move-exception v0

    const-string v0, "Couldn\'t initialize voice recorder, disabling."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    iput-object v3, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    goto :goto_0
.end method

.method final a(D)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->i:Lcom/kamcord/android/core/KC_S;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_S;->a:Lcom/kamcord/android/core/KC_H;

    iput-wide p1, v0, Lcom/kamcord/android/core/KC_H;->h:D

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->i:Lcom/kamcord/android/core/KC_S;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_S;->b()V

    return-void
.end method

.method final a(J)V
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_T;->m:Z

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->d:Lcom/kamcord/android/AudioSource;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    invoke-virtual {v0, p1, p2}, Lcom/kamcord/android/core/KC_a;->a(J)V

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_U;->f()V

    :cond_1
    :goto_0
    return-void

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->d:Lcom/kamcord/android/AudioSource;

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->d:Lcom/kamcord/android/AudioSource;

    invoke-interface {v0}, Lcom/kamcord/android/AudioSource;->skip()V

    goto :goto_0
.end method

.method final a(Lcom/kamcord/android/core/KC_I;Lcom/kamcord/android/core/KC_f;Lcom/kamcord/android/core/KC_a;Lcom/kamcord/android/AudioSource;)V
    .locals 8

    const/4 v2, 0x0

    const/4 v7, 0x0

    iput-object p1, p0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    iput-object p3, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    iput-boolean v2, p0, Lcom/kamcord/android/core/KC_T;->k:Z

    iput-boolean v2, p0, Lcom/kamcord/android/core/KC_T;->l:Z

    invoke-virtual {p1}, Lcom/kamcord/android/core/KC_I;->c()[I

    move-result-object v5

    const-string v1, "video/avc"

    move-object v0, p0

    move v3, v2

    move v4, v2

    move-object v6, p2

    invoke-direct/range {v0 .. v6}, Lcom/kamcord/android/core/KC_T;->a(Ljava/lang/String;ZII[ILcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/core/KC_h;

    move-result-object v0

    if-eqz v0, :cond_0

    iget-object v1, v0, Lcom/kamcord/android/core/KC_h;->a:Landroid/media/MediaCodec;

    if-eqz v1, :cond_0

    iget-object v1, v0, Lcom/kamcord/android/core/KC_h;->b:Landroid/media/MediaFormat;

    if-nez v1, :cond_2

    :cond_0
    const-string v0, "video codec unavailable."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    iput-object v7, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    :goto_0
    if-eqz p4, :cond_4

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    if-eqz v0, :cond_4

    const-string v0, "Audio source provided.  Initializing audio codec."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    :try_start_0
    invoke-interface {p4}, Lcom/kamcord/android/AudioSource;->getSampleRate()I

    move-result v3

    invoke-interface {p4}, Lcom/kamcord/android/AudioSource;->getNumChannels()I
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    move-result v4

    const-string v1, "audio/mp4a-latm"

    const/4 v2, 0x1

    move-object v0, p0

    move-object v5, v7

    move-object v6, v7

    invoke-direct/range {v0 .. v6}, Lcom/kamcord/android/core/KC_T;->a(Ljava/lang/String;ZII[ILcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/core/KC_h;

    move-result-object v0

    if-eqz v0, :cond_1

    iget-object v1, v0, Lcom/kamcord/android/core/KC_h;->a:Landroid/media/MediaCodec;

    if-eqz v1, :cond_1

    iget-object v1, v0, Lcom/kamcord/android/core/KC_h;->b:Landroid/media/MediaFormat;

    if-nez v1, :cond_3

    :cond_1
    const-string v0, "Audio codec unavailable.  Cancelling audio for this recording."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    iput-object v7, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    iput-object v7, p0, Lcom/kamcord/android/core/KC_T;->d:Lcom/kamcord/android/AudioSource;

    iput-object v7, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    :goto_1
    return-void

    :cond_2
    iget-object v1, v0, Lcom/kamcord/android/core/KC_h;->a:Landroid/media/MediaCodec;

    iput-object v1, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    iget-object v1, v0, Lcom/kamcord/android/core/KC_h;->b:Landroid/media/MediaFormat;

    iput-object v1, p0, Lcom/kamcord/android/core/KC_T;->e:Landroid/media/MediaFormat;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    invoke-virtual {v1, v0, p2}, Lcom/kamcord/android/core/KC_I;->a(Lcom/kamcord/android/core/KC_h;Lcom/kamcord/android/core/KC_f;)Z

    iget-object v0, v0, Lcom/kamcord/android/core/KC_h;->c:Lcom/kamcord/android/core/KC_f;

    goto :goto_0

    :catch_0
    move-exception v0

    const-string v1, "Client audio-source exception. Disabling Audio."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    iput-object v7, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    iput-object v7, p0, Lcom/kamcord/android/core/KC_T;->d:Lcom/kamcord/android/AudioSource;

    iput-object v7, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    goto :goto_1

    :cond_3
    const-string v1, "Audio codec initialized."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v1, v0, Lcom/kamcord/android/core/KC_h;->a:Landroid/media/MediaCodec;

    iput-object v1, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    iget-object v1, v0, Lcom/kamcord/android/core/KC_h;->b:Landroid/media/MediaFormat;

    iput-object v1, p0, Lcom/kamcord/android/core/KC_T;->f:Landroid/media/MediaFormat;

    iput-object p3, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    invoke-virtual {v1, v0, p4}, Lcom/kamcord/android/core/KC_a;->a(Lcom/kamcord/android/core/KC_h;Lcom/kamcord/android/AudioSource;)Z

    iput-object p4, p0, Lcom/kamcord/android/core/KC_T;->d:Lcom/kamcord/android/AudioSource;

    goto :goto_1

    :cond_4
    iput-object v7, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    const-string v0, "Either audio source or input not provided. Audio off."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    goto :goto_1
.end method

.method final a(Lcom/kamcord/android/core/KC_S;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/core/KC_T;->i:Lcom/kamcord/android/core/KC_S;

    return-void
.end method

.method final a(Z)V
    .locals 5

    const/4 v1, 0x1

    const/4 v2, 0x0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->o:Lcom/kamcord/android/core/KC_e;

    if-eqz v0, :cond_6

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->o:Lcom/kamcord/android/core/KC_e;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_e;->isAlive()Z

    move-result v0

    if-eqz v0, :cond_6

    move v0, v1

    :goto_0
    iget-boolean v3, p0, Lcom/kamcord/android/core/KC_T;->m:Z

    if-eqz v3, :cond_3

    iput-boolean v2, p0, Lcom/kamcord/android/core/KC_T;->m:Z

    if-eqz v0, :cond_3

    const-wide/16 v2, 0x0

    iget-object v4, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    if-eqz v4, :cond_0

    iget-object v4, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    invoke-virtual {v4}, Lcom/kamcord/android/core/KC_a;->e()V

    :cond_0
    iget-object v4, p0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    if-eqz v4, :cond_1

    iget-object v2, p0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_I;->e()V

    iget-object v2, p0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_I;->i()D

    move-result-wide v2

    :cond_1
    if-eqz p1, :cond_2

    iget-object v4, p0, Lcom/kamcord/android/core/KC_T;->o:Lcom/kamcord/android/core/KC_e;

    invoke-virtual {v4}, Lcom/kamcord/android/core/KC_e;->e()V

    :cond_2
    iget-object v4, p0, Lcom/kamcord/android/core/KC_T;->o:Lcom/kamcord/android/core/KC_e;

    invoke-virtual {v4}, Lcom/kamcord/android/core/KC_e;->f()V

    iget-object v4, p0, Lcom/kamcord/android/core/KC_T;->i:Lcom/kamcord/android/core/KC_S;

    iget-object v4, v4, Lcom/kamcord/android/core/KC_S;->a:Lcom/kamcord/android/core/KC_H;

    iput-boolean v1, v4, Lcom/kamcord/android/core/KC_H;->d:Z

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->o:Lcom/kamcord/android/core/KC_e;

    invoke-virtual {v1, v2, v3}, Lcom/kamcord/android/core/KC_e;->a(D)V

    :cond_3
    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    if-eqz v1, :cond_4

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    invoke-virtual {v1}, Lcom/kamcord/android/core/KC_U;->e()V

    :cond_4
    if-eqz v0, :cond_5

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->o:Lcom/kamcord/android/core/KC_e;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_e;->d()V

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_T;->o:Lcom/kamcord/android/core/KC_e;

    :cond_5
    return-void

    :cond_6
    move v0, v2

    goto :goto_0
.end method

.method final a(Lcom/kamcord/android/core/KC_s;J)Z
    .locals 8

    const/4 v0, 0x0

    iget-object v2, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    monitor-enter v2

    :try_start_0
    iget-boolean v1, p0, Lcom/kamcord/android/core/KC_T;->m:Z

    if-eqz v1, :cond_1

    iget-boolean v1, p0, Lcom/kamcord/android/core/KC_T;->n:Z

    if-nez v1, :cond_1

    const/4 v1, 0x1

    :goto_0
    if-eqz v1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    invoke-virtual {v1}, Lcom/kamcord/android/core/KC_I;->g()Z

    move-result v1

    if-eqz v1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->o:Lcom/kamcord/android/core/KC_e;

    invoke-virtual {v1}, Lcom/kamcord/android/core/KC_e;->e()V

    invoke-virtual {p1}, Lcom/kamcord/android/core/KC_s;->d()Lcom/kamcord/android/core/KC_y;

    move-result-object v1

    if-nez v1, :cond_2

    :cond_0
    :goto_1
    monitor-exit v2

    return v0

    :cond_1
    move v1, v0

    goto :goto_0

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    invoke-virtual {v0, v1, p2, p3}, Lcom/kamcord/android/core/KC_I;->a(Lcom/kamcord/android/core/KC_y;J)Z

    move-result v0

    iget-wide v4, p0, Lcom/kamcord/android/core/KC_T;->j:J

    const-wide/16 v6, 0x1

    add-long/2addr v4, v6

    iput-wide v4, p0, Lcom/kamcord/android/core/KC_T;->j:J
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    goto :goto_1

    :catchall_0
    move-exception v0

    monitor-exit v2

    throw v0
.end method

.method final b()V
    .locals 4

    const/4 v3, 0x0

    const/4 v2, 0x0

    const-string v0, "Writer start recording."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    if-nez v0, :cond_0

    const-string v0, "Writer started recording with no codec."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    :goto_0
    return-void

    :cond_0
    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    monitor-enter v1

    :try_start_0
    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_T;->m:Z

    if-eqz v0, :cond_1

    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    :cond_1
    :try_start_1
    const-string v0, "Starting video codec..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    invoke-virtual {v0}, Landroid/media/MediaCodec;->start()V

    const-string v0, "...done starting video codec."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    monitor-exit v1
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    if-eqz v0, :cond_2

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    monitor-enter v1

    :try_start_2
    const-string v0, "Starting audio codec..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    invoke-virtual {v0}, Landroid/media/MediaCodec;->start()V

    const-string v0, "...done starting audio codec."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    monitor-exit v1
    :try_end_2
    .catchall {:try_start_2 .. :try_end_2} :catchall_1

    :cond_2
    iput-object v3, p0, Lcom/kamcord/android/core/KC_T;->p:Landroid/media/MediaFormat;

    iput-object v3, p0, Lcom/kamcord/android/core/KC_T;->q:Landroid/media/MediaFormat;

    iput-boolean v2, p0, Lcom/kamcord/android/core/KC_T;->r:Z

    new-instance v0, Lcom/kamcord/android/core/KC_e;

    invoke-direct {v0, p0}, Lcom/kamcord/android/core/KC_e;-><init>(Lcom/kamcord/android/core/KC_T;)V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_T;->o:Lcom/kamcord/android/core/KC_e;

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->o:Lcom/kamcord/android/core/KC_e;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_e;->start()V

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    monitor-enter v1

    :try_start_3
    const-string v0, "Starting video codec input..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_I;->d()V

    const-string v0, "...done starting codec input."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    monitor-exit v1
    :try_end_3
    .catchall {:try_start_3 .. :try_end_3} :catchall_2

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    if-eqz v0, :cond_3

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    monitor-enter v1

    :try_start_4
    const-string v0, "Starting audio codec input..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_a;->d()V

    const-string v0, "...done starting audio codec input."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    monitor-exit v1
    :try_end_4
    .catchall {:try_start_4 .. :try_end_4} :catchall_3

    :cond_3
    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    if-eqz v0, :cond_4

    const-string v0, "Starting voice writer..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_U;->b()V

    const-string v0, "...done starting voice writer."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    :cond_4
    const-wide/16 v0, 0x0

    iput-wide v0, p0, Lcom/kamcord/android/core/KC_T;->j:J

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_T;->m:Z

    iput-boolean v2, p0, Lcom/kamcord/android/core/KC_T;->n:Z

    goto/16 :goto_0

    :catchall_1
    move-exception v0

    monitor-exit v1

    throw v0

    :catchall_2
    move-exception v0

    monitor-exit v1

    throw v0

    :catchall_3
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method final c()V
    .locals 2

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    monitor-enter v1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_I;->j()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_a;->f()V

    :cond_0
    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_U;->c()V

    :cond_1
    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_T;->n:Z

    return-void

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method final d()V
    .locals 4

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_T;->n:Z

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    monitor-enter v1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    if-nez v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_I;->k()V

    :goto_0
    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_U;->d()V

    :cond_0
    return-void

    :cond_1
    :try_start_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    iget-object v2, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_a;->c()J

    move-result-wide v2

    invoke-virtual {v0, v2, v3}, Lcom/kamcord/android/core/KC_I;->b(J)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_a;->g()V
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method final e()V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    if-eqz v0, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    monitor-enter v1

    :try_start_0
    const-string v0, "Stopping video codec..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    invoke-virtual {v0}, Landroid/media/MediaCodec;->stop()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    invoke-virtual {v0}, Landroid/media/MediaCodec;->release()V

    const-string v0, "Video codec stopped."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :goto_0
    :try_start_1
    monitor-exit v1
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    if-eqz v0, :cond_1

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    monitor-enter v1

    :try_start_2
    const-string v0, "Stopping audio codec..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    invoke-virtual {v0}, Landroid/media/MediaCodec;->stop()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    invoke-virtual {v0}, Landroid/media/MediaCodec;->release()V

    const-string v0, "Audio codec stopped."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_2
    .catch Ljava/lang/Throwable; {:try_start_2 .. :try_end_2} :catch_1
    .catchall {:try_start_2 .. :try_end_2} :catchall_1

    :goto_1
    :try_start_3
    monitor-exit v1
    :try_end_3
    .catchall {:try_start_3 .. :try_end_3} :catchall_1

    :cond_1
    return-void

    :catch_0
    move-exception v0

    :try_start_4
    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V
    :try_end_4
    .catchall {:try_start_4 .. :try_end_4} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    :catch_1
    move-exception v0

    :try_start_5
    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V
    :try_end_5
    .catchall {:try_start_5 .. :try_end_5} :catchall_1

    goto :goto_1

    :catchall_1
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method final f()V
    .locals 11

    const/4 v0, 0x0

    iget-object v2, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    monitor-enter v2

    :try_start_0
    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    invoke-virtual {v1}, Landroid/media/MediaCodec;->getOutputBuffers()[Ljava/nio/ByteBuffer;

    move-result-object v1

    new-instance v3, Landroid/media/MediaCodec$BufferInfo;

    invoke-direct {v3}, Landroid/media/MediaCodec$BufferInfo;-><init>()V

    monitor-exit v2
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    move v10, v0

    move-object v0, v1

    move v1, v10

    :goto_0
    const/16 v2, 0xa

    if-ge v1, v2, :cond_0

    iget-object v2, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    monitor-enter v2

    :try_start_1
    iget-object v4, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    const-wide/16 v6, 0xa

    invoke-virtual {v4, v3, v6, v7}, Landroid/media/MediaCodec;->dequeueOutputBuffer(Landroid/media/MediaCodec$BufferInfo;J)I

    move-result v4

    const/4 v5, -0x1

    if-ne v4, v5, :cond_2

    monitor-exit v2
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_1

    :cond_0
    :goto_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    if-eqz v0, :cond_1

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_T;->m()V

    :cond_1
    return-void

    :catchall_0
    move-exception v0

    monitor-exit v2

    throw v0

    :cond_2
    const/4 v5, -0x3

    if-ne v4, v5, :cond_3

    :try_start_2
    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    invoke-virtual {v0}, Landroid/media/MediaCodec;->getOutputBuffers()[Ljava/nio/ByteBuffer;

    move-result-object v0

    monitor-exit v2

    :goto_2
    add-int/lit8 v1, v1, 0x1

    goto :goto_0

    :cond_3
    const/4 v5, -0x2

    if-ne v4, v5, :cond_4

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    invoke-virtual {v0}, Landroid/media/MediaCodec;->getOutputFormat()Landroid/media/MediaFormat;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_T;->p:Landroid/media/MediaFormat;

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_T;->k:Z

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_T;->l()V

    monitor-exit v2
    :try_end_2
    .catchall {:try_start_2 .. :try_end_2} :catchall_1

    goto :goto_1

    :catchall_1
    move-exception v0

    monitor-exit v2

    throw v0

    :cond_4
    if-ltz v4, :cond_8

    :try_start_3
    iget-boolean v5, p0, Lcom/kamcord/android/core/KC_T;->k:Z

    if-nez v5, :cond_5

    const/4 v5, 0x1

    iput-boolean v5, p0, Lcom/kamcord/android/core/KC_T;->k:Z

    iget-object v5, p0, Lcom/kamcord/android/core/KC_T;->e:Landroid/media/MediaFormat;

    iput-object v5, p0, Lcom/kamcord/android/core/KC_T;->p:Landroid/media/MediaFormat;

    iget v5, v3, Landroid/media/MediaCodec$BufferInfo;->size:I

    add-int/lit8 v5, v5, -0x8

    new-array v5, v5, [B

    const/16 v6, 0x8

    new-array v6, v6, [B

    aget-object v7, v0, v4

    invoke-virtual {v7, v5}, Ljava/nio/ByteBuffer;->get([B)Ljava/nio/ByteBuffer;

    aget-object v7, v0, v4

    invoke-virtual {v7, v6}, Ljava/nio/ByteBuffer;->get([B)Ljava/nio/ByteBuffer;

    aget-object v7, v0, v4

    invoke-virtual {v7}, Ljava/nio/ByteBuffer;->rewind()Ljava/nio/Buffer;

    invoke-static {v5}, Ljava/nio/ByteBuffer;->wrap([B)Ljava/nio/ByteBuffer;

    move-result-object v5

    invoke-static {v6}, Ljava/nio/ByteBuffer;->wrap([B)Ljava/nio/ByteBuffer;

    move-result-object v6

    iget-object v7, p0, Lcom/kamcord/android/core/KC_T;->p:Landroid/media/MediaFormat;

    const-string v8, "csd-0"

    invoke-virtual {v7, v8, v5}, Landroid/media/MediaFormat;->setByteBuffer(Ljava/lang/String;Ljava/nio/ByteBuffer;)V

    iget-object v5, p0, Lcom/kamcord/android/core/KC_T;->p:Landroid/media/MediaFormat;

    const-string v7, "csd-1"

    invoke-virtual {v5, v7, v6}, Landroid/media/MediaFormat;->setByteBuffer(Ljava/lang/String;Ljava/nio/ByteBuffer;)V

    :cond_5
    invoke-direct {p0}, Lcom/kamcord/android/core/KC_T;->l()V

    iget-boolean v5, p0, Lcom/kamcord/android/core/KC_T;->r:Z

    if-nez v5, :cond_6

    monitor-exit v2

    goto :goto_1

    :cond_6
    aget-object v5, v0, v4

    iget-object v6, p0, Lcom/kamcord/android/core/KC_T;->i:Lcom/kamcord/android/core/KC_S;

    invoke-virtual {v6, v5, v3}, Lcom/kamcord/android/core/KC_S;->a(Ljava/nio/ByteBuffer;Landroid/media/MediaCodec$BufferInfo;)V

    iget-object v5, p0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    if-eqz v5, :cond_7

    iget-object v5, p0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    iget-wide v6, v3, Landroid/media/MediaCodec$BufferInfo;->presentationTimeUs:J

    long-to-double v6, v6

    const-wide v8, 0x3eb0c6f7a0b5ed8dL    # 1.0E-6

    mul-double/2addr v6, v8

    invoke-virtual {v5, v6, v7}, Lcom/kamcord/android/core/KC_I;->a(D)V

    :cond_7
    iget-object v5, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    const/4 v6, 0x0

    invoke-virtual {v5, v4, v6}, Landroid/media/MediaCodec;->releaseOutputBuffer(IZ)V

    iget v4, v3, Landroid/media/MediaCodec$BufferInfo;->flags:I

    and-int/lit8 v4, v4, 0x4

    if-eqz v4, :cond_8

    monitor-exit v2
    :try_end_3
    .catchall {:try_start_3 .. :try_end_3} :catchall_1

    goto/16 :goto_1

    :cond_8
    monitor-exit v2

    goto/16 :goto_2
.end method

.method public final g()V
    .locals 12

    const/16 v11, 0xa

    const/4 v10, -0x1

    const/4 v9, -0x2

    const/4 v8, -0x3

    const/4 v0, 0x0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    monitor-enter v1

    :try_start_0
    new-instance v2, Landroid/media/MediaCodec$BufferInfo;

    invoke-direct {v2}, Landroid/media/MediaCodec$BufferInfo;-><init>()V

    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    move v1, v0

    :goto_0
    if-ge v1, v11, :cond_0

    iget-object v3, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    monitor-enter v3

    :try_start_1
    iget-object v4, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    const-wide/16 v6, 0x3e8

    invoke-virtual {v4, v2, v6, v7}, Landroid/media/MediaCodec;->dequeueOutputBuffer(Landroid/media/MediaCodec$BufferInfo;J)I

    move-result v4

    if-ne v4, v10, :cond_2

    monitor-exit v3
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_1

    :cond_0
    :goto_1
    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    if-eqz v1, :cond_1

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    monitor-enter v1

    :try_start_2
    new-instance v2, Landroid/media/MediaCodec$BufferInfo;

    invoke-direct {v2}, Landroid/media/MediaCodec$BufferInfo;-><init>()V

    monitor-exit v1
    :try_end_2
    .catchall {:try_start_2 .. :try_end_2} :catchall_2

    :goto_2
    if-ge v0, v11, :cond_1

    iget-object v1, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    monitor-enter v1

    :try_start_3
    iget-object v3, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    const-wide/16 v4, 0x3e8

    invoke-virtual {v3, v2, v4, v5}, Landroid/media/MediaCodec;->dequeueOutputBuffer(Landroid/media/MediaCodec$BufferInfo;J)I

    move-result v3

    if-ne v3, v10, :cond_6

    monitor-exit v1
    :try_end_3
    .catchall {:try_start_3 .. :try_end_3} :catchall_3

    :cond_1
    :goto_3
    return-void

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    :cond_2
    if-eq v4, v8, :cond_3

    if-ne v4, v9, :cond_4

    :cond_3
    monitor-exit v3

    :goto_4
    add-int/lit8 v1, v1, 0x1

    goto :goto_0

    :cond_4
    if-ltz v4, :cond_5

    :try_start_4
    iget-object v5, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    const/4 v6, 0x0

    invoke-virtual {v5, v4, v6}, Landroid/media/MediaCodec;->releaseOutputBuffer(IZ)V

    iget v4, v2, Landroid/media/MediaCodec$BufferInfo;->flags:I

    and-int/lit8 v4, v4, 0x4

    if-eqz v4, :cond_5

    monitor-exit v3
    :try_end_4
    .catchall {:try_start_4 .. :try_end_4} :catchall_1

    goto :goto_1

    :catchall_1
    move-exception v0

    monitor-exit v3

    throw v0

    :cond_5
    monitor-exit v3

    goto :goto_4

    :catchall_2
    move-exception v0

    monitor-exit v1

    throw v0

    :cond_6
    if-eq v3, v8, :cond_7

    if-ne v3, v9, :cond_8

    :cond_7
    monitor-exit v1

    :goto_5
    add-int/lit8 v0, v0, 0x1

    goto :goto_2

    :cond_8
    if-ltz v3, :cond_9

    :try_start_5
    iget-object v4, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    const/4 v5, 0x0

    invoke-virtual {v4, v3, v5}, Landroid/media/MediaCodec;->releaseOutputBuffer(IZ)V

    iget v3, v2, Landroid/media/MediaCodec$BufferInfo;->flags:I

    and-int/lit8 v3, v3, 0x4

    if-eqz v3, :cond_9

    monitor-exit v1
    :try_end_5
    .catchall {:try_start_5 .. :try_end_5} :catchall_3

    goto :goto_3

    :catchall_3
    move-exception v0

    monitor-exit v1

    throw v0

    :cond_9
    monitor-exit v1

    goto :goto_5
.end method

.method public final h()Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_T;->m:Z

    if-eqz v0, :cond_0

    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_T;->n:Z

    if-nez v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public final i()Z
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->g:Landroid/media/MediaCodec;

    if-eqz v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public final j()Z
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_T;->h:Landroid/media/MediaCodec;

    if-eqz v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method
