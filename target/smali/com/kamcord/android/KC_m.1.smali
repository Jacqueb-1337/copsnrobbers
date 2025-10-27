.class public final Lcom/kamcord/android/KC_m;
.super Ljava/lang/Object;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_m$KC_a;
    }
.end annotation


# static fields
.field static a:Ljava/util/HashSet;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/HashSet",
            "<",
            "Lcom/kamcord/android/KC_m$KC_a;",
            ">;"
        }
    .end annotation
.end field

.field private static b:Lcom/kamcord/android/KC_m;


# instance fields
.field private c:Lcom/kamcord/android/KC_c;

.field private d:J

.field private e:Landroid/util/SparseArray;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Landroid/util/SparseArray",
            "<",
            "Lcom/kamcord/android/KC_a;",
            ">;"
        }
    .end annotation
.end field


# direct methods
.method static constructor <clinit>()V
    .locals 5

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    sput-object v0, Lcom/kamcord/android/KC_m;->a:Ljava/util/HashSet;

    new-instance v1, Lcom/kamcord/android/KC_m$KC_a;

    const-string v2, "RECORDING_COUNT"

    const/4 v3, 0x0

    const-string v4, "RECORDING_EVENT_V2"

    invoke-direct {v1, v2, v3, v4}, Lcom/kamcord/android/KC_m$KC_a;-><init>(Ljava/lang/String;ILjava/lang/String;)V

    invoke-virtual {v0, v1}, Ljava/util/HashSet;->add(Ljava/lang/Object;)Z

    sget-object v0, Lcom/kamcord/android/KC_m;->a:Ljava/util/HashSet;

    new-instance v1, Lcom/kamcord/android/KC_m$KC_a;

    const-string v2, "UI_OPEN_COUNT"

    const/4 v3, 0x1

    const-string v4, "UI_OPEN_EVENT_V2"

    invoke-direct {v1, v2, v3, v4}, Lcom/kamcord/android/KC_m$KC_a;-><init>(Ljava/lang/String;ILjava/lang/String;)V

    invoke-virtual {v0, v1}, Ljava/util/HashSet;->add(Ljava/lang/Object;)Z

    sget-object v0, Lcom/kamcord/android/KC_m;->a:Ljava/util/HashSet;

    new-instance v1, Lcom/kamcord/android/KC_m$KC_a;

    const-string v2, "REPLAY_COUNT"

    const/4 v3, 0x2

    const-string v4, "REPLAY_EVENT_V2"

    invoke-direct {v1, v2, v3, v4}, Lcom/kamcord/android/KC_m$KC_a;-><init>(Ljava/lang/String;ILjava/lang/String;)V

    invoke-virtual {v0, v1}, Ljava/util/HashSet;->add(Ljava/lang/Object;)Z

    sget-object v0, Lcom/kamcord/android/KC_m;->a:Ljava/util/HashSet;

    new-instance v1, Lcom/kamcord/android/KC_m$KC_a;

    const-string v2, "KAMCORD_VIDEO_UPLOAD_SUCCEEDED_COUNT"

    const/4 v3, 0x3

    const-string v4, "SHARE_EVENT_V2"

    invoke-direct {v1, v2, v3, v4}, Lcom/kamcord/android/KC_m$KC_a;-><init>(Ljava/lang/String;ILjava/lang/String;)V

    invoke-virtual {v0, v1}, Ljava/util/HashSet;->add(Ljava/lang/Object;)Z

    sget-object v0, Lcom/kamcord/android/KC_m;->a:Ljava/util/HashSet;

    new-instance v1, Lcom/kamcord/android/KC_m$KC_a;

    const-string v2, "VIDEO_VIEW_COUNT"

    const/4 v3, 0x4

    const-string v4, "VIDEO_VIEW_EVENT_V2"

    invoke-direct {v1, v2, v3, v4}, Lcom/kamcord/android/KC_m$KC_a;-><init>(Ljava/lang/String;ILjava/lang/String;)V

    invoke-virtual {v0, v1}, Ljava/util/HashSet;->add(Ljava/lang/Object;)Z

    sget-object v0, Lcom/kamcord/android/KC_m;->a:Ljava/util/HashSet;

    new-instance v1, Lcom/kamcord/android/KC_m$KC_a;

    const-string v2, "LAUNCH_COUNT"

    const/4 v3, 0x5

    const-string v4, "APP_LAUNCH_EVENT_V2"

    invoke-direct {v1, v2, v3, v4}, Lcom/kamcord/android/KC_m$KC_a;-><init>(Ljava/lang/String;ILjava/lang/String;)V

    invoke-virtual {v0, v1}, Ljava/util/HashSet;->add(Ljava/lang/Object;)Z

    sget-object v0, Lcom/kamcord/android/KC_m;->a:Ljava/util/HashSet;

    new-instance v1, Lcom/kamcord/android/KC_m$KC_a;

    const-string v2, "FACEBOOK_SHARE_SUCCEEDED_COUNT"

    const/4 v3, 0x6

    const-string v4, "FACEBOOK_SHARE_EVENT_V2"

    invoke-direct {v1, v2, v3, v4}, Lcom/kamcord/android/KC_m$KC_a;-><init>(Ljava/lang/String;ILjava/lang/String;)V

    invoke-virtual {v0, v1}, Ljava/util/HashSet;->add(Ljava/lang/Object;)Z

    sget-object v0, Lcom/kamcord/android/KC_m;->a:Ljava/util/HashSet;

    new-instance v1, Lcom/kamcord/android/KC_m$KC_a;

    const-string v2, "TWITTER_SHARE_SUCCEEDED_COUNT"

    const/4 v3, 0x7

    const-string v4, "TWITTER_SHARE_EVENT_V2"

    invoke-direct {v1, v2, v3, v4}, Lcom/kamcord/android/KC_m$KC_a;-><init>(Ljava/lang/String;ILjava/lang/String;)V

    invoke-virtual {v0, v1}, Ljava/util/HashSet;->add(Ljava/lang/Object;)Z

    return-void
