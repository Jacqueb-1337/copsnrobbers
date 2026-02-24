.class final Lcom/kamcord/android/core/KC_w;
.super Ljava/lang/Object;


# instance fields
.field private a:D

.field private b:D


# direct methods
.method constructor <init>()V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method final declared-synchronized a()V
    .locals 4

    monitor-enter p0

    :try_start_0
    iget-wide v0, p0, Lcom/kamcord/android/core/KC_w;->a:D

    iget-wide v2, p0, Lcom/kamcord/android/core/KC_w;->b:D
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    sub-double/2addr v0, v2

    const-wide v2, 0x3fc999999999999aL    # 0.2

    cmpl-double v2, v0, v2

    if-lez v2, :cond_1

    const-wide/16 v0, 0x5

    :try_start_1
    invoke-static {v0, v1}, Ljava/lang/Thread;->sleep(J)V
    :try_end_1
    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_0
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    :cond_0
    :goto_0
    monitor-exit p0

    return-void

    :cond_1
    const-wide v2, 0x3fb999999999999aL    # 0.1

    cmpl-double v0, v0, v2

    if-lez v0, :cond_0

    const-wide/16 v0, 0x1

    :try_start_2
    invoke-static {v0, v1}, Ljava/lang/Thread;->sleep(J)V
    :try_end_2
    .catch Ljava/lang/Exception; {:try_start_2 .. :try_end_2} :catch_0
    .catchall {:try_start_2 .. :try_end_2} :catchall_0

    goto :goto_0

    :catch_0
    move-exception v0

    :try_start_3
    const-string v0, "Sleep interrupted."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_3
    .catchall {:try_start_3 .. :try_end_3} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method final declared-synchronized a(D)V
    .locals 1

    monitor-enter p0

    :try_start_0
    iput-wide p1, p0, Lcom/kamcord/android/core/KC_w;->a:D
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method final declared-synchronized b(D)V
    .locals 1

    monitor-enter p0

    :try_start_0
    iput-wide p1, p0, Lcom/kamcord/android/core/KC_w;->b:D
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method
