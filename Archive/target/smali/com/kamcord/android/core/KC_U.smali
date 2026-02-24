.class public final Lcom/kamcord/android/core/KC_U;
.super Ljava/lang/Object;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/core/KC_U$KC_b;,
        Lcom/kamcord/android/core/KC_U$KC_a;
    }
.end annotation


# instance fields
.field a:Landroid/media/AudioRecord;

.field b:I

.field c:Ljava/lang/String;

.field private d:I

.field private volatile e:Z

.field private volatile f:Z

.field private g:I

.field private h:I

.field private i:Ljava/nio/ByteBuffer;

.field private j:I

.field private k:I

.field private l:Ljava/io/FileOutputStream;

.field private m:Lcom/kamcord/android/core/KC_U$KC_b;


# direct methods
.method public constructor <init>()V
    .locals 3

    const/4 v2, 0x0

    const/4 v1, 0x0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    const v0, 0xac44

    iput v0, p0, Lcom/kamcord/android/core/KC_U;->b:I

    const/16 v0, 0x10

    iput v0, p0, Lcom/kamcord/android/core/KC_U;->d:I

    iput-object v2, p0, Lcom/kamcord/android/core/KC_U;->c:Ljava/lang/String;

    iput-boolean v1, p0, Lcom/kamcord/android/core/KC_U;->e:Z

    iput-boolean v1, p0, Lcom/kamcord/android/core/KC_U;->f:Z

    iput v1, p0, Lcom/kamcord/android/core/KC_U;->j:I

    iput v1, p0, Lcom/kamcord/android/core/KC_U;->k:I

    iput-object v2, p0, Lcom/kamcord/android/core/KC_U;->m:Lcom/kamcord/android/core/KC_U$KC_b;

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/core/KC_U;I)I
    .locals 1

    const/4 v0, 0x0

    iput v0, p0, Lcom/kamcord/android/core/KC_U;->j:I

    return v0
.end method

.method static synthetic a(Lcom/kamcord/android/core/KC_U;Ljava/io/FileOutputStream;)Ljava/io/FileOutputStream;
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/core/KC_U;->l:Ljava/io/FileOutputStream;

    return-object p1
.end method

.method static synthetic a(Lcom/kamcord/android/core/KC_U;)Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_U;->e:Z

    return v0
.end method

.method static synthetic a(Lcom/kamcord/android/core/KC_U;Z)Z
    .locals 0

    iput-boolean p1, p0, Lcom/kamcord/android/core/KC_U;->e:Z

    return p1
.end method

