.class public final Lcom/kamcord/android/core/KC_Q;
.super Lcom/kamcord/android/core/KC_S;


# instance fields
.field private b:Z

.field private c:Z

.field private d:Ljava/lang/Object;

.field private e:Landroid/media/MediaMuxer;

.field private f:I

.field private g:I


# direct methods
.method constructor <init>(Lcom/kamcord/android/core/KC_T;)V
    .locals 1

    invoke-direct {p0, p1}, Lcom/kamcord/android/core/KC_S;-><init>(Lcom/kamcord/android/core/KC_T;)V

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_Q;->b:Z

    new-instance v0, Ljava/lang/Object;

    invoke-direct {v0}, Ljava/lang/Object;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_Q;->d:Ljava/lang/Object;

    return-void
.end method

.method private f()V
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_Q;->b:Z

    if-nez v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_Q;->e:Landroid/media/MediaMuxer;

    invoke-virtual {v0}, Landroid/media/MediaMuxer;->start()V

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_Q;->b:Z

    :cond_0
    return-void
.end method

.method private g()V
    .locals 2

    const/4 v1, 0x0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_Q;->a:Lcom/kamcord/android/core/KC_H;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_Q;->b(Lcom/kamcord/android/core/KC_H;)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_Q;->a:Lcom/kamcord/android/core/KC_H;

    iput-boolean v1, v0, Lcom/kamcord/android/core/KC_H;->d:Z

    iget-object v0, p0, Lcom/kamcord/android/core/KC_Q;->a:Lcom/kamcord/android/core/KC_H;

    iput-boolean v1, v0, Lcom/kamcord/android/core/KC_H;->c:Z

    return-void
.end method


# virtual methods
.method protected final a()V
    .locals 3

    :try_start_0
    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_Q;->e()Ljava/lang/String;

    move-result-object v0

    new-instance v1, Landroid/media/MediaMuxer;

    const/4 v2, 0x0

    invoke-direct {v1, v0, v2}, Landroid/media/MediaMuxer;-><init>(Ljava/lang/String;I)V

    iput-object v1, p0, Lcom/kamcord/android/core/KC_Q;->e:Landroid/media/MediaMuxer;
    :try_end_0
    .catch Ljava/io/IOException; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    return-void

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/io/IOException;->printStackTrace()V

    goto :goto_0
.end method

.method protected final a(Landroid/media/MediaFormat;Landroid/media/MediaFormat;)V
    .locals 2

    iget-object v1, p0, Lcom/kamcord/android/core/KC_Q;->d:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_Q;->e:Landroid/media/MediaMuxer;

    invoke-virtual {v0, p1}, Landroid/media/MediaMuxer;->addTrack(Landroid/media/MediaFormat;)I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/core/KC_Q;->f:I

    if-eqz p2, :cond_1

    const/4 v0, 0x1

    :goto_0
    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_Q;->c:Z

    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_Q;->c:Z

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_Q;->e:Landroid/media/MediaMuxer;

    invoke-virtual {v0, p2}, Landroid/media/MediaMuxer;->addTrack(Landroid/media/MediaFormat;)I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/core/KC_Q;->g:I

    :cond_0
    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    return-void

    :cond_1
    const/4 v0, 0x0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method protected final a(Ljava/nio/ByteBuffer;Landroid/media/MediaCodec$BufferInfo;)V
    .locals 3

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_Q;->f()V

    iget v0, p2, Landroid/media/MediaCodec$BufferInfo;->offset:I

    invoke-virtual {p1, v0}, Ljava/nio/ByteBuffer;->position(I)Ljava/nio/Buffer;

    iget v0, p2, Landroid/media/MediaCodec$BufferInfo;->offset:I

    iget v1, p2, Landroid/media/MediaCodec$BufferInfo;->size:I

    add-int/2addr v0, v1

    invoke-virtual {p1, v0}, Ljava/nio/ByteBuffer;->limit(I)Ljava/nio/Buffer;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_Q;->d:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    iget v0, p2, Landroid/media/MediaCodec$BufferInfo;->flags:I

    and-int/lit8 v0, v0, 0x2

    if-nez v0, :cond_0

    iget v0, p2, Landroid/media/MediaCodec$BufferInfo;->size:I

    if-nez v0, :cond_1

    :cond_0
    monitor-exit v1

    :goto_0
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_Q;->e:Landroid/media/MediaMuxer;

    iget v2, p0, Lcom/kamcord/android/core/KC_Q;->f:I

    invoke-virtual {v0, v2, p1, p2}, Landroid/media/MediaMuxer;->writeSampleData(ILjava/nio/ByteBuffer;Landroid/media/MediaCodec$BufferInfo;)V

    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method protected final b()V
    .locals 3

    iget-object v1, p0, Lcom/kamcord/android/core/KC_Q;->d:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_Q;->b:Z

    if-nez v0, :cond_0

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_Q;->g()V

    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :goto_0
    return-void

    :cond_0
    :try_start_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_Q;->e:Landroid/media/MediaMuxer;

    invoke-virtual {v0}, Landroid/media/MediaMuxer;->stop()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_Q;->e:Landroid/media/MediaMuxer;

    invoke-virtual {v0}, Landroid/media/MediaMuxer;->release()V

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_Q;->c()Z

    iget-object v0, p0, Lcom/kamcord/android/core/KC_Q;->a:Lcom/kamcord/android/core/KC_H;

    const/4 v2, 0x1

    iput-boolean v2, v0, Lcom/kamcord/android/core/KC_H;->c:Z

    iget-object v0, p0, Lcom/kamcord/android/core/KC_Q;->a:Lcom/kamcord/android/core/KC_H;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_Q;->a(Lcom/kamcord/android/core/KC_H;)V
    :try_end_1
    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_0
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    :goto_1
    const/4 v0, 0x0

    :try_start_2
    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_Q;->b:Z

    monitor-exit v1
    :try_end_2
    .catchall {:try_start_2 .. :try_end_2} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    :catch_0
    move-exception v0

    :try_start_3
    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_Q;->g()V
    :try_end_3
    .catchall {:try_start_3 .. :try_end_3} :catchall_0

    goto :goto_1
.end method

.method protected final b(Ljava/nio/ByteBuffer;Landroid/media/MediaCodec$BufferInfo;)V
    .locals 3

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_Q;->f()V

    iget v0, p2, Landroid/media/MediaCodec$BufferInfo;->offset:I

    invoke-virtual {p1, v0}, Ljava/nio/ByteBuffer;->position(I)Ljava/nio/Buffer;

    iget v0, p2, Landroid/media/MediaCodec$BufferInfo;->offset:I

    iget v1, p2, Landroid/media/MediaCodec$BufferInfo;->size:I

    add-int/2addr v0, v1

    invoke-virtual {p1, v0}, Ljava/nio/ByteBuffer;->limit(I)Ljava/nio/Buffer;

    iget v0, p2, Landroid/media/MediaCodec$BufferInfo;->size:I

    if-gtz v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_Q;->c:Z

    if-eqz v0, :cond_0

    iget v0, p2, Landroid/media/MediaCodec$BufferInfo;->flags:I

    and-int/lit8 v0, v0, 0x2

    if-nez v0, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_Q;->d:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_Q;->e:Landroid/media/MediaMuxer;

    iget v2, p0, Lcom/kamcord/android/core/KC_Q;->g:I

    invoke-virtual {v0, v2, p1, p2}, Landroid/media/MediaMuxer;->writeSampleData(ILjava/nio/ByteBuffer;Landroid/media/MediaCodec$BufferInfo;)V

    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method
