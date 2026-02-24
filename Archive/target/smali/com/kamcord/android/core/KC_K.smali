.class final Lcom/kamcord/android/core/KC_K;
.super Lcom/kamcord/android/core/KC_J;


# instance fields
.field private d:Lcom/kamcord/android/core/KC_l;


# direct methods
.method constructor <init>(Lcom/kamcord/android/core/KC_x;)V
    .locals 0

    invoke-direct {p0, p1}, Lcom/kamcord/android/core/KC_J;-><init>(Lcom/kamcord/android/core/KC_x;)V

    return-void
.end method


# virtual methods
.method public final a(Lcom/kamcord/android/core/KC_y;J)Z
    .locals 6

    const/4 v5, 0x1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->isAlive()Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

    monitor-enter v1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_K;->b:Lcom/kamcord/android/core/KC_x;

    const/4 v2, 0x1

    const/4 v3, 0x1

    iget-object v4, p0, Lcom/kamcord/android/core/KC_K;->c:Lcom/kamcord/android/core/KC_z;

    invoke-virtual {v0, v2, v3, p1, v4}, Lcom/kamcord/android/core/KC_x;->a(ZZLcom/kamcord/android/core/KC_y;Lcom/kamcord/android/core/KC_y;)V

    invoke-static {}, Landroid/opengl/GLES20;->glFinish()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

    new-instance v2, Lcom/kamcord/android/core/KC_F;

    invoke-direct {v2, p2, p3}, Lcom/kamcord/android/core/KC_F;-><init>(J)V

    invoke-virtual {v0, v2}, Lcom/kamcord/android/core/KC_l;->a(Lcom/kamcord/android/core/KC_F;)V

    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :cond_0
    return v5

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method final d()V
    .locals 2

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_K;->h()V

    new-instance v0, Lcom/kamcord/android/core/KC_l;

    invoke-direct {v0, p0}, Lcom/kamcord/android/core/KC_l;-><init>(Lcom/kamcord/android/core/KC_j;)V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

    monitor-enter v1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->start()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

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

    iget-object v0, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->isAlive()Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

    monitor-enter v1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->f()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_l;->d()V

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_K;->d:Lcom/kamcord/android/core/KC_l;

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