.method static synthetic a(I)[B
    .locals 2

    const/4 v0, 0x4

    invoke-static {v0}, Ljava/nio/ByteBuffer;->allocate(I)Ljava/nio/ByteBuffer;

    move-result-object v0

    sget-object v1, Ljava/nio/ByteOrder;->LITTLE_ENDIAN:Ljava/nio/ByteOrder;

    invoke-virtual {v0, v1}, Ljava/nio/ByteBuffer;->order(Ljava/nio/ByteOrder;)Ljava/nio/ByteBuffer;

    invoke-virtual {v0, p0}, Ljava/nio/ByteBuffer;->putInt(I)Ljava/nio/ByteBuffer;

    invoke-virtual {v0}, Ljava/nio/ByteBuffer;->array()[B

    move-result-object v0

    return-object v0
.end method

.method static synthetic a(S)[B
    .locals 2

    const/4 v0, 0x2

    invoke-static {v0}, Ljava/nio/ByteBuffer;->allocate(I)Ljava/nio/ByteBuffer;

    move-result-object v0

    sget-object v1, Ljava/nio/ByteOrder;->LITTLE_ENDIAN:Ljava/nio/ByteOrder;

    invoke-virtual {v0, v1}, Ljava/nio/ByteBuffer;->order(Ljava/nio/ByteOrder;)Ljava/nio/ByteBuffer;

    invoke-virtual {v0, p0}, Ljava/nio/ByteBuffer;->putShort(S)Ljava/nio/ByteBuffer;

    invoke-virtual {v0}, Ljava/nio/ByteBuffer;->array()[B

    move-result-object v0

    return-object v0
.end method

.method static synthetic b(Lcom/kamcord/android/core/KC_U;I)I
    .locals 1

    const/4 v0, 0x0

    iput v0, p0, Lcom/kamcord/android/core/KC_U;->k:I

    return v0
.end method

.method static synthetic b(Lcom/kamcord/android/core/KC_U;)Ljava/io/FileOutputStream;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U;->l:Ljava/io/FileOutputStream;

    return-object v0
.end method

.method static synthetic b(Lcom/kamcord/android/core/KC_U;Z)Z
    .locals 0

    iput-boolean p1, p0, Lcom/kamcord/android/core/KC_U;->f:Z

    return p1
.end method

.method static synthetic c(Lcom/kamcord/android/core/KC_U;)I
    .locals 1

    iget v0, p0, Lcom/kamcord/android/core/KC_U;->j:I

    return v0
.end method

.method static synthetic c(Lcom/kamcord/android/core/KC_U;I)I
    .locals 1

    iget v0, p0, Lcom/kamcord/android/core/KC_U;->j:I

    add-int/2addr v0, p1

    iput v0, p0, Lcom/kamcord/android/core/KC_U;->j:I

    return v0
.end method

.method static synthetic d(Lcom/kamcord/android/core/KC_U;I)I
    .locals 1

    iget v0, p0, Lcom/kamcord/android/core/KC_U;->k:I

    add-int/2addr v0, p1

    iput v0, p0, Lcom/kamcord/android/core/KC_U;->k:I

    return v0
.end method

.method static synthetic d(Lcom/kamcord/android/core/KC_U;)Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_U;->f:Z

    return v0
.end method

.method static synthetic e(Lcom/kamcord/android/core/KC_U;)I
    .locals 1

    iget v0, p0, Lcom/kamcord/android/core/KC_U;->k:I

    return v0
.end method

.method static synthetic f(Lcom/kamcord/android/core/KC_U;)I
    .locals 1

    iget v0, p0, Lcom/kamcord/android/core/KC_U;->g:I

    return v0
.end method

.method static synthetic g(Lcom/kamcord/android/core/KC_U;)Ljava/nio/ByteBuffer;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U;->i:Ljava/nio/ByteBuffer;

    return-object v0
.end method

.method static synthetic h(Lcom/kamcord/android/core/KC_U;)I
    .locals 1

    iget v0, p0, Lcom/kamcord/android/core/KC_U;->h:I

    return v0
.end method


# virtual methods
.method public final a()V
    .locals 6

    const/4 v4, 0x2

    iget v0, p0, Lcom/kamcord/android/core/KC_U;->b:I

    iget v1, p0, Lcom/kamcord/android/core/KC_U;->d:I

    invoke-static {v0, v1, v4}, Landroid/media/AudioRecord;->getMinBufferSize(III)I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/core/KC_U;->h:I

    const v0, 0x25800

    iget v1, p0, Lcom/kamcord/android/core/KC_U;->h:I

    mul-int/lit8 v1, v1, 0xa

    invoke-static {v0, v1}, Ljava/lang/Math;->max(II)I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/core/KC_U;->g:I

    iget v0, p0, Lcom/kamcord/android/core/KC_U;->g:I

    invoke-static {v0}, Ljava/nio/ByteBuffer;->allocateDirect(I)Ljava/nio/ByteBuffer;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_U;->i:Ljava/nio/ByteBuffer;

    new-instance v0, Landroid/media/AudioRecord;

    const/4 v1, 0x1

    iget v2, p0, Lcom/kamcord/android/core/KC_U;->b:I

    iget v3, p0, Lcom/kamcord/android/core/KC_U;->d:I

    iget v5, p0, Lcom/kamcord/android/core/KC_U;->g:I

    invoke-direct/range {v0 .. v5}, Landroid/media/AudioRecord;-><init>(IIIII)V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_U;->a:Landroid/media/AudioRecord;

    return-void
.end method

.method public final declared-synchronized b()V
    .locals 2

    monitor-enter p0

    :try_start_0
    new-instance v0, Lcom/kamcord/android/core/KC_U$KC_b;

    invoke-direct {v0, p0}, Lcom/kamcord/android/core/KC_U$KC_b;-><init>(Lcom/kamcord/android/core/KC_U;)V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_U;->m:Lcom/kamcord/android/core/KC_U$KC_b;

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U;->m:Lcom/kamcord/android/core/KC_U$KC_b;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_U$KC_b;->start()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U;->m:Lcom/kamcord/android/core/KC_U$KC_b;

    const/4 v1, 0x0

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KC_U$KC_b;->a(Lcom/kamcord/android/core/KC_U$KC_b;I)V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final declared-synchronized c()V
    .locals 2

    monitor-enter p0

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U;->m:Lcom/kamcord/android/core/KC_U$KC_b;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U;->m:Lcom/kamcord/android/core/KC_U$KC_b;

    const/4 v1, 0x2

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KC_U$KC_b;->a(Lcom/kamcord/android/core/KC_U$KC_b;I)V
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

.method public final declared-synchronized d()V
    .locals 2

    monitor-enter p0

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U;->m:Lcom/kamcord/android/core/KC_U$KC_b;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U;->m:Lcom/kamcord/android/core/KC_U$KC_b;

    const/4 v1, 0x3

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KC_U$KC_b;->a(Lcom/kamcord/android/core/KC_U$KC_b;I)V
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

.method public final declared-synchronized e()V
    .locals 2

    monitor-enter p0

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U;->m:Lcom/kamcord/android/core/KC_U$KC_b;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U;->m:Lcom/kamcord/android/core/KC_U$KC_b;

    const/4 v1, 0x1

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KC_U$KC_b;->a(Lcom/kamcord/android/core/KC_U$KC_b;I)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U;->m:Lcom/kamcord/android/core/KC_U$KC_b;

    const v1, 0x7fffffff

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KC_U$KC_b;->a(Lcom/kamcord/android/core/KC_U$KC_b;I)V
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

.method public final declared-synchronized f()V
    .locals 2

    monitor-enter p0

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U;->m:Lcom/kamcord/android/core/KC_U$KC_b;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U;->m:Lcom/kamcord/android/core/KC_U$KC_b;

    const/4 v1, 0x5

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KC_U$KC_b;->a(Lcom/kamcord/android/core/KC_U$KC_b;I)V
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
