.class public abstract Lcom/kamcord/a/a/f/KC_a;
.super Ljava/lang/Object;


# static fields
.field private static a:Lcom/kamcord/a/a/f/KC_a;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method

.method public static declared-synchronized a()Lcom/kamcord/a/a/f/KC_a;
    .locals 2

    const-class v1, Lcom/kamcord/a/a/f/KC_a;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/a/a/f/KC_a;->a:Lcom/kamcord/a/a/f/KC_a;

    if-nez v0, :cond_0

    invoke-static {}, Lcom/kamcord/a/a/f/KC_b;->c()Z

    move-result v0

    if-eqz v0, :cond_1

    new-instance v0, Lcom/kamcord/a/a/f/KC_b;

    invoke-direct {v0}, Lcom/kamcord/a/a/f/KC_b;-><init>()V

    :goto_0
    sput-object v0, Lcom/kamcord/a/a/f/KC_a;->a:Lcom/kamcord/a/a/f/KC_a;

    :cond_0
    sget-object v0, Lcom/kamcord/a/a/f/KC_a;->a:Lcom/kamcord/a/a/f/KC_a;
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit v1

    return-object v0

    :cond_1
    :try_start_1
    new-instance v0, Lcom/kamcord/a/a/f/KC_c;

    invoke-direct {v0}, Lcom/kamcord/a/a/f/KC_c;-><init>()V
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method


# virtual methods
.method public abstract a([B)Ljava/lang/String;
.end method

.method public abstract b()Ljava/lang/String;
.end method
