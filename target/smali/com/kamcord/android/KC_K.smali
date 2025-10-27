.class public final Lcom/kamcord/android/KC_K;
.super Ljava/lang/Object;

# interfaces
.implements Lcom/kamcord/android/KC_E;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_K$KC_a;
    }
.end annotation


# instance fields
.field private a:Landroid/media/MediaPlayer;

.field private b:Landroid/media/MediaPlayer;

.field private c:Z

.field private d:Lcom/kamcord/android/KC_K$KC_a;

.field private e:Z

.field private f:Z


# direct methods
.method constructor <init>(Landroid/media/MediaPlayer;Landroid/media/MediaPlayer;)V
    .locals 2

    const/4 v1, 0x0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->b:Landroid/media/MediaPlayer;

    iput-boolean v1, p0, Lcom/kamcord/android/KC_K;->c:Z

    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->a:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    iput-boolean v1, p0, Lcom/kamcord/android/KC_K;->e:Z

    iput-boolean v1, p0, Lcom/kamcord/android/KC_K;->f:Z

    iput-object p1, p0, Lcom/kamcord/android/KC_K;->a:Landroid/media/MediaPlayer;

    iput-object p2, p0, Lcom/kamcord/android/KC_K;->b:Landroid/media/MediaPlayer;

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->b:Landroid/media/MediaPlayer;

    if-eqz v0, :cond_0

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/KC_K;->c:Z

    :cond_0
    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_K;)Lcom/kamcord/android/KC_K$KC_a;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    return-object v0
.end method

.method static synthetic b(Lcom/kamcord/android/KC_K;)Landroid/media/MediaPlayer;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->a:Landroid/media/MediaPlayer;

    return-object v0
.end method

.method static synthetic c(Lcom/kamcord/android/KC_K;)Landroid/media/MediaPlayer;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->b:Landroid/media/MediaPlayer;

    return-object v0
.end method

.method private declared-synchronized h()V
    .locals 4

    monitor-enter p0

    :try_start_0
    iget-boolean v0, p0, Lcom/kamcord/android/KC_K;->c:Z
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    if-nez v0, :cond_0

    :goto_0
    monitor-exit p0

    return-void

    :cond_0
    :try_start_1
    new-instance v0, Landroid/os/Handler;

    invoke-direct {v0}, Landroid/os/Handler;-><init>()V

    new-instance v1, Lcom/kamcord/android/KC_K$1;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_K$1;-><init>(Lcom/kamcord/android/KC_K;)V

    const-wide/16 v2, 0x64

    invoke-virtual {v0, v1, v2, v3}, Landroid/os/Handler;->postDelayed(Ljava/lang/Runnable;J)Z
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method


# virtual methods
.method public final declared-synchronized a(Landroid/media/MediaPlayer;)V
    .locals 1

    monitor-enter p0

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_K;->a:Landroid/media/MediaPlayer;

    if-ne p1, v0, :cond_0

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/KC_K;->e:Z

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_K;->b:Landroid/media/MediaPlayer;

    if-ne p1, v0, :cond_1

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/KC_K;->f:Z

    :cond_1
    iget-boolean v0, p0, Lcom/kamcord/android/KC_K;->e:Z

    if-eqz v0, :cond_3

    iget-boolean v0, p0, Lcom/kamcord/android/KC_K;->f:Z

    if-nez v0, :cond_2

    iget-boolean v0, p0, Lcom/kamcord/android/KC_K;->c:Z

    if-nez v0, :cond_3

    :cond_2
    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->d:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_K;->e:Z

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_K;->f:Z
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :cond_3
    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final declared-synchronized a(Ljava/lang/String;Ljava/lang/String;)V
    .locals 3
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/io/IOException;
        }
    .end annotation

    monitor-enter p0

    :try_start_0
    const-string v0, "ReplayPlayerControl"

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "setDataSource("

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, ")"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->a:Lcom/kamcord/android/KC_K$KC_a;

    if-ne v0, v1, :cond_2

    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->b:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->a:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p1}, Landroid/media/MediaPlayer;->setDataSource(Ljava/lang/String;)V

    iget-boolean v0, p0, Lcom/kamcord/android/KC_K;->c:Z

    if-eqz v0, :cond_0

    if-eqz p2, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->b:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p2}, Landroid/media/MediaPlayer;->setDataSource(Ljava/lang/String;)V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :cond_0
    :goto_0
    monitor-exit p0

    return-void

    :cond_1
    :try_start_1
    iget-object v0, p0, Lcom/kamcord/android/KC_K;->b:Landroid/media/MediaPlayer;

    invoke-virtual {v0}, Landroid/media/MediaPlayer;->release()V

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->b:Landroid/media/MediaPlayer;

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_K;->c:Z
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0

    :cond_2
    :try_start_2
    const-string v0, "ReplayPlayerControl"

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Could not set data source in state "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v2, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "!"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->b(Ljava/lang/String;Ljava/lang/String;)I

    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->i:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;
    :try_end_2
    .catchall {:try_start_2 .. :try_end_2} :catchall_0

    goto :goto_0