.end method

.method constructor <init>()V
    .locals 4

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    new-instance v0, Landroid/util/SparseArray;

    invoke-direct {v0}, Landroid/util/SparseArray;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_m;->e:Landroid/util/SparseArray;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/KC_m;->a(Landroid/content/SharedPreferences;)V

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_m;->b(Landroid/content/SharedPreferences;)V

    const-string v1, "lastDataUpload"

    const-wide/16 v2, 0x0

    invoke-interface {v0, v1, v2, v3}, Landroid/content/SharedPreferences;->getLong(Ljava/lang/String;J)J

    move-result-wide v0

    iput-wide v0, p0, Lcom/kamcord/android/KC_m;->d:J

    invoke-direct {p0}, Lcom/kamcord/android/KC_m;->e()V

    new-instance v0, Lcom/kamcord/android/KC_c;

    invoke-direct {v0}, Lcom/kamcord/android/KC_c;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_m;->c:Lcom/kamcord/android/KC_c;

    iget-object v0, p0, Lcom/kamcord/android/KC_m;->c:Lcom/kamcord/android/KC_c;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_c;->start()V

    return-void
.end method

.method public static a()V
    .locals 6

    invoke-static {}, Lcom/kamcord/android/KC_m;->i()Lcom/kamcord/android/KC_m;

    move-result-object v1

    invoke-direct {v1}, Lcom/kamcord/android/KC_m;->f()Z

    move-result v0

    if-eqz v0, :cond_1

    new-instance v2, Landroid/os/Bundle;

    invoke-direct {v2}, Landroid/os/Bundle;-><init>()V

    sget-object v0, Lcom/kamcord/android/KC_m;->a:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v3

    :goto_0
    invoke-interface {v3}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v3}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_m$KC_a;

    iget-object v4, v0, Lcom/kamcord/android/KC_m$KC_a;->c:Ljava/lang/String;

    iget-object v5, v1, Lcom/kamcord/android/KC_m;->e:Landroid/util/SparseArray;

    iget v0, v0, Lcom/kamcord/android/KC_m$KC_a;->b:I

    invoke-virtual {v5, v0}, Landroid/util/SparseArray;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_a;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_a;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v2, v4, v0}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    :cond_0
    new-instance v0, Landroid/os/Message;

    invoke-direct {v0}, Landroid/os/Message;-><init>()V

    const/4 v3, 0x2

    iput v3, v0, Landroid/os/Message;->what:I

    invoke-virtual {v0, v2}, Landroid/os/Message;->setData(Landroid/os/Bundle;)V

    iget-object v1, v1, Lcom/kamcord/android/KC_m;->c:Lcom/kamcord/android/KC_c;

    iget-object v1, v1, Lcom/kamcord/android/KC_c;->a:Lcom/kamcord/android/KC_ac;

    invoke-virtual {v1, v0}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    :cond_1
    return-void
