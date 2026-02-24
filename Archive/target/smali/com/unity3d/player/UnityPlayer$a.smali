.class final Lcom/unity3d/player/UnityPlayer$a;
.super Ljava/lang/Thread;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/unity3d/player/UnityPlayer;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x2
    name = "a"
.end annotation


# instance fields
.field volatile a:Z

.field volatile b:Z

.field volatile c:Z

.field final synthetic d:Lcom/unity3d/player/UnityPlayer;


# direct methods
.method private constructor <init>(Lcom/unity3d/player/UnityPlayer;)V
    .locals 0

    iput-object p1, p0, Lcom/unity3d/player/UnityPlayer$a;->d:Lcom/unity3d/player/UnityPlayer;

    invoke-direct {p0}, Ljava/lang/Thread;-><init>()V

    return-void
.end method

.method synthetic constructor <init>(Lcom/unity3d/player/UnityPlayer;B)V
    .locals 0

    invoke-direct {p0, p1}, Lcom/unity3d/player/UnityPlayer$a;-><init>(Lcom/unity3d/player/UnityPlayer;)V

    return-void
.end method


# virtual methods
.method public final a()V
    .locals 1

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/unity3d/player/UnityPlayer$a;->b:Z

    monitor-enter p0

    :try_start_0
    invoke-virtual {p0}, Ljava/lang/Object;->notify()V

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    throw v0
.end method

.method public final a(Z)V
    .locals 1

    iput-boolean p1, p0, Lcom/unity3d/player/UnityPlayer$a;->c:Z

    monitor-enter p0

    :try_start_0
    invoke-virtual {p0}, Ljava/lang/Object;->notify()V

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    throw v0
.end method

.method public final b()V
    .locals 1

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/unity3d/player/UnityPlayer$a;->a:Z

    monitor-enter p0

    :try_start_0
    invoke-virtual {p0}, Ljava/lang/Object;->notify()V

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    throw v0
.end method

.method public final c()V
    .locals 1

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/unity3d/player/UnityPlayer$a;->a:Z

    monitor-enter p0

    :try_start_0
    invoke-virtual {p0}, Ljava/lang/Object;->notify()V

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    throw v0
.end method

.method public final run()V
    .locals 1

    const-string v0, "UnityMain"

    invoke-virtual {p0, v0}, Lcom/unity3d/player/UnityPlayer$a;->setName(Ljava/lang/String;)V

    :cond_0
    :goto_0
    :try_start_0
    iget-boolean v0, p0, Lcom/unity3d/player/UnityPlayer$a;->b:Z

    if-nez v0, :cond_2

    monitor-enter p0
    :try_end_0
    .catch Ljava/lang/InterruptedException; {:try_start_0 .. :try_end_0} :catch_0

    :try_start_1
    invoke-virtual {p0}, Ljava/lang/Object;->wait()V

    monitor-exit p0
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    :cond_1
    :goto_1
    :try_start_2
    iget-boolean v0, p0, Lcom/unity3d/player/UnityPlayer$a;->b:Z

    if-nez v0, :cond_3

    iget-boolean v0, p0, Lcom/unity3d/player/UnityPlayer$a;->a:Z

    if-nez v0, :cond_3

    iget-object v0, p0, Lcom/unity3d/player/UnityPlayer$a;->d:Lcom/unity3d/player/UnityPlayer;

    invoke-virtual {v0}, Lcom/unity3d/player/UnityPlayer;->executeGLThreadJobs()V

    iget-object v0, p0, Lcom/unity3d/player/UnityPlayer$a;->d:Lcom/unity3d/player/UnityPlayer;

    invoke-virtual {v0}, Lcom/unity3d/player/UnityPlayer;->isFinishing()Z

    move-result v0

    if-nez v0, :cond_1

    iget-object v0, p0, Lcom/unity3d/player/UnityPlayer$a;->d:Lcom/unity3d/player/UnityPlayer;

    invoke-static {v0}, Lcom/unity3d/player/UnityPlayer;->a(Lcom/unity3d/player/UnityPlayer;)Z

    move-result v0

    if-nez v0, :cond_1

    iget-object v0, p0, Lcom/unity3d/player/UnityPlayer$a;->d:Lcom/unity3d/player/UnityPlayer;

    invoke-static {v0}, Lcom/unity3d/player/UnityPlayer;->b(Lcom/unity3d/player/UnityPlayer;)V
    :try_end_2
    .catch Ljava/lang/InterruptedException; {:try_start_2 .. :try_end_2} :catch_0

    goto :goto_1

    :catch_0
    move-exception v0

    :cond_2
    return-void

    :catchall_0
    move-exception v0

    :try_start_3
    monitor-exit p0
    :try_end_3
    .catchall {:try_start_3 .. :try_end_3} :catchall_0

    :try_start_4
    throw v0

    :cond_3
    iget-boolean v0, p0, Lcom/unity3d/player/UnityPlayer$a;->c:Z

    if-nez v0, :cond_0

    iget-object v0, p0, Lcom/unity3d/player/UnityPlayer$a;->d:Lcom/unity3d/player/UnityPlayer;

    invoke-virtual {v0}, Lcom/unity3d/player/UnityPlayer;->executeGLThreadJobs()V
    :try_end_4
    .catch Ljava/lang/InterruptedException; {:try_start_4 .. :try_end_4} :catch_0

    goto :goto_0
.end method