.end method

.method public final declared-synchronized a()Z
    .locals 2

    monitor-enter p0

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->h:Lcom/kamcord/android/KC_K$KC_a;
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    if-ne v0, v1, :cond_0

    const/4 v0, 0x1

    :goto_0
    monitor-exit p0

    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final declared-synchronized a(I)Z
    .locals 1

    const/4 v0, 0x1

    monitor-enter p0

    if-ne p1, v0, :cond_0

    :goto_0
    monitor-exit p0

    return v0

    :cond_0
    :try_start_0
    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->i:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    const/4 v0, 0x0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final declared-synchronized b()V
    .locals 1

    monitor-enter p0

    :try_start_0
    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->j:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->a:Landroid/media/MediaPlayer;

    invoke-virtual {v0}, Landroid/media/MediaPlayer;->release()V

    iget-boolean v0, p0, Lcom/kamcord/android/KC_K;->c:Z

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->b:Landroid/media/MediaPlayer;

    invoke-virtual {v0}, Landroid/media/MediaPlayer;->release()V
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

.method public final declared-synchronized c()V
    .locals 1

    monitor-enter p0

    :try_start_0
    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->a:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->a:Landroid/media/MediaPlayer;

    invoke-virtual {v0}, Landroid/media/MediaPlayer;->reset()V

    iget-boolean v0, p0, Lcom/kamcord/android/KC_K;->c:Z

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->b:Landroid/media/MediaPlayer;

    invoke-virtual {v0}, Landroid/media/MediaPlayer;->reset()V
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

.method public final declared-synchronized canPause()Z
    .locals 2

    monitor-enter p0

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->e:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->g:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->h:Lcom/kamcord/android/KC_K$KC_a;
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    if-ne v0, v1, :cond_1

    :cond_0
    const/4 v0, 0x1

    :goto_0
    monitor-exit p0

    return v0

    :cond_1
    const/4 v0, 0x0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final canSeekBackward()Z
    .locals 1

    const/4 v0, 0x1

    return v0
.end method

.method public final canSeekForward()Z
    .locals 1

    const/4 v0, 0x1

    return v0
.end method

.method public final declared-synchronized d()V
    .locals 3

    monitor-enter p0

    :try_start_0
    const-string v0, "ReplayPlayerControl"

    const-string v1, "prepareAsync()"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->b:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->f:Lcom/kamcord/android/KC_K$KC_a;

    if-ne v0, v1, :cond_2

    :cond_0
    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->c:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->a:Landroid/media/MediaPlayer;

    invoke-virtual {v0}, Landroid/media/MediaPlayer;->prepareAsync()V

    iget-boolean v0, p0, Lcom/kamcord/android/KC_K;->c:Z

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->b:Landroid/media/MediaPlayer;

    invoke-virtual {v0}, Landroid/media/MediaPlayer;->prepareAsync()V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :cond_1
    :goto_0
    monitor-exit p0

    return-void

    :cond_2
    :try_start_1
    const-string v0, "ReplayPlayerControl"

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Could not call prepareAsync() in state "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v2, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "!"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->b(Ljava/lang/String;Ljava/lang/String;)I

    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->i:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final e()Z
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->d:Lcom/kamcord/android/KC_K$KC_a;

    if-ne v0, v1, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public final declared-synchronized f()V
    .locals 1

    monitor-enter p0

    :try_start_0
    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->h:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final declared-synchronized g()V
    .locals 2

    monitor-enter p0

    :try_start_0
    const-string v0, "ReplayPlayerControl"

    const-string v1, "onSeekComplete"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;Ljava/lang/String;)I
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final getAudioSessionId()I
    .locals 1

    const/4 v0, 0x0

    return v0
.end method

.method public final getBufferPercentage()I
    .locals 1

    const/16 v0, 0x64

    return v0
.end method