.end method

.method public static a(I)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/KC_m;->i()Lcom/kamcord/android/KC_m;

    move-result-object v0

    invoke-direct {v0, p0}, Lcom/kamcord/android/KC_m;->c(I)V

    return-void
.end method

.method private static a(Landroid/content/SharedPreferences;)V
    .locals 4

    :try_start_0
    new-instance v0, Ljava/io/File;

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    invoke-static {}, Landroid/os/Environment;->getExternalStorageDirectory()Ljava/io/File;

    move-result-object v2

    invoke-virtual {v2}, Ljava/io/File;->getPath()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "/Kamcord/.guid"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-virtual {v0}, Ljava/io/File;->length()J

    move-result-wide v2

    long-to-int v2, v2

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(I)V

    new-instance v2, Ljava/util/Scanner;

    invoke-direct {v2, v0}, Ljava/util/Scanner;-><init>(Ljava/io/File;)V
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    :try_start_1
    invoke-virtual {v2}, Ljava/util/Scanner;->nextLine()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    move-result-object v0

    :try_start_2
    invoke-virtual {v2}, Ljava/util/Scanner;->close()V

    invoke-static {v0}, Ljava/util/UUID;->fromString(Ljava/lang/String;)Ljava/util/UUID;
    :try_end_2
    .catch Ljava/lang/Exception; {:try_start_2 .. :try_end_2} :catch_0

    :goto_0
    invoke-interface {p0}, Landroid/content/SharedPreferences;->edit()Landroid/content/SharedPreferences$Editor;

    move-result-object v1

    const-string v2, "uuid"

    invoke-interface {v1, v2, v0}, Landroid/content/SharedPreferences$Editor;->putString(Ljava/lang/String;Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    invoke-interface {v0}, Landroid/content/SharedPreferences$Editor;->apply()V

    return-void

    :catchall_0
    move-exception v0

    :try_start_3
    invoke-virtual {v2}, Ljava/util/Scanner;->close()V

    throw v0
    :try_end_3
    .catch Ljava/lang/Exception; {:try_start_3 .. :try_end_3} :catch_0

    :catch_0
    move-exception v0

    invoke-static {}, Ljava/util/UUID;->randomUUID()Ljava/util/UUID;

    move-result-object v0

    invoke-virtual {v0}, Ljava/util/UUID;->toString()Ljava/lang/String;

    move-result-object v0

    :try_start_4
    new-instance v1, Ljava/io/FileWriter;

    new-instance v2, Ljava/lang/StringBuilder;

    invoke-direct {v2}, Ljava/lang/StringBuilder;-><init>()V

    invoke-static {}, Landroid/os/Environment;->getExternalStorageDirectory()Ljava/io/File;

    move-result-object v3

    invoke-virtual {v3}, Ljava/io/File;->getPath()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "/Kamcord/.guid"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v1, v2}, Ljava/io/FileWriter;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, v0}, Ljava/io/FileWriter;->write(Ljava/lang/String;)V

    invoke-virtual {v1}, Ljava/io/FileWriter;->close()V
    :try_end_4
    .catch Ljava/io/IOException; {:try_start_4 .. :try_end_4} :catch_1

    goto :goto_0

    :catch_1
    move-exception v1

    invoke-virtual {v1}, Ljava/io/IOException;->printStackTrace()V

    goto :goto_0
