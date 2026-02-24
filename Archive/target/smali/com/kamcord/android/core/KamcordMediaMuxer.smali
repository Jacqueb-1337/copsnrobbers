.class public final Lcom/kamcord/android/core/KamcordMediaMuxer;
.super Ljava/lang/Object;


# instance fields
.field private a:I

.field private b:I

.field private c:J


# direct methods
.method public constructor <init>(Ljava/lang/String;I)V
    .locals 4
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/io/IOException;
        }
    .end annotation

    const/4 v0, -0x1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->a:I

    iput v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->b:I

    if-nez p1, :cond_0

    new-instance v0, Ljava/lang/IllegalArgumentException;

    const-string v1, "path must not be null"

    invoke-direct {v0, v1}, Ljava/lang/IllegalArgumentException;-><init>(Ljava/lang/String;)V

    throw v0

    :cond_0
    const/4 v2, 0x0

    :try_start_0
    new-instance v0, Ljava/io/File;

    invoke-direct {v0, p1}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    new-instance v1, Ljava/io/FileOutputStream;

    invoke-direct {v1, v0}, Ljava/io/FileOutputStream;-><init>(Ljava/io/File;)V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :try_start_1
    invoke-virtual {v1}, Ljava/io/FileOutputStream;->getFD()Ljava/io/FileDescriptor;

    move-result-object v0

    const/4 v2, 0x0

    invoke-static {v0, v2}, Lcom/kamcord/android/core/KamcordMediaMuxer;->nativeSetup(Ljava/io/FileDescriptor;I)J

    move-result-wide v2

    iput-wide v2, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J

    const/4 v0, 0x0

    iput v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->a:I
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_1

    invoke-virtual {v1}, Ljava/io/FileOutputStream;->close()V

    return-void

    :catchall_0
    move-exception v0

    move-object v1, v2

    :goto_0
    if-eqz v1, :cond_1

    invoke-virtual {v1}, Ljava/io/FileOutputStream;->close()V

    :cond_1
    throw v0

    :catchall_1
    move-exception v0

    goto :goto_0
.end method