.method public final declared-synchronized getCurrentPosition()I
    .locals 3

    monitor-enter p0

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->i:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->a:Landroid/media/MediaPlayer;

    invoke-virtual {v0}, Landroid/media/MediaPlayer;->getCurrentPosition()I
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    move-result v0

    :goto_0
    monitor-exit p0

    return v0

    :cond_0
    :try_start_1
    const-string v0, "ReplayPlayerControl"

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Could not get current position in state "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v2, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "!"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->b(Ljava/lang/String;Ljava/lang/String;)I
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    const/4 v0, -0x1

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final declared-synchronized getDuration()I
    .locals 3

    monitor-enter p0

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->d:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->e:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->g:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->f:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->h:Lcom/kamcord/android/KC_K$KC_a;

    if-ne v0, v1, :cond_1

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_K;->a:Landroid/media/MediaPlayer;

    invoke-virtual {v0}, Landroid/media/MediaPlayer;->getDuration()I
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    move-result v0

    :goto_0
    monitor-exit p0

    return v0

    :cond_1
    :try_start_1
    const-string v0, "ReplayPlayerControl"

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Could not get duration in state "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v2, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "!"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->b(Ljava/lang/String;Ljava/lang/String;)I
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    const/4 v0, -0x1

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final declared-synchronized isPlaying()Z
    .locals 2

    monitor-enter p0

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->e:Lcom/kamcord/android/KC_K$KC_a;
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    if-ne v0, v1, :cond_0

    const/4 v0, 0x1

    :goto_0
    monitor-exit p0

    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final declared-synchronized pause()V
    .locals 3

    monitor-enter p0

    :try_start_0
    const-string v0, "ReplayPlayerControl"

    const-string v1, "pause()"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->e:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->g:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->h:Lcom/kamcord/android/KC_K$KC_a;

    if-ne v0, v1, :cond_2

    :cond_0
    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->g:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->a:Landroid/media/MediaPlayer;

    invoke-virtual {v0}, Landroid/media/MediaPlayer;->pause()V

    iget-boolean v0, p0, Lcom/kamcord/android/KC_K;->c:Z

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->b:Landroid/media/MediaPlayer;

    invoke-virtual {v0}, Landroid/media/MediaPlayer;->pause()V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :cond_1
    :goto_0
    monitor-exit p0

    return-void

    :cond_2
    :try_start_1
    const-string v0, "ReplayPlayerControl"

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Could not pause playback in state "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v2, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "!"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->b(Ljava/lang/String;Ljava/lang/String;)I

    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->i:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final declared-synchronized seekTo(I)V
    .locals 3

    monitor-enter p0

    :try_start_0
    const-string v0, "ReplayPlayerControl"

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "seekTo("

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, ")"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->d:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->e:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->g:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->h:Lcom/kamcord/android/KC_K$KC_a;

    if-ne v0, v1, :cond_2

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_K;->a:Landroid/media/MediaPlayer;

    invoke-virtual {v0, p1}, Landroid/media/MediaPlayer;->seekTo(I)V

    iget-boolean v0, p0, Lcom/kamcord/android/KC_K;->c:Z

    if-eqz v0, :cond_1

    invoke-direct {p0}, Lcom/kamcord/android/KC_K;->h()V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :cond_1
    :goto_0
    monitor-exit p0

    return-void

    :cond_2
    :try_start_1
    const-string v0, "ReplayPlayerControl"

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Could not seek in state "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v2, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "!"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->b(Ljava/lang/String;Ljava/lang/String;)I

    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->i:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final declared-synchronized start()V
    .locals 3

    monitor-enter p0

    :try_start_0
    const-string v0, "ReplayPlayerControl"

    const-string v1, "start()"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->d:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->e:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->g:Lcom/kamcord/android/KC_K$KC_a;

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->h:Lcom/kamcord/android/KC_K$KC_a;

    if-ne v0, v1, :cond_2

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_K;->a:Landroid/media/MediaPlayer;

    invoke-virtual {v0}, Landroid/media/MediaPlayer;->start()V

    iget-boolean v0, p0, Lcom/kamcord/android/KC_K;->c:Z

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_K;->b:Landroid/media/MediaPlayer;

    invoke-virtual {v0}, Landroid/media/MediaPlayer;->start()V

    invoke-direct {p0}, Lcom/kamcord/android/KC_K;->h()V

    :cond_1
    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->e:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :goto_0
    monitor-exit p0

    return-void

    :cond_2
    :try_start_1
    const-string v0, "ReplayPlayerControl"

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Could not start playback in state "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v2, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "!"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->b(Ljava/lang/String;Ljava/lang/String;)I

    sget-object v0, Lcom/kamcord/android/KC_K$KC_a;->i:Lcom/kamcord/android/KC_K$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_K;->d:Lcom/kamcord/android/KC_K$KC_a;
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method