.end method

.method public static b()V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/KC_m;->i()Lcom/kamcord/android/KC_m;

    move-result-object v0

    invoke-direct {v0}, Lcom/kamcord/android/KC_m;->g()V

    return-void
.end method

.method public static b(I)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/KC_m;->i()Lcom/kamcord/android/KC_m;

    move-result-object v0

    invoke-direct {v0, p0}, Lcom/kamcord/android/KC_m;->d(I)V

    return-void
.end method

.method private b(Landroid/content/SharedPreferences;)V
    .locals 7

    sget-object v0, Lcom/kamcord/android/KC_m;->a:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_m$KC_a;

    iget-object v2, p0, Lcom/kamcord/android/KC_m;->e:Landroid/util/SparseArray;

    iget v3, v0, Lcom/kamcord/android/KC_m$KC_a;->b:I

    iget-object v4, v0, Lcom/kamcord/android/KC_m$KC_a;->c:Ljava/lang/String;

    new-instance v5, Ljava/lang/StringBuilder;

    const-string v6, "{\"name\":\""

    invoke-direct {v5, v6}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v0, v0, Lcom/kamcord/android/KC_m$KC_a;->a:Ljava/lang/String;

    invoke-virtual {v5, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v5, "\", \"bins\":[]}"

    invoke-virtual {v0, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-interface {p1, v4, v0}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/KC_a;->a(Ljava/lang/String;)Lcom/kamcord/android/KC_a;

    move-result-object v0

    invoke-virtual {v2, v3, v0}, Landroid/util/SparseArray;->append(ILjava/lang/Object;)V

    goto :goto_0

    :cond_0
    return-void
.end method

.method public static c()V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/KC_m;->i()Lcom/kamcord/android/KC_m;

    move-result-object v0

    invoke-direct {v0}, Lcom/kamcord/android/KC_m;->h()V

    return-void
.end method

.method private declared-synchronized c(I)V
    .locals 3

    monitor-enter p0

    :try_start_0
    new-instance v0, Landroid/os/Bundle;

    invoke-direct {v0}, Landroid/os/Bundle;-><init>()V

    const-string v1, "event"

    invoke-virtual {v0, v1, p1}, Landroid/os/Bundle;->putInt(Ljava/lang/String;I)V

    new-instance v1, Landroid/os/Message;

    invoke-direct {v1}, Landroid/os/Message;-><init>()V

    const/4 v2, 0x3

    iput v2, v1, Landroid/os/Message;->what:I

    invoke-virtual {v1, v0}, Landroid/os/Message;->setData(Landroid/os/Bundle;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_m;->c:Lcom/kamcord/android/KC_c;

    iget-object v0, v0, Lcom/kamcord/android/KC_c;->a:Lcom/kamcord/android/KC_ac;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public static d()Ljava/lang/String;
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    const-string v1, "uuid"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences;->contains(Ljava/lang/String;)Z

    move-result v1

    if-nez v1, :cond_0

    invoke-static {v0}, Lcom/kamcord/android/KC_m;->a(Landroid/content/SharedPreferences;)V

    :cond_0
    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    const-string v1, "uuid"

    const-string v2, ""

    invoke-interface {v0, v1, v2}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method

.method private declared-synchronized d(I)V
    .locals 1

    monitor-enter p0

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_m;->e:Landroid/util/SparseArray;

    invoke-virtual {v0, p1}, Landroid/util/SparseArray;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_a;

    if-eqz v0, :cond_0

    invoke-virtual {v0}, Lcom/kamcord/android/KC_a;->a()V

    :cond_0
    invoke-direct {p0}, Lcom/kamcord/android/KC_m;->e()V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method private e()V
    .locals 5

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    invoke-interface {v0}, Landroid/content/SharedPreferences;->edit()Landroid/content/SharedPreferences$Editor;

    move-result-object v1

    sget-object v0, Lcom/kamcord/android/KC_m;->a:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v2

    :goto_0
    invoke-interface {v2}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v2}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_m$KC_a;

    iget-object v3, v0, Lcom/kamcord/android/KC_m$KC_a;->c:Ljava/lang/String;

    iget-object v4, p0, Lcom/kamcord/android/KC_m;->e:Landroid/util/SparseArray;

    iget v0, v0, Lcom/kamcord/android/KC_m$KC_a;->b:I

    invoke-virtual {v4, v0}, Landroid/util/SparseArray;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_a;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_a;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-interface {v1, v3, v0}, Landroid/content/SharedPreferences$Editor;->putString(Ljava/lang/String;Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    goto :goto_0

    :cond_0
    const-string v0, "lastDataUpload"

    iget-wide v2, p0, Lcom/kamcord/android/KC_m;->d:J

    invoke-interface {v1, v0, v2, v3}, Landroid/content/SharedPreferences$Editor;->putLong(Ljava/lang/String;J)Landroid/content/SharedPreferences$Editor;

    invoke-interface {v1}, Landroid/content/SharedPreferences$Editor;->commit()Z

    return-void
.end method

.method private declared-synchronized f()Z
    .locals 4

    monitor-enter p0

    :try_start_0
    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J

    move-result-wide v0

    const-wide/16 v2, 0x3e8

    div-long/2addr v0, v2

    iget-wide v2, p0, Lcom/kamcord/android/KC_m;->d:J
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    sub-long/2addr v0, v2

    const-wide/32 v2, 0xa8c0

    cmp-long v0, v0, v2

    if-lez v0, :cond_0

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

.method private declared-synchronized g()V
    .locals 5

    monitor-enter p0

    :try_start_0
    const-string v0, "KamcordAnalytics"

    const-string v1, "Data successfully uploaded."

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;Ljava/lang/String;)I

    sget-object v0, Lcom/kamcord/android/KC_m;->a:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_m$KC_a;

    iget-object v2, p0, Lcom/kamcord/android/KC_m;->e:Landroid/util/SparseArray;

    iget v3, v0, Lcom/kamcord/android/KC_m$KC_a;->b:I

    new-instance v4, Lcom/kamcord/android/KC_a;

    iget-object v0, v0, Lcom/kamcord/android/KC_m$KC_a;->a:Ljava/lang/String;

    invoke-direct {v4, v0}, Lcom/kamcord/android/KC_a;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v3, v4}, Landroid/util/SparseArray;->put(ILjava/lang/Object;)V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0

    :cond_0
    :try_start_1
    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J

    move-result-wide v0

    const-wide/16 v2, 0x3e8

    div-long/2addr v0, v2

    iput-wide v0, p0, Lcom/kamcord/android/KC_m;->d:J

    invoke-direct {p0}, Lcom/kamcord/android/KC_m;->e()V
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    monitor-exit p0

    return-void
.end method

.method private declared-synchronized h()V
    .locals 4

    monitor-enter p0

    :try_start_0
    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J

    move-result-wide v0

    const-wide/16 v2, 0x3e8

    div-long/2addr v0, v2

    iput-wide v0, p0, Lcom/kamcord/android/KC_m;->d:J

    invoke-direct {p0}, Lcom/kamcord/android/KC_m;->e()V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method private static declared-synchronized i()Lcom/kamcord/android/KC_m;
    .locals 2

    const-class v1, Lcom/kamcord/android/KC_m;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/KC_m;->b:Lcom/kamcord/android/KC_m;

    if-nez v0, :cond_0

    new-instance v0, Lcom/kamcord/android/KC_m;

    invoke-direct {v0}, Lcom/kamcord/android/KC_m;-><init>()V

    sput-object v0, Lcom/kamcord/android/KC_m;->b:Lcom/kamcord/android/KC_m;

    :cond_0
    sget-object v0, Lcom/kamcord/android/KC_m;->b:Lcom/kamcord/android/KC_m;
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit v1

    return-object v0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method