.method private static native nativeAddTrack(J[Ljava/lang/String;[Ljava/lang/Object;)I
.end method

.method private static native nativeRelease(J)V
.end method

.method private static native nativeSetup(Ljava/io/FileDescriptor;I)J
.end method

.method private static native nativeStart(J)V
.end method

.method private static native nativeStop(J)V
.end method

.method private static native nativeWriteSampleData(JILjava/nio/ByteBuffer;IIJI)V
.end method


# virtual methods
.method public final a(Landroid/media/MediaFormat;)I
    .locals 6

    const/4 v1, 0x0

    if-nez p1, :cond_0

    new-instance v0, Ljava/lang/IllegalArgumentException;

    const-string v1, "format must not be null."

    invoke-direct {v0, v1}, Ljava/lang/IllegalArgumentException;-><init>(Ljava/lang/String;)V

    throw v0

    :cond_0
    iget v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->a:I

    if-eqz v0, :cond_1

    new-instance v0, Ljava/lang/IllegalStateException;

    const-string v1, "Muxer is not initialized."

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    :cond_1
    iget-wide v2, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J

    const-wide/16 v4, 0x0

    cmp-long v0, v2, v4

    if-nez v0, :cond_2

    new-instance v0, Ljava/lang/IllegalStateException;

    const-string v1, "Muxer has been released!"

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    :cond_2
    :try_start_0
    invoke-virtual {p1}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v0

    const-string v2, "getMap"

    const/4 v3, 0x0

    invoke-virtual {v0, v2, v3}, Ljava/lang/Class;->getDeclaredMethod(Ljava/lang/String;[Ljava/lang/Class;)Ljava/lang/reflect/Method;

    move-result-object v0

    const/4 v2, 0x1

    invoke-virtual {v0, v2}, Ljava/lang/reflect/Method;->setAccessible(Z)V

    const/4 v2, 0x0

    invoke-virtual {v0, p1, v2}, Ljava/lang/reflect/Method;->invoke(Ljava/lang/Object;[Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Map;
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    invoke-interface {v0}, Ljava/util/Map;->size()I

    move-result v2

    if-lez v2, :cond_4

    new-array v3, v2, [Ljava/lang/String;

    new-array v4, v2, [Ljava/lang/Object;

    invoke-interface {v0}, Ljava/util/Map;->entrySet()Ljava/util/Set;

    move-result-object v0

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v5

    move v2, v1

    :goto_0
    invoke-interface {v5}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_3

    invoke-interface {v5}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Map$Entry;

    invoke-interface {v0}, Ljava/util/Map$Entry;->getKey()Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Ljava/lang/String;

    aput-object v1, v3, v2

    invoke-interface {v0}, Ljava/util/Map$Entry;->getValue()Ljava/lang/Object;

    move-result-object v0

    aput-object v0, v4, v2

    add-int/lit8 v0, v2, 0x1

    move v2, v0

    goto :goto_0

    :catch_0
    move-exception v0

    const-string v2, "Unable to access MediaFormat methods"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/Kamcord;->bail()V

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    :goto_1
    return v1

    :cond_3
    iget-wide v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J

    invoke-static {v0, v1, v3, v4}, Lcom/kamcord/android/core/KamcordMediaMuxer;->nativeAddTrack(J[Ljava/lang/String;[Ljava/lang/Object;)I

    move-result v1

    iget v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->b:I

    if-lt v0, v1, :cond_5

    new-instance v0, Ljava/lang/IllegalArgumentException;

    const-string v1, "Invalid format."

    invoke-direct {v0, v1}, Ljava/lang/IllegalArgumentException;-><init>(Ljava/lang/String;)V

    throw v0

    :cond_4
    new-instance v0, Ljava/lang/IllegalArgumentException;

    const-string v1, "format must not be empty."

    invoke-direct {v0, v1}, Ljava/lang/IllegalArgumentException;-><init>(Ljava/lang/String;)V

    throw v0

    :cond_5
    iput v1, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->b:I

    goto :goto_1
.end method

.method public final a()V
    .locals 4

    iget-wide v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J

    const-wide/16 v2, 0x0

    cmp-long v0, v0, v2

    if-nez v0, :cond_0

    new-instance v0, Ljava/lang/IllegalStateException;

    const-string v1, "Muxer has been released!"

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    :cond_0
    iget v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->a:I

    if-nez v0, :cond_1

    iget-wide v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KamcordMediaMuxer;->nativeStart(J)V

    const/4 v0, 0x1

    iput v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->a:I

    return-void

    :cond_1
    new-instance v0, Ljava/lang/IllegalStateException;

    const-string v1, "Can\'t start due to wrong state."

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0
.end method

.method public final a(ILjava/nio/ByteBuffer;Landroid/media/MediaCodec$BufferInfo;)V
    .locals 9

    const-wide/16 v2, 0x0

    if-ltz p1, :cond_0

    iget v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->b:I

    if-le p1, v0, :cond_1

    :cond_0
    new-instance v0, Ljava/lang/IllegalArgumentException;

    const-string v1, "trackIndex is invalid"

    invoke-direct {v0, v1}, Ljava/lang/IllegalArgumentException;-><init>(Ljava/lang/String;)V

    throw v0

    :cond_1
    if-nez p2, :cond_2

    new-instance v0, Ljava/lang/IllegalArgumentException;

    const-string v1, "byteBuffer must not be null"

    invoke-direct {v0, v1}, Ljava/lang/IllegalArgumentException;-><init>(Ljava/lang/String;)V

    throw v0

    :cond_2
    if-nez p3, :cond_3

    new-instance v0, Ljava/lang/IllegalArgumentException;

    const-string v1, "bufferInfo must not be null"

    invoke-direct {v0, v1}, Ljava/lang/IllegalArgumentException;-><init>(Ljava/lang/String;)V

    throw v0

    :cond_3
    iget v0, p3, Landroid/media/MediaCodec$BufferInfo;->size:I

    if-ltz v0, :cond_4

    iget v0, p3, Landroid/media/MediaCodec$BufferInfo;->offset:I

    if-ltz v0, :cond_4

    iget v0, p3, Landroid/media/MediaCodec$BufferInfo;->offset:I

    iget v1, p3, Landroid/media/MediaCodec$BufferInfo;->size:I

    add-int/2addr v0, v1

    invoke-virtual {p2}, Ljava/nio/ByteBuffer;->capacity()I

    move-result v1

    if-gt v0, v1, :cond_4

    iget-wide v0, p3, Landroid/media/MediaCodec$BufferInfo;->presentationTimeUs:J

    cmp-long v0, v0, v2

    if-gez v0, :cond_5

    :cond_4
    new-instance v0, Ljava/lang/IllegalArgumentException;

    const-string v1, "bufferInfo must specify a valid buffer offset, size and presentation time"

    invoke-direct {v0, v1}, Ljava/lang/IllegalArgumentException;-><init>(Ljava/lang/String;)V

    throw v0

    :cond_5
    iget-wide v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J

    cmp-long v0, v0, v2

    if-nez v0, :cond_6

    new-instance v0, Ljava/lang/IllegalStateException;

    const-string v1, "Muxer has been released!"

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    :cond_6
    iget v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->a:I

    const/4 v1, 0x1

    if-eq v0, v1, :cond_7

    new-instance v0, Ljava/lang/IllegalStateException;

    const-string v1, "Can\'t write, muxer is not started"

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    :cond_7
    iget-wide v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J

    iget v4, p3, Landroid/media/MediaCodec$BufferInfo;->offset:I

    iget v5, p3, Landroid/media/MediaCodec$BufferInfo;->size:I

    iget-wide v6, p3, Landroid/media/MediaCodec$BufferInfo;->presentationTimeUs:J

    iget v8, p3, Landroid/media/MediaCodec$BufferInfo;->flags:I

    move v2, p1

    move-object v3, p2

    invoke-static/range {v0 .. v8}, Lcom/kamcord/android/core/KamcordMediaMuxer;->nativeWriteSampleData(JILjava/nio/ByteBuffer;IIJI)V

    return-void
.end method

.method public final b()V
    .locals 2

    iget v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->a:I

    const/4 v1, 0x1

    if-ne v0, v1, :cond_0

    iget-wide v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KamcordMediaMuxer;->nativeStop(J)V

    const/4 v0, 0x2

    iput v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->a:I

    return-void

    :cond_0
    new-instance v0, Ljava/lang/IllegalStateException;

    const-string v1, "Can\'t stop due to wrong state."

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0
.end method

.method public final c()V
    .locals 4

    const-wide/16 v2, 0x0

    iget v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->a:I

    const/4 v1, 0x1

    if-ne v0, v1, :cond_0

    invoke-virtual {p0}, Lcom/kamcord/android/core/KamcordMediaMuxer;->b()V

    :cond_0
    iget-wide v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J

    cmp-long v0, v0, v2

    if-eqz v0, :cond_1

    iget-wide v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KamcordMediaMuxer;->nativeRelease(J)V

    iput-wide v2, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J

    :cond_1
    const/4 v0, -0x1

    iput v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->a:I

    return-void
.end method

.method protected final finalize()V
    .locals 4
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Throwable;
        }
    .end annotation

    const-wide/16 v2, 0x0

    :try_start_0
    iget-wide v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J

    cmp-long v0, v0, v2

    if-eqz v0, :cond_0

    iget-wide v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KamcordMediaMuxer;->nativeRelease(J)V

    const-wide/16 v0, 0x0

    iput-wide v0, p0, Lcom/kamcord/android/core/KamcordMediaMuxer;->c:J
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :cond_0
    invoke-super {p0}, Ljava/lang/Object;->finalize()V

    return-void

    :catchall_0
    move-exception v0

    invoke-super {p0}, Ljava/lang/Object;->finalize()V

    throw v0
.end method
