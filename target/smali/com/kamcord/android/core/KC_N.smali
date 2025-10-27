.class final Lcom/kamcord/android/core/KC_N;
.super Lcom/kamcord/android/core/KC_M;


# instance fields
.field private d:Lcom/kamcord/android/core/KC_l;


# direct methods
.method constructor <init>(Lcom/kamcord/android/core/KC_x;Lcom/kamcord/android/core/KC_V;)V
    .locals 1

    invoke-direct {p0, p1, p2}, Lcom/kamcord/android/core/KC_M;-><init>(Lcom/kamcord/android/core/KC_x;Lcom/kamcord/android/core/KC_V;)V

    new-instance v0, Lcom/kamcord/android/core/KC_w;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_w;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_N;->a:Lcom/kamcord/android/core/KC_w;

    return-void
.end method


# virtual methods
.method public final a(Lcom/kamcord/android/core/KC_F;)V
    .locals 6

    iget-object v0, p0, Lcom/kamcord/android/core/KC_N;->a:Lcom/kamcord/android/core/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_w;->a()V

    invoke-virtual {p0, p1}, Lcom/kamcord/android/core/KC_N;->b(Lcom/kamcord/android/core/KC_F;)J

    move-result-wide v0

    iget-object v2, p1, Lcom/kamcord/android/core/KC_F;->a:Lcom/kamcord/android/core/KC_y;

    const/4 v3, 0x1

    iput-boolean v3, v2, Lcom/kamcord/android/core/KC_y;->g:Z

    iget-object v2, p0, Lcom/kamcord/android/core/KC_N;->a:Lcom/kamcord/android/core/KC_w;

    long-to-double v0, v0

    const-wide v4, 0x3e112e0be826d695L    # 1.0E-9

    mul-double/2addr v0, v4

    invoke-virtual {v2, v0, v1}, Lcom/kamcord/android/core/KC_w;->a(D)V

    return-void
.end method

.method final a(Lcom/kamcord/android/core/KC_y;J)Z
    .locals 6

    const/4 v0, 0x0

    if-eqz p1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    if-eqz v1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v1}, Lcom/kamcord/android/core/KC_l;->isAlive()Z

    move-result v1

    if-eqz v1, :cond_0

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_N;->g()Z

    move-result v1

    if-eqz v1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    new-instance v2, Lcom/kamcord/android/core/KC_g;

    iget-object v3, p0, Lcom/kamcord/android/core/KC_N;->b:Lcom/kamcord/android/core/KC_x;

    iget-object v4, p0, Lcom/kamcord/android/core/KC_N;->c:Lcom/kamcord/android/core/KC_y;

    invoke-direct {v2, v3, v0, p1, v4}, Lcom/kamcord/android/core/KC_g;-><init>(Lcom/kamcord/android/core/KC_x;ZLcom/kamcord/android/core/KC_y;Lcom/kamcord/android/core/KC_y;)V

    invoke-virtual {v1, v2}, Lcom/kamcord/android/core/KC_l;->a(Lcom/kamcord/android/core/KC_g;)V

    new-instance v0, Lcom/kamcord/android/core/KC_F;

    invoke-direct {v0, p2, p3}, Lcom/kamcord/android/core/KC_F;-><init>(J)V

    iput-object p1, v0, Lcom/kamcord/android/core/KC_F;->a:Lcom/kamcord/android/core/KC_y;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v1, v0}, Lcom/kamcord/android/core/KC_l;->a(Lcom/kamcord/android/core/KC_F;)V

    const/4 v0, 0x1

    :cond_0
    return v0
.end method

.method final d()V
    .locals 2

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_N;->h()V

    new-instance v0, Lcom/kamcord/android/core/KC_l;

    invoke-direct {v0, p0}, Lcom/kamcord/android/core/KC_l;-><init>(Lcom/kamcord/android/core/KC_j;)V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    monitor-enter v1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->start()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->e()V

    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method final e()V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->isAlive()Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    monitor-enter v1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->f()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->d()V

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_N;->d:Lcom/kamcord/android/core/KC_l;

    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :cond_0
    return-void

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method
