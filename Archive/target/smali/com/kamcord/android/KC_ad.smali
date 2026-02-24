.class public abstract Lcom/kamcord/android/KC_ad;
.super Landroid/os/HandlerThread;


# static fields
.field private static b:Ljava/util/Set;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/Set",
            "<",
            "Lcom/kamcord/android/KC_ad;",
            ">;"
        }
    .end annotation
.end field


# instance fields
.field protected a:Lcom/kamcord/android/KC_ac;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    sput-object v0, Lcom/kamcord/android/KC_ad;->b:Ljava/util/Set;

    return-void
.end method

.method protected constructor <init>(Ljava/lang/String;)V
    .locals 1

    invoke-direct {p0, p1}, Landroid/os/HandlerThread;-><init>(Ljava/lang/String;)V

    new-instance v0, Lcom/kamcord/android/KC_t;

    invoke-direct {v0}, Lcom/kamcord/android/KC_t;-><init>()V

    invoke-virtual {p0, v0}, Lcom/kamcord/android/KC_ad;->setUncaughtExceptionHandler(Ljava/lang/Thread$UncaughtExceptionHandler;)V

    return-void
.end method

.method private static declared-synchronized a(Lcom/kamcord/android/KC_ad;)V
    .locals 2

    const-class v1, Lcom/kamcord/android/KC_ad;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/KC_ad;->b:Ljava/util/Set;

    invoke-interface {v0, p0}, Ljava/util/Set;->add(Ljava/lang/Object;)Z
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit v1

    return-void

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method static declared-synchronized b()V
    .locals 4

    const-class v1, Lcom/kamcord/android/KC_ad;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/KC_ad;->b:Ljava/util/Set;

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v2

    :goto_0
    invoke-interface {v2}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v2}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_ad;

    iget-object v0, v0, Lcom/kamcord/android/KC_ad;->a:Lcom/kamcord/android/KC_ac;

    const/4 v3, 0x1

    iput-boolean v3, v0, Lcom/kamcord/android/KC_ac;->a:Z
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    :cond_0
    :try_start_1
    sget-object v0, Lcom/kamcord/android/KC_ad;->b:Ljava/util/Set;

    invoke-interface {v0}, Ljava/util/Set;->clear()V
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    monitor-exit v1

    return-void
.end method

.method private static declared-synchronized b(Lcom/kamcord/android/KC_ad;)V
    .locals 2

    const-class v1, Lcom/kamcord/android/KC_ad;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/KC_ad;->b:Ljava/util/Set;

    invoke-interface {v0, p0}, Ljava/util/Set;->remove(Ljava/lang/Object;)Z
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit v1

    return-void

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method private declared-synchronized e()V
    .locals 1

    monitor-enter p0

    :try_start_0
    invoke-virtual {p0}, Lcom/kamcord/android/KC_ad;->a()Lcom/kamcord/android/KC_ac;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_ad;->a:Lcom/kamcord/android/KC_ac;
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method


# virtual methods
.method protected abstract a()Lcom/kamcord/android/KC_ac;
.end method

.method protected final c()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/KC_ad;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/KC_ad;->a:Lcom/kamcord/android/KC_ac;

    const v2, 0x7fffffff

    invoke-static {v1, v2}, Landroid/os/Message;->obtain(Landroid/os/Handler;I)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method

.method public final d()V
    .locals 3

    invoke-virtual {p0}, Lcom/kamcord/android/KC_ad;->c()V

    invoke-static {p0}, Lcom/kamcord/android/KC_ad;->b(Lcom/kamcord/android/KC_ad;)V

    :try_start_0
    invoke-virtual {p0}, Lcom/kamcord/android/KC_ad;->join()V
    :try_end_0
    .catch Ljava/lang/InterruptedException; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    return-void

    :catch_0
    move-exception v0

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "worker thread "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_ad;->getName()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " interrupted."

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/InterruptedException;->printStackTrace()V

    goto :goto_0
.end method

.method public declared-synchronized start()V
    .locals 2

    monitor-enter p0

    :try_start_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Starting thread: "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_ad;->getName()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-super {p0}, Landroid/os/HandlerThread;->start()V

    invoke-direct {p0}, Lcom/kamcord/android/KC_ad;->e()V

    invoke-static {p0}, Lcom/kamcord/android/KC_ad;->a(Lcom/kamcord/android/KC_ad;)V

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "...done starting thread: "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_ad;->getName()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method
